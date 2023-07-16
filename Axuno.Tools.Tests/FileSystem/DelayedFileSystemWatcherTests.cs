using NUnit.Framework;

namespace Axuno.Tools.FileSystem.Tests
{
    [TestFixture]
    public class DelayedFileSystemWatcherTests
    {
        private string? _directoryToWatch;

        private FileSystemWatcher GetFileSystemWatcher()
        {
            _directoryToWatch ??= CreateTempPathFolder();
            return new FileSystemWatcher(_directoryToWatch)
            {
                IncludeSubdirectories = false, EnableRaisingEvents = true
            };
        }

        private DelayedFileSystemWatcher GetDelayedFileSystemWatcher(int filterType)
        {
            _directoryToWatch ??= CreateTempPathFolder();
            // use filterType in order to cover different CTORs
            switch (filterType)
            {
                default:
                    return new DelayedFileSystemWatcher
                    {
                        Path = _directoryToWatch, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName,
                        IncludeSubdirectories = false, EnableRaisingEvents = true, ConsolidationInterval = 1,
                        Filter = "*.*"
                    };
                case 2:
                    var watcher = new DelayedFileSystemWatcher(_directoryToWatch)
                    {
                        IncludeSubdirectories = false, EnableRaisingEvents = true, ConsolidationInterval = 1,
                    };
                    watcher.Filters.Add("*.*");
                    return watcher;
                case 3:
                    return new DelayedFileSystemWatcher(_directoryToWatch, "*.*")
                    {
                        IncludeSubdirectories = false, EnableRaisingEvents = true, ConsolidationInterval = 1
                    };
                case 4:
                    return new DelayedFileSystemWatcher(_directoryToWatch, new [] {"*.*"})
                    {
                        IncludeSubdirectories = false, EnableRaisingEvents = true, ConsolidationInterval = 1
                    };
            }
        }

        [Test]
        public async Task CreateFile()
        {
            using var watcher = GetFileSystemWatcher();
            using var delayedWatcher = GetDelayedFileSystemWatcher(1);

            var delayedEventCount = 0;
            var watcherEventCount = 0;

            delayedWatcher.Created += WatcherDelegate;
            watcher.Created += WatcherDelegate;

            void WatcherDelegate(object sender, FileSystemEventArgs e)
            {
                if (sender.GetType() == typeof(DelayedFileSystemWatcher))
                {
                    delayedEventCount++;
                }
                else
                {
                    watcherEventCount++;
                }
            }
            
            await File.WriteAllTextAsync(Path.Combine(_directoryToWatch!, Path.GetRandomFileName()), "Any text");
            await Task.Delay(100); // must be higher than ConsolidationInterval

            Assert.That(watcherEventCount, Is.EqualTo(1));
            Assert.That(delayedEventCount, Is.EqualTo(1));
        }

        [Test]
        public async Task RenameFile()
        {
            using var watcher = GetFileSystemWatcher();
            using var delayedWatcher = GetDelayedFileSystemWatcher(2);

            var delayedEventCount = 0;
            var watcherEventCount = 0;

            delayedWatcher.Renamed += WatcherDelegate;
            watcher.Renamed += WatcherDelegate;

            void WatcherDelegate(object sender, FileSystemEventArgs e)
            {
                if (sender.GetType() == typeof(DelayedFileSystemWatcher))
                {
                    delayedEventCount++;
                }
                else
                {
                    watcherEventCount++;
                }
            }

            var filename = Path.Combine(_directoryToWatch!, Path.GetRandomFileName());

            File.Create(filename).Close();
            File.Move(filename, filename + ".renamed");
            
            await Task.Delay(100); // must be higher than ConsolidationInterval

            Assert.That(watcherEventCount, Is.EqualTo(1));
            Assert.That(delayedEventCount, Is.EqualTo(1));
        }

