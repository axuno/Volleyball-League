//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

namespace TournamentManager.HtmlToPdfConverter;

/// <summary>
/// Specifies to kind of browser to use in <see cref="HtmlToPdfConverter"/>.
/// </summary>
public enum BrowserKind
{
    /// <summary>Chrome.</summary>
    Chrome,
    /// <summary>Firefox.</summary>
    Firefox,
    /// <summary>Chromium.</summary>
    Chromium,
    /// <summary>Chrome headless shell.</summary>
    ChromeHeadlessShell
}
