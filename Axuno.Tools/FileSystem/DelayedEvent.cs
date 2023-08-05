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

    public virtual bool IsDuplicate(object obj)
    {
        if (!(obj is DelayedEvent delayedEvent))
            return false;

        var allEventArgs = Args;
        var renamedEventArgs = Args as RenamedEventArgs;

        var allDelayedEventArgs = delayedEvent.Args;
        var delayedRenamedEventArgs = delayedEvent.Args as RenamedEventArgs;
        // The events are equal only if they are of the same type and both have all properties equal.        
        // We also eliminate Changed events that follow recent Created events
        // because many apps create new files by creating an empty file and then
        // update the file with the file content.
        return (allEventArgs.ChangeType == allDelayedEventArgs.ChangeType
                && allEventArgs.FullPath == allDelayedEventArgs.FullPath &&
                allEventArgs.Name == allDelayedEventArgs.Name) &&
               ((renamedEventArgs == null && delayedRenamedEventArgs == null) || (renamedEventArgs != null &&
                   delayedRenamedEventArgs != null &&
                   renamedEventArgs.OldFullPath == delayedRenamedEventArgs.OldFullPath &&
                   renamedEventArgs.OldName == delayedRenamedEventArgs.OldName)) ||
               (allEventArgs.ChangeType == WatcherChangeTypes.Created
                && allDelayedEventArgs.ChangeType == WatcherChangeTypes.Changed
                && allEventArgs.FullPath == allDelayedEventArgs.FullPath &&
                allEventArgs.Name == allDelayedEventArgs.Name);
    }
}
