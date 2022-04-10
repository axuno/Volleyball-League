using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask
{
    public class ConcurrentBackgroundQueueService : BackgroundService
    {
        private readonly ILogger<ConcurrentBackgroundQueueService> _logger;
        private readonly ManualResetEvent _resetEvent = new(true);
        private readonly object _locker = new();
        private int _concurrentTaskCount;
        
        public ConcurrentBackgroundQueueService(IBackgroundQueue taskQueue,
            ILogger<ConcurrentBackgroundQueueService> logger, IOptions<ConcurrentBackgroundQueueServiceConfig> config)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            Config = config.Value ?? new ConcurrentBackgroundQueueServiceConfig();
        }

        /// <summary>
        /// Gets the configuration for this service.
        /// </summary>
        public ConcurrentBackgroundQueueServiceConfig Config { get; }

        /// <summary>
        /// Gets the <see cref="IBackgroundQueue"/> of this service.
        /// </summary>
        public IBackgroundQueue TaskQueue { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            lock (_locker)
            {
                _resetEvent.WaitOne();
            }

            var taskListReference = new Queue<IBackgroundTask>();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(Config.PollQueueDelay, cancellationToken);

                    if (_concurrentTaskCount < Config.MaxConcurrentCount && TaskQueue.Count > 0)
                    {
                        Interlocked.Increment(ref _concurrentTaskCount);
                        _logger.LogDebug("Num of tasks: {concurrentTaskCount}", _concurrentTaskCount);
                        taskListReference.Enqueue(TaskQueue.DequeueTask());
                    }
                    else
                    {
                        var taskChunk = new List<Task>();
                        while (taskListReference.Count > 0)
                        {
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                // The service shall only be cancelled when the app shuts down
                                using (var taskCancellation = new CancellationTokenSource())
                                using (var combinedCancellation =
                                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                                        taskCancellation.Token))
                                {
                                    var t = TaskQueue.RunTaskAsync(taskListReference.Dequeue(),
                                        combinedCancellation.Token);
                                    if (t.Exception != null) throw t.Exception;
                                    taskChunk.Add(t);
                                }

                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Error occurred executing TaskItem.");
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _concurrentTaskCount);
                            }
                        }

                        if (!taskChunk.Any()) continue;

                        // Task.WhenAll will not throw all exceptions when it encounters them.
                        // Instead, it adds them to an AggregateException, that must be
                        // checked at the end of waiting for the tasks to complete
                        Task allTasks = null;
                        try
                        {
                            allTasks = Task.WhenAll(taskChunk);
                            // re-throws an AggregateException if one exists
                            // after waiting for the tasks to complete
                            allTasks.Wait(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Task chunk exception");
                            if (allTasks?.Exception != null)
                            {
                                _logger.LogError(allTasks.Exception, "Task chunk aggregate exception");
                            }
                        }
                        finally
                        {
                            taskListReference.Clear();
                        }
                    }
                }
            }
            catch (Exception e) when (e is TaskCanceledException)
            {
                _logger.LogError(e, $"{nameof(ConcurrentBackgroundQueueService)} was canceled.");
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{nameof(ConcurrentBackgroundQueueService)} failed.");
                TaskQueue = null; // we can't process the queue any more
            }
            finally
            {
                lock (_locker)
                {
                    _resetEvent.Reset();
                    _resetEvent.Set();
                }
            }
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{nameof(ConcurrentBackgroundQueueService)} is stopping.");
            await base.StopAsync(cancellationToken);
            _logger.LogDebug($"{nameof(ConcurrentBackgroundQueueService)} stopped.");
        }
    }
}
