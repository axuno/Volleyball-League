using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;

namespace Axuno.Tools.FileSystem;

/// <summary>
/// This class wraps a <see cref="FileSystemWatcher"/> object. The class is not derived
/// from <see cref="FileSystemWatcher"/> because most of the FileSystemWatcher methods
/// are not virtual. The class was designed to resemble the <see cref="FileSystemWatcher"/> class
/// as much as possible so that you can use <see cref="DelayedFileSystemWatcher"/> instead
/// of <see cref="FileSystemWatcher"/> objects.
/// <see cref="DelayedFileSystemWatcher"/> will capture all events from the <see cref="FileSystemWatcher"/> object.
/// The captured events will be delayed by at least <see cref="DelayedFileSystemWatcher.ConsolidationInterval"/> milliseconds in order
/// to be able to eliminate duplicate events. When duplicate events are found, the last event
/// is dropped and the first event is fired (the reverse is not recommended because it could
/// cause some events not be fired at all since the last event will become the first event and
/// it won't fire if a new similar event arrives immediately afterwards).
/// </summary>
public class DelayedFileSystemWatcher : IDisposable
{
    private readonly FileSystemWatcher _fileSystemWatcher;

    /// <summary>
    /// Lock order is _enterThread, _events.SyncRoot
    /// </summary>
    private readonly object _enterThread = new (); // Only one timer event is processed at any given moment

    /// <summary>
    /// Stores the events fired by <see cref="FileSystemWatcher"/>.
    /// </summary>
    private ArrayList _events = new ();

    private int _consolidationInterval = 1000; // initial value in milliseconds
    private readonly System.Timers.Timer _timer;