        [Test]
        public async Task MultipleRenameFile()
        {
            using var watcher = GetFileSystemWatcher();
            using var delayedWatcher = GetDelayedFileSystemWatcher(3);
            delayedWatcher.ConsolidationInterval = 100;

            var delayedEventCount = 0;
            var watcherEventCount = 0;

            delayedWatcher.Renamed += WatcherDelegate;
            watcher.Renamed += WatcherDelegate;

            void WatcherDelegate(object sender, FileSystemEventArgs e)
            {
                if (sender.GetType() == typeof(DelayedFileSystemWatcher))
                {
                    delayedEventCount++;
                }
                else
                {
                    watcherEventCount++;
                }
            }

            var filename1 = Path.Combine(_directoryToWatch!, Path.GetRandomFileName());
            var filename2 = filename1 + ".renamed";
            
            File.Create(filename1).Close();

            File.Move(filename1, filename2);
            File.Move(filename2, filename1);
            File.Move(filename1, filename2);
            File.Move(filename2, filename1);
            File.Move(filename1, filename2);
            File.Move(filename2, filename1);

            await Task.Delay(500); // must be higher than ConsolidationInterval

            Assert.That(watcherEventCount, Is.EqualTo(6));
            // Multiple events of the same type are consolidated per file
            Assert.That(delayedEventCount, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteFile()
        {
            using var watcher = GetFileSystemWatcher();
            using var delayedWatcher = GetDelayedFileSystemWatcher(4);

            var delayedEventCount = 0;
            var watcherEventCount = 0;

            delayedWatcher.Deleted += WatcherDelegate;
            watcher.Deleted += WatcherDelegate;

            void WatcherDelegate(object sender, FileSystemEventArgs e)
            {
                if (sender.GetType() == typeof(DelayedFileSystemWatcher))
                {
                    delayedEventCount++;
                }
                else
                {
                    watcherEventCount++;
                }
            }

            delayedWatcher.EnableRaisingEvents = false;
            var filename = Path.Combine(_directoryToWatch!, Path.GetRandomFileName());

            File.Create(filename).Close();
            // Enable after file is created.
            delayedWatcher.EnableRaisingEvents = true;

            File.Delete(filename);
            await Task.Delay(100); // must be higher than ConsolidationInterval

            Assert.That(watcherEventCount, Is.EqualTo(1));
            Assert.That(delayedEventCount, Is.EqualTo(1));
        }

        [Test]
        public async Task MultipleChangeFile()
        {
            using var watcher = GetFileSystemWatcher();
            using var delayedWatcher = GetDelayedFileSystemWatcher(1);
            delayedWatcher.ConsolidationInterval = 200;
            delayedWatcher.EnableRaisingEvents = false;

            var delayedEventCount = 0;
            var watcherEventCount = 0;

            delayedWatcher.Changed += WatcherDelegate;
            watcher.Changed += WatcherDelegate;

            void WatcherDelegate(object sender, FileSystemEventArgs e)
            {
                if (sender.GetType() == typeof(DelayedFileSystemWatcher))
                {
                    delayedEventCount++;
                }
                else
                {
                    watcherEventCount++;
                }
            }

            var filename = Path.Combine(_directoryToWatch!, Path.GetRandomFileName());
            File.Create(filename).Close();
            // Must be enabled after the file is created.
            // Otherwise, the Created event would fire.
            delayedWatcher.EnableRaisingEvents = true;

            await File.AppendAllTextAsync(filename, "1");
            await File.AppendAllTextAsync(filename, "2");
            await File.AppendAllTextAsync(filename, "3");
            await Task.Delay(500); // must be higher than ConsolidationInterval

            Assert.That(watcherEventCount, Is.EqualTo(3));
            Assert.That(delayedEventCount, Is.EqualTo(1));
        }

        private static string CreateTempPathFolder()
        {
            // Create folder in TempPath
            var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            if (_directoryToWatch is null) return;
            Directory.Delete(_directoryToWatch, true);
            _directoryToWatch = null;
        }
    }
}
