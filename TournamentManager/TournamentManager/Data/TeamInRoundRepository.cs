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
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data
{
	public class TeamInRoundRepository
	{
        private static readonly ILogger _logger = AppLogging.CreateLogger<TeamInRoundRepository>();
        private readonly MultiTenancy.IDbContext _dbContext;
        public TeamInRoundRepository(MultiTenancy.IDbContext dbContext)
	    {
	        _dbContext = dbContext;
	    }

        /// <summary>
        /// Gets an <see cref="IList{T}"/> of <see cref="TeamInRoundEntity"/>s matching the filter criteria.
        /// </summary>
        /// <param name="filter">The filter <see cref="IPredicateExpression"/> may contain <see cref="TeamInRoundFields"/> and <see cref="RoundFields"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns an <see cref="IList{T}"/> of <see cref="TeamInRoundEntity"/>s matching the filter criteria.</returns>
        public virtual async Task<IList<TeamInRoundEntity>> GetTeamInRoundAsync(IPredicateExpression filter, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var qf = new QueryFactory();
            var q = qf.TeamInRound.From(QueryTarget.InnerJoin(qf.Round)
                .On(TeamInRoundFields.RoundId == RoundFields.Id)).Where(filter);

            return (IList<TeamInRoundEntity>) await da.FetchQueryAsync<TeamInRoundEntity>(
                q, cancellationToken);
        }
	}
}