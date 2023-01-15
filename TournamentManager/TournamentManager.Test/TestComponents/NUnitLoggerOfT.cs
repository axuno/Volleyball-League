using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TournamentManager.Tests.TestComponents;

/// <summary>
/// Simple generic <see cref="ILogger"/> implementation for a unit test logger,
/// which will write log messages to the NUnit output window
/// </summary>
public class NUnitLogger<T> : NUnitLogger, ILogger<T>
{
    /// <summary>
    /// CTOR.
    /// </summary>
    public NUnitLogger() : base(typeof(T).FullName)
    { }

    /// <summary>
    /// Gets a new instance of an <see cref="NUnitLogger"/>.
    /// </summary>
    /// <returns>Returns a new instance of an <see cref="NUnitLogger"/>.</returns>
    public new ILogger<T> Create()
    {
        return new NUnitLogger<T>();
    }
}