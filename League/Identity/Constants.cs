﻿using System.Reflection;

namespace League.Identity;

/// <summary>
/// Class for <see cref="League.Identity"/> constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Constants which are used as identity roles.
    /// </summary>
    public static class RoleName
    {
        /// <summary>
        /// Role name for <see ref="Player"/>s.
        /// </summary>
        public const string Player = nameof(Player);
        /// <summary>
        /// Role name for <see ref="TeamManager"/>s.
        /// </summary>
        public const string TeamManager = nameof(TeamManager);
        /// <summary>
        /// Role name for <see ref="TournamentManager"/>s.
        /// </summary>           
        public const string TournamentManager = nameof(TournamentManager);
        /// <summary>
        /// Role name for <see ref="SystemManager"/>s.
        /// </summary>  
        public const string SystemManager = nameof(SystemManager);

        /// <summary>
        /// Get the roles which are implicitly defined with a relation of
        /// a user to teams.
        /// </summary>
        /// <returns>Returns the (forbidden) roles which are implicitly defined with a relation of a user to teams.</returns>
        public static IEnumerable<string> GetTeamRelatedRoles()
        {
            return new[] {Player, TeamManager};
        }

        /// <summary>
        /// Get the values of all constants in this class.
        /// </summary>
        /// <returns>Returns the values of all constants in this class.</returns>
        public static IEnumerable<T?> GetAllRoleValues<T>()
        {
            return GetAllValues<T>(typeof(RoleName));
        }

        /// <summary>
        /// Get the names of all constants in this class.
        /// </summary>
        /// <returns>Returns the names of all constants in this class.</returns>
        public static IEnumerable<string?> GetAllRoleNames()
        {
            return GetAllNames(typeof(RoleName));
        }
    }

    /// <summary>
    /// Constants which are used as identity roles.
    /// </summary>
    public static class ClaimType
    {
        public const string ManagesTeam = nameof(ManagesTeam);
        public const string PlaysInTeam = nameof(PlaysInTeam);
        public const string ImpersonatedByUser = nameof(ImpersonatedByUser);
#if DEBUG
        /// <summary>
        /// Only used for unit tests. This special claim type is not implemented and will throw when used.
        /// </summary>
        public const string NotImplementedClaim = nameof(NotImplementedClaim);
#endif

        /// <summary>
        /// Get the claims which are implicitly defined with a relation of
        /// a user to teams.
        /// </summary>
        /// <returns>Returns the claims which are implicitly defined with a relation of a user to teams.</returns>
        public static IEnumerable<string> GetTeamRelatedClaimTypes()
        {
            return new[] { ManagesTeam, PlaysInTeam
#if DEBUG
                , NotImplementedClaim
#endif
            };
        }

        /// <summary>
        /// Get the programmatic claims which are never stored in the database.
        /// </summary>
        /// <returns>Returns programmatic claims which are never stored in the database.</returns>
        public static IEnumerable<string> GetProgrammaticClaimTypes()
        {
            return new[] { ImpersonatedByUser };
        }

        /// <summary>
        /// Get the values of type T of all constants in this class.
        /// </summary>
        /// <returns>Returns the values of type T of all constants in this class.</returns>
        public static IEnumerable<T?> GetAllClaimTypeValues<T>()
        {
            return Constants.GetAllValues<T>(typeof(ClaimType));
        }

        /// <summary>
        /// Get the names of all constants in this class.
        /// </summary>
        /// <returns>Returns the names of all constants in this class.</returns>
        public static IEnumerable<string?> GetAllClaimTypeNames()
        {
            return Constants.GetAllNames(typeof(ClaimType));
        }
    }

    /// <summary>
    /// Get the values of type T of all constants in this class.
    /// </summary>
    /// <returns>Returns the values of type T of all constants in this class.</returns>
    private static IEnumerable<T?> GetAllValues<T>(IReflect type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(x => (T?)x.GetRawConstantValue())
            .ToList();
    }

    /// <summary>
    /// Get the names of all constants in this class.
    /// </summary>
    /// <returns>Returns the names of all constants in this class.</returns>
    private static IEnumerable<string?> GetAllNames(IReflect type)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

        var fields = type.GetFields(flags).Where(f => f.IsLiteral);
        foreach (var fieldInfo in fields)
        {
            yield return fieldInfo.GetValue(null)?.ToString();
        }
    }
}
