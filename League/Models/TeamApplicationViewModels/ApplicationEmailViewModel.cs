using System;
using League.DI;
using TournamentManager.Data;

namespace League.Models.TeamApplicationViewModels
{
    public class ApplicationEmailViewModel
    {
        /// <summary>
        /// If <see langword="true"/>, it is a new application, otherwise
        /// an existing application was updated.
        /// </summary>
        public bool IsNewApplication { get; set; }
        public string TournamentName { get; set; }
        public long RoundId { get; set; }
        public string RoundDescription { get; set; }
        public string RoundTypeDescription { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        /// <summary>
        /// If <see langword="true"/>, specific content for the registering person is shown.
        /// </summary>
        public bool IsRegisteringUser { get; set; }
        public long RegisteredByUserId { get; set; }
        public string RegisteredByName { get; set; }
        public OrganizationContext OrganizationContext { get; set; }
        public string UrlToEditApplication { get; set; }
    }
}
