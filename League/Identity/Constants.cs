using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace League.Identity
{
    public class Constants
    {
        /// <summary>
        /// Constants which are used as identity roles.
        /// </summary>
        public class RoleName
        {
            public const string Player = nameof(Player);
            public const string TeamManager = nameof(TeamManager);
            public const string TournamentManager = nameof(TournamentManager);
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
            public static IEnumerable<T> GetAllValues<T>()
            {
                return Constants.GetAllValues<T>(typeof(RoleName));
            }

            /// <summary>
            /// Get the names of all constants in this class.
            /// </summary>
            /// <returns>Returns the names of all constants in this class.</returns>
            public static IEnumerable<string> GetAllNames()
            {
                return Constants.GetAllNames(typeof(RoleName));
            }
        }

        /// <summary>
        /// Constants which are used as identity roles.
        /// </summary>
        public class ClaimType
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
            public static IEnumerable<T> GetAllValues<T>()
            {
                return Constants.GetAllValues<T>(typeof(ClaimType));
            }

            /// <summary>
            /// Get the names of all constants in this class.
            /// </summary>
            /// <returns>Returns the names of all constants in this class.</returns>
            public static IEnumerable<string> GetAllNames()
            {
                return Constants.GetAllNames(typeof(ClaimType));
            }
        }

        /// <summary>
        /// Get the values of type T of all constants in this class.
        /// </summary>
        /// <returns>Returns the values of type T of all constants in this class.</returns>
        private static IEnumerable<T> GetAllValues<T>(IReflect type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        /// <summary>
        /// Get the names of all constants in this class.
        /// </summary>
        /// <returns>Returns the names of all constants in this class.</returns>
        private static IEnumerable<string> GetAllNames(IReflect type)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

            var fields = type.GetFields(flags).Where(f => f.IsLiteral);
            foreach (var fieldInfo in fields)
            {
                yield return fieldInfo.GetValue(null)?.ToString();
            }
        }
    }
}
