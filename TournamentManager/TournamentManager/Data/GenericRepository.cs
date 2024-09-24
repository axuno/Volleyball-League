using System.Data;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.Data;

public class GenericRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<GenericRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public GenericRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public virtual async Task<bool> SaveEntityAsync<T>(T entityToSave, bool refetchAfterSave, bool recurse, CancellationToken cancellationToken) where T : IEntity2
    {
        var transactionName = string.Concat(nameof(GenericRepository), nameof(SaveEntityAsync), Guid.NewGuid().ToString("N"));
        using var da = _dbContext.GetNewAdapter();
        try
        {
            await da.StartTransactionAsync(IsolationLevel.ReadCommitted, transactionName, cancellationToken);
            await da.SaveEntityAsync(entityToSave, refetchAfterSave, recurse, cancellationToken);
            await da.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving entity in transaction: {entity}", entityToSave);

            if (da.IsTransactionInProgress)
                da.Rollback();

            throw;
        }
    }

    public virtual async Task SaveEntitiesAsync<T>(T entitiesToSave, bool refetchAfterSave, bool recurse, CancellationToken cancellationToken) where T : IEntityCollection2
    {
        using var da = _dbContext.GetNewAdapter();
        // SaveEntityCollectionAsync uses a transaction automatically
        var count = await da.SaveEntityCollectionAsync(entitiesToSave, refetchAfterSave, recurse, cancellationToken);
    }
 
    public virtual async Task<bool> DeleteEntityAsync<T>(T entityToDelete, CancellationToken cancellationToken) where T : IEntity2
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.DeleteEntityAsync(entityToDelete, null, cancellationToken);
    }

    public virtual async Task DeleteEntitiesAsync<T>(T entitiesToDelete, CancellationToken cancellationToken) where T : IEntityCollection2
    {
        using var da = _dbContext.GetNewAdapter();
        // DeleteEntityCollectionAsync uses a transaction automatically
        var count = await da.DeleteEntityCollectionAsync(entitiesToDelete, cancellationToken);
    }

    public virtual async Task<int> DeleteEntitiesDirectlyAsync(Type entityType, IRelationPredicateBucket? filterBucket, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.DeleteEntitiesDirectlyAsync(entityType, filterBucket, cancellationToken);
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
        using var da = _dbContext.GetNewAdapter();
        var bucket = new RelationPredicateBucket(uniqueConstraintFilter);
        return await da.DeleteEntitiesDirectlyAsync(typeof(T), bucket, cancellationToken);
    }
}
