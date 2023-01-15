using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace League.Emailing.TemplateModels;

public class ResultEnteredModel
{
    public string Username { get; set; }
    public string RoundDescription { get; set; }
    public string HomeTeamName { get; set; }
    public string GuestTeamName { get; set; }
    public TournamentManager.DAL.EntityClasses.MatchEntity Match { get; set; }
}