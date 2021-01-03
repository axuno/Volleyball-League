
namespace League.TextTemplatingModule
{
    /// <summary>
    /// Determines how rendering errors are handled.
    /// </summary>
    public enum RenderErrorAction
    {
        /// <summary>Throws an exception.</summary>
        ThrowError,
        /// <summary>Includes an error message in the output.</summary>
        OutputErrorInResult,
        /// <summary>Leaves invalid tokens unmodified in the text.</summary>
        MaintainToken,
        /// <summary>Leaves the output empty.</summary>
        LeaveEmpty
    }
}
