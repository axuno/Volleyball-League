namespace League.Emailing;

/// <summary>
/// Exception thrown when a Razor view cannot be found.
/// </summary>
public class ViewNotFoundException : InvalidOperationException
{
    public ViewNotFoundException(string message) : base(message)
    {
    }

    public ViewNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
