using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data;

/// <summary>
/// Class for database operations for available match dates.
/// </summary>
public class AvailableMatchDateRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<AvailableMatchDateRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;
    public AvailableMatchDateRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the <see cref="EntityCollection{TEntity}"/> of type <see cref="AvailableMatchDateEntity"/> for a tournament.
    /// </summary>
    /// <param name="tournamentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the <see cref="EntityCollection{TEntity}"/> of type <see cref="AvailableMatchDateEntity"/> for a tournament.</returns>
    public async Task<EntityCollection<AvailableMatchDateEntity>> GetAvailableMatchDatesAsync (long tournamentId, CancellationToken cancellationToken)
    {
        var available = new EntityCollection<AvailableMatchDateEntity>();
        using var da = _dbContext.GetNewAdapter();
        var qp = new QueryParameters
        {
            CollectionToFetch = available,
            FilterToUse = AvailableMatchDateFields.TournamentId == tournamentId
        };
        await da.FetchEntityCollectionAsync(qp, cancellationToken);
        da.CloseConnection();

        return available;
    }
}
