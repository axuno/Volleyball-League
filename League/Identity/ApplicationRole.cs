using System;
using Microsoft.AspNetCore.Identity;

namespace League.Identity
{
    /// <summary>
    /// Used with AddIdentity&lt;ApplicationUser, ApplicationRole&gt;
    /// </summary>
    /// <remarks>This class does not inherit from IdentityRole&lt;long&gt;</remarks>
    public class ApplicationRole
    {
        public long Id { get; set; } = -1;
        public string Name { get; set; }
    }
}
