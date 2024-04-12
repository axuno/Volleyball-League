namespace Axuno.Tools.FileSystem;

/// <summary>
/// The <see cref="DelayedEvent"/> class  wraps <see cref="FileSystemEventArgs"/> and <see cref="RenamedEventArgs"/> objects
/// and detects duplicate events.
/// </summary>
public class DelayedEvent
{
    public DelayedEvent(FileSystemEventArgs args)
    {
        Delayed = false;
        Args = args;
    }


    /// <summary>
    /// If <see langword="true"/>, only delayed events that are unique will be fired.
    /// </summary>
    public bool Delayed { get; set; }

    /// <summary>
    /// Gets the <see cref="FileSystemEventArgs"/>.
    /// </summary>
    public FileSystemEventArgs Args { get; }

    public virtual bool IsDuplicate(object? obj)
    {
        if (obj is not DelayedEvent delayedEvent)
            return false;

        var renamedEventArgs = Args as RenamedEventArgs;

        var allDelayedEventArgs = delayedEvent.Args;
        var delayedRenamedEventArgs = delayedEvent.Args as RenamedEventArgs;
        // The events are equal only if they are of the same type and both have all properties equal.        
        // We also eliminate Changed events that follow recent Created events
        // because many apps create new files by creating an empty file and then
        // update the file with the file content.
        return (Args.ChangeType == allDelayedEventArgs.ChangeType
                && Args.FullPath == allDelayedEventArgs.FullPath &&
                Args.Name == allDelayedEventArgs.Name) &&
               ((renamedEventArgs == null && delayedRenamedEventArgs == null) || (renamedEventArgs != null &&
                   delayedRenamedEventArgs != null &&
                   renamedEventArgs.OldFullPath == delayedRenamedEventArgs.OldFullPath &&
                   renamedEventArgs.OldName == delayedRenamedEventArgs.OldName)) ||
               (Args.ChangeType == WatcherChangeTypes.Created
                && allDelayedEventArgs.ChangeType == WatcherChangeTypes.Changed
                && Args.FullPath == allDelayedEventArgs.FullPath &&
                Args.Name == allDelayedEventArgs.Name);
    }
}
