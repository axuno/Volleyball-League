using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data
{
    /// <summary>
    /// Class for User and Manager related data selections
    /// </summary>
    public class UserRepository
    {
        private static readonly ILogger _logger = AppLogging.CreateLogger<UserRepository>();
        private readonly IDbContext _dbContext;

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Gets a User or the Manager subtype.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a User or the Manager subtype, or null if not found.</returns>
        public virtual async Task<UserEntity> GetLoginUserAsync(long id, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                var result = await (from u in metaData.User
                    where
                        u.Id == id
                    select u).WithPath(new IPathEdge[]
                {
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathManagerOfTeams),
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathPlayerInTeams)
                }).ExecuteAsync<EntityCollection<UserEntity>>(cancellationToken);

                da.CloseConnection();
                return result.FirstOrDefault();
            }
        }

        public virtual async Task<UserEntity> GetLoginUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            email = email.ToLowerInvariant().Trim();
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                var result = await (from u in metaData.User
                    where
                        u.Email.ToLower() == email
                    select u).WithPath(new IPathEdge[]
                {
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathManagerOfTeams),
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathPlayerInTeams)
                }).ExecuteAsync<EntityCollection<UserEntity>>(cancellationToken);
                da.CloseConnection();
                return result.FirstOrDefault();
            }
        }

        public virtual async Task<UserEntity> GetLoginUserByEmail2Async(string email, CancellationToken cancellationToken)
        {
            email = email.ToLowerInvariant().Trim();
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                var result = await (from u in metaData.User
                    where
                        u.Email2.ToLower() == email
                    select u).WithPath(new IPathEdge[]
                {
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathManagerOfTeams),
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathPlayerInTeams)
                }).ExecuteAsync<EntityCollection<UserEntity>>(cancellationToken);
                da.CloseConnection();
                return result.FirstOrDefault();
            }
        }

        public virtual async Task<UserEntity> GetLoginUserByUserNameAsync(string userName, CancellationToken cancellationToken)
        {
            userName = userName.ToLowerInvariant().Trim();
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                var result = await (from u in metaData.User
                    where
                        u.UserName.ToLower() == userName
                    select u).WithPath(new IPathEdge[]
                {
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathManagerOfTeams),
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathPlayerInTeams)
                }).ExecuteAsync<EntityCollection<UserEntity>>(cancellationToken);
                da.CloseConnection();
                return result.FirstOrDefault();
            }
        }

        public virtual async Task<UserEntity> GetLoginUserByGuidAsync(string guid, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                var result = await (from u in metaData.User
                    where
                        u.Guid == guid
                    select u).WithPath(new IPathEdge[]
                {
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathManagerOfTeams),
                    new PathEdge<UserEntity>(UserEntity.PrefetchPathPlayerInTeams)
                }).ExecuteAsync<EntityCollection<UserEntity>>(cancellationToken);
                da.CloseConnection();
                return result.FirstOrDefault();
            }
        }

        /// <summary>
        /// Checks whether a user exists for the given username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Returns true, if a user with the given username exists, else false.</returns>
        public virtual async Task<bool> UsernameExistsAsync(string username)
        {
            username = username.ToLowerInvariant();
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                var result = await (from u in metaData.User
                    where username == u.UserName.ToLower()
                    select u).FirstOrDefaultAsync();
                return result != null;
            }
        }

        /// <summary>
        /// Checks whether a user exists for the given email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Returns true, if a user with the given email exists, else false.</returns>
        public virtual async Task<bool> EmailExistsAsync(string email)
        {
            email = email.ToLowerInvariant();
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                var result = await (from u in metaData.User
                    where email == u.Email.ToLower()
                    select u).FirstOrDefaultAsync();
                return result != null;
            }
        }

        public virtual async Task<List<UserEntity>> FindUserAsync(IPredicateExpression filter, int limit, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            return (await da.FetchQueryAsync<UserEntity>(
                new QueryFactory().User.Where(filter).Distinct().Limit(limit), cancellationToken)).Cast<UserEntity>().ToList();
        }

        /// <summary>
        /// Sets the last login date and time, without changing the ModifiedOn value.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="loginDateTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns <c>true</c>, if login date successfully set, else <c>false</c></returns>
        public virtual async Task<bool> SetLastLoginDateAsync(string userName, DateTime? loginDateTime, CancellationToken cancellationToken)
        {
            var user = await GetLoginUserByUserNameAsync(userName, cancellationToken);
            if (user == null) return false;

            var count = 0;
            using (var da = _dbContext.GetNewAdapter())
            {
                user = new UserEntity(user.Id) {IsNew = false, LastLoginOn = loginDateTime ?? DateTime.UtcNow };
                try
                {
                    count = await da.UpdateEntitiesDirectlyAsync(user,
                        new RelationPredicateBucket(UserFields.Id == user.Id), cancellationToken);
                }
                catch
                {
                    // Count variable indicates success
                }

                if (count == 1)
                {
                    _logger.LogInformation($"Sign-in date for user id '{user.Id}' updated.");
                }
                else
                {
                    _logger.LogError($"Sign-in date for user id '{user.Id}' could not be updated.");
                }
            }

            return count == 1;
        }

        public virtual async Task<string> GenerateUniqueUsernameAsync(string email, string allowedCharacters, string defaultName = "User")
        {
            var userNameToTry = (email ?? defaultName).Split(new[] {'@'}, 2)[0];
            userNameToTry = ReplaceDisallowedCharacters(userNameToTry.Length == 0 ? defaultName : userNameToTry,
                allowedCharacters);
            var rnd = new Random();
            var i = rnd.Next(1122, 9922);

            do
            {
                var lastNameTried = userNameToTry;
                userNameToTry = ReplaceDisallowedCharacters(userNameToTry + (i += 3), allowedCharacters);
                if (lastNameTried.Equals(userNameToTry)) throw new Exception("Infinite loop: Allowed characters prevent generating a unique username");
            } while (await UsernameExistsAsync(userNameToTry));

            return userNameToTry;
        }

        private static string ReplaceDisallowedCharacters(string input, string allowedCharacters = "abcdefghijklmnopqrstuvwxyz")
        {
            if (string.IsNullOrEmpty(allowedCharacters) || string.IsNullOrEmpty(input)) return input;

            var rnd = new Random();
            var result = input.ToCharArray();
            var allowedCharSet = new System.Collections.Generic.HashSet<char>(allowedCharacters);
            for (var i = 0; i < result.Length; i++)
            {
                if (!allowedCharSet.Contains(result[i]))
                {
                    result[i] = allowedCharSet.ElementAt(rnd.Next(0, allowedCharSet.Count - 1));
                }
            }

            return new string(result);
        }
    }
}