using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.Data
{
    public class GenericRepository
    {
        private static readonly ILogger _logger = AppLogging.CreateLogger<GenericRepository>();
        private readonly IDbContext _dbContext;

        public GenericRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual bool SaveEntity<T>(T entityToSave, bool refetchAfterSave, bool recurse) where T:IEntity2
        {
            var transactionName = Guid.NewGuid().ToString("N");
            using (var da = _dbContext.GetNewAdapter())
            {
                try
                {
                    da.StartTransaction(IsolationLevel.ReadCommitted, transactionName);
                    var success = da.SaveEntity(entityToSave, refetchAfterSave, recurse);
                    da.Commit();
                    return success;
                }
                catch (Exception)
                {
                    if (da.IsTransactionInProgress)
                        da.Rollback();

                    da.CloseConnection();
                    throw;
                }
            }
        }

        public virtual async Task<bool> SaveEntityAsync<T>(T entityToSave, bool refetchAfterSave, bool recurse, CancellationToken cancellationToken) where T : IEntity2
        {
            var transactionName = Guid.NewGuid().ToString("N");
            using (var da = _dbContext.GetNewAdapter())
            {
                try
                {
                    await da.StartTransactionAsync(IsolationLevel.ReadCommitted, transactionName, cancellationToken);
                    var success = await da.SaveEntityAsync(entityToSave, refetchAfterSave, recurse, cancellationToken);
                    da.Commit();
                    return success;
                }
                catch (Exception)
                {
                    if (da.IsTransactionInProgress)
                        da.Rollback();

                    da.CloseConnection();
                    throw;
                }
            }
        }

        public virtual async Task SaveEntitiesAsync<T>(T entitiesToSave, bool refetchAfterSave, bool recurse, CancellationToken cancellationToken) where T : IEntityCollection2
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                // SaveEntityCollectionAsync uses a transaction automatically
                var count = await da.SaveEntityCollectionAsync(entitiesToSave, refetchAfterSave, recurse, cancellationToken);
            }
        }
 
        public virtual async Task<bool> DeleteEntityAsync<T>(T entityToDelete, CancellationToken cancellationToken) where T : IEntity2
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.DeleteEntityAsync(entityToDelete, null, cancellationToken);
            }
        }

        public virtual async Task DeleteEntitiesAsync<T>(T entitiesToDelete, CancellationToken cancellationToken) where T : IEntityCollection2
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                // DeleteEntityCollectionAsync uses a transaction automatically
                var count = await da.DeleteEntityCollectionAsync(entitiesToDelete, cancellationToken);
            }
        }

        /// <summary>
        /// Deletes all entities matching the <see cref="IPredicateExpression"/>.
        /// If the <see cref="IPredicateExpression"/> is null, all entities will be deleted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueConstraintFilter">The <see cref="IPredicateExpression"/> filter to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>Returns the number of deleted table rows.</returns>
        public virtual async Task<int> DeleteEntitiesUsingConstraintAsync<T>(IPredicateExpression uniqueConstraintFilter, CancellationToken cancellationToken) where T : IEntity2
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var bucket = new RelationPredicateBucket(uniqueConstraintFilter);
                return await da.DeleteEntitiesDirectlyAsync(typeof(T), bucket, cancellationToken);
            }
        }
    }
}