    #region Delegate to FileSystemWatcher

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedFileSystemWatcher"/> class.
    /// </summary>
    public DelayedFileSystemWatcher()
    {
        _fileSystemWatcher = new FileSystemWatcher();
        Initialize(out _timer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedFileSystemWatcher"/> class, given the specified directory to monitor.
    /// </summary>
    /// <param name="path">The directory to monitor.</param>
    public DelayedFileSystemWatcher(string path)
    {
        _fileSystemWatcher = new FileSystemWatcher(path);
        Initialize(out _timer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
    /// </summary>
    /// <param name="path">The directory to monitor.</param>
    /// <param name="typeFilter">The type of files to monitor.</param>
    public DelayedFileSystemWatcher(string path, string typeFilter)
    {
        _fileSystemWatcher = new FileSystemWatcher(path, typeFilter);
        Initialize(out _timer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedFileSystemWatcher"/> class, given the specified directory and type of files to monitor.
    /// </summary>
    /// <param name="path">The directory to monitor.</param>
    /// <param name="typeFilters">The types of files to monitor, e.g. new[] {"*.yml", "*.yaml"}</param>
    public DelayedFileSystemWatcher(string path, IEnumerable<string> typeFilters)
    {
        _fileSystemWatcher = new FileSystemWatcher(path);
        foreach (var filter in typeFilters) _fileSystemWatcher.Filters.Add(filter);
        
        Initialize(out _timer);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the component is enabled.
    /// </summary>
    /// <value>true if the component is enabled; otherwise, false. The default is false. If you are using the component on a designer in Visual Studio 2005, the default is true.</value>
    public bool EnableRaisingEvents
    {
        get => _fileSystemWatcher.EnableRaisingEvents;
        set
        {
            _fileSystemWatcher.EnableRaisingEvents = value;
            if (value)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _events.Clear();
            }
        }
    }

    /// <summary>
    /// Gets or sets the filter string, used to determine what files are monitored in a directory.
    /// </summary>
    /// <value>The filter string. The default is "*.*" (watches all files).</value>
    public string Filter
    {
        get => _fileSystemWatcher.Filter;
        set => _fileSystemWatcher.Filter = value;
    }

    /// <summary>
    /// Gets the filters collection, used to determine what files are monitored in a directory.
    /// </summary>
    /// <value>The collection of filter strings. The default is "*.*" (watches all files).</value>
    public Collection<string> Filters => _fileSystemWatcher.Filters;

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
    /// </summary>
    /// <value>true if you want to monitor subdirectories; otherwise, false. The default is false.</value>
    public bool IncludeSubdirectories
    {
        get => _fileSystemWatcher.IncludeSubdirectories;
        set => _fileSystemWatcher.IncludeSubdirectories = value;
    }

    /// <summary>
    /// Gets or sets the size of the internal buffer.
    /// </summary>
    /// <value>The internal buffer size. The default is 8192 (8K).</value>
    public int InternalBufferSize
    {
        get => _fileSystemWatcher.InternalBufferSize;
        set => _fileSystemWatcher.InternalBufferSize = value;
    }

    /// <summary>
    /// Gets or sets the type of changes to watch for.
    /// </summary>
    /// <value>One of the System.IO.NotifyFilters values. The default is the bitwise OR combination of LastWrite, FileName, and DirectoryName.</value>
    /// <exception cref="System.ArgumentException">The value is not a valid bitwise OR combination of the System.IO.NotifyFilters values.</exception>
    public NotifyFilters NotifyFilter
    {
        get => _fileSystemWatcher.NotifyFilter;
        set => _fileSystemWatcher.NotifyFilter = value;
    }

    /// <summary>
    /// Gets or sets the path of the directory to watch.
    /// </summary>
    /// <value>The path to monitor. The default is an empty string ("").</value>
    /// <exception cref="System.ArgumentException">The specified path contains wildcard characters.-or- The specified path contains invalid path characters.</exception>
    public string Path
    {
        get => _fileSystemWatcher.Path;
        set => _fileSystemWatcher.Path = value;
    }

    /// <summary>
    /// Gets or sets the object used to marshal the event handler calls issued as a result of a directory change.
    /// </summary>
    /// <value>The <see cref="System.ComponentModel.ISynchronizeInvoke"/> that represents the object used to marshal the event handler calls issued as a result of a directory change. The default is <see langword="null"/>.</value>
    public ISynchronizeInvoke? SynchronizingObject
    {
        get => _fileSystemWatcher.SynchronizingObject;
        set => _fileSystemWatcher.SynchronizingObject = value;
    }

    /// <summary>
    /// Occurs when a file or directory in the specified <see cref="FileSystemWatcher.Path"/> is changed.
    /// </summary>
    public event FileSystemEventHandler? Changed;

    /// <summary>
    /// Occurs when a file or directory in the specified <see cref="FileSystemWatcher.Path"/> is created.
    /// </summary>
    public event FileSystemEventHandler? Created;

    /// <summary>
    /// Occurs when a file or directory in the specified <see cref="FileSystemWatcher.Path"/> is deleted.
    /// </summary>
    public event FileSystemEventHandler? Deleted;

    /// <summary>
    /// Occurs when the internal buffer overflows.
    /// </summary>
    public event ErrorEventHandler? Error;

    /// <summary>
    /// Occurs when a file or directory in the specified <see cref="FileSystemWatcher.Path"/> is renamed.
    /// </summary>
    public event RenamedEventHandler? Renamed;

    /// <summary>
    /// Begins the initialization of a <see cref="FileSystemWatcher"/> used on a form or used by another component. The initialization occurs at run time.
    /// </summary>
    public void BeginInit()
    {
        _fileSystemWatcher.BeginInit();
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="FileSystemWatcher"/> and optionally releases the managed resources.
    /// </summary>
    public void Dispose()
    {
        _fileSystemWatcher.Dispose();
        _timer.Dispose();
    }

    /// <summary>
    /// Ends the initialization of a <see cref="FileSystemWatcher"/> used on a form or used by another component. The initialization occurs at run time.
    /// </summary>
    public void EndInit()
    {
        _fileSystemWatcher.EndInit();
    }

    /// <summary>
    /// Raises the <see cref="FileSystemWatcher.Changed"/> event.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    protected void OnChanged(FileSystemEventArgs e)
    {
        Changed?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FileSystemWatcher.Created"/> event.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    protected void OnCreated(FileSystemEventArgs e)
    {
        Created?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FileSystemWatcher.Deleted"/> event.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    protected void OnDeleted(FileSystemEventArgs e)
    {
        Deleted?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FileSystemWatcher.Error"/> event.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    protected void OnError(ErrorEventArgs e)
    {
        Error?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="FileSystemWatcher.Renamed"/> event.
    /// </summary>
    /// <param name="e">A <see cref="FileSystemEventArgs"/> that contains the event data.</param>
    protected void OnRenamed(RenamedEventArgs e)
    {
        Renamed?.Invoke(this, e);
    }

    /// <summary>
    /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor.
    /// </summary>
    /// <param name="changeType">The <see cref="System.IO.WatcherChangeTypes" /> to watch for.</param>
    /// <returns>A <see cref="WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
    public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor
    /// and the time (in milliseconds) to wait before timing out.
    /// </summary>
    /// <param name="changeType">The System.IO.WatcherChangeTypes to watch for.</param>
    /// <param name="timeout">The time (in milliseconds) to wait before timing out.</param>
    /// <returns>A <see cref="System.IO.WaitForChangedResult"/> that contains specific information on the change that occurred.</returns>
    public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Implementation

    private void Initialize(out System.Timers.Timer timer)
    {
        _events = ArrayList.Synchronized(new ArrayList(32));
        _fileSystemWatcher.Changed += FileSystemEventHandler;
        _fileSystemWatcher.Created += FileSystemEventHandler;
        _fileSystemWatcher.Deleted += FileSystemEventHandler;
        _fileSystemWatcher.Error += ErrorEventHandler;
        _fileSystemWatcher.Renamed += RenamedEventHandler;

        timer = new System.Timers.Timer(_consolidationInterval);
        timer.Elapsed += ElapsedEventHandler;
        timer.AutoReset = true;
        timer.Enabled = _fileSystemWatcher.EnableRaisingEvents;
    }

    private void FileSystemEventHandler(object sender, FileSystemEventArgs e)
    {
        _events.Add(new DelayedEvent(e));
    }

    private void ErrorEventHandler(object sender, ErrorEventArgs e)
    {
        OnError(e);
    }

    private void RenamedEventHandler(object sender, RenamedEventArgs e)
    {
        _events.Add(new DelayedEvent(e));
    }

    private void ElapsedEventHandler(object? sender, ElapsedEventArgs e)
    {
        // We don't fire the events inside the lock.
        // We will queue them here until the code exits the locks.
        Queue? eventsToBeFired = null;
        if (Monitor.TryEnter(_enterThread))
        {
            // Only one thread at a time is processing the events                
            try
            {
                eventsToBeFired = new Queue(32);
                // Lock the collection while processing the events
                lock (_events.SyncRoot)
                {
                    for (var i = 0; i < _events.Count; i++)
                    {
                        var current = _events[i] as DelayedEvent;
                        if (current is { Delayed: true })
                        {
                            // This event has been delayed already so we can fire it
                            // We just need to remove any duplicates
                            for (var j = i + 1; j < _events.Count; j++)
                            {
                                if (current.IsDuplicate(_events[j]!))
                                {
                                    // Removing later duplicates
                                    _events.RemoveAt(j);
                                    j--; // Don't skip next event
                                }
                            }

                            var raiseEvent = true;
                            if (current.Args.ChangeType is WatcherChangeTypes.Created or WatcherChangeTypes.Changed)
                            {
                                // check if the file can be opened for reading (i.e. copying has been completed)
                                FileStream? stream = null;
                                try
                                {
                                    stream = File.Open(current.Args.FullPath, FileMode.Open, FileAccess.Read,
                                        FileShare.None);
                                }
                                catch (IOException)
                                {
                                    raiseEvent = false;
                                }
                                finally
                                {
                                    stream?.Close();
                                }
                            }

                            if (!raiseEvent) continue;

                            // Add the event to the list of events to be fired
                            eventsToBeFired.Enqueue(current);
                            // Remove it from the current list
                            _events.RemoveAt(i);
                            i--; // meaning: the next event won't be skipped
                        }
                        else
                        {
                            // This event was not delayed yet, so we will delay processing
                            // of this event for at least one timer interval
                            if (current != null) current.Delayed = true;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(_enterThread);
            }
        }
        // else - this timer event was skipped, processing will happen during the next timer event

        // Now fire all the events if any events are in eventsToBeFired
        if (eventsToBeFired != null) RaiseEvents(eventsToBeFired);
    }

    /// <summary>
    /// Gets or sets the interval in milliseconds, after which events will be fired.
    /// </summary>
    public int ConsolidationInterval
    {
        get => _consolidationInterval;

        set
        {
            _consolidationInterval = value;
            _timer.Interval = value; // existing timer
        }
    }

    private void RaiseEvents(Queue? queue)
    {
        if (queue is not { Count: > 0 }) return;

        while (queue.Count > 0)
        {
            if (queue.Dequeue() is DelayedEvent dequeuedInvent)
                switch (dequeuedInvent.Args.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        OnChanged(dequeuedInvent.Args);
                        break;
                    case WatcherChangeTypes.Created:
                        OnCreated(dequeuedInvent.Args);
                        break;
                    case WatcherChangeTypes.Deleted:
                        OnDeleted(dequeuedInvent.Args);
                        break;
                    case WatcherChangeTypes.Renamed:
                        if (dequeuedInvent.Args is RenamedEventArgs renamed) OnRenamed(renamed);
                        break;
                    case WatcherChangeTypes.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
    }

    #endregion
}
