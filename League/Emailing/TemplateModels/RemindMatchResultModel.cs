using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace League.Emailing.TemplateModels
{
    public class RemindMatchResultModel
    {
        public TournamentManager.DAL.TypedViewClasses.PlannedMatchRow Fixture { get; set; } = new TournamentManager.DAL.TypedViewClasses.PlannedMatchRow();
    }
}
