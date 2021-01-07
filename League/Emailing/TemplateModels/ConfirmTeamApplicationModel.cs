﻿using System;
using TournamentManager.MultiTenancy;
using League.Templates.Email;

namespace League.Emailing.TemplateModels
{
    /// <summary>
    /// The model is used for template <see cref="TemplateName.ConfirmTeamApplicationTxt"/>
    /// </summary>
    public class ConfirmTeamApplicationModel
    {
        /// <summary>
        /// If <see langword="true"/>, it is a new application, otherwise
        /// an existing application was updated.
        /// </summary>
        public bool IsNewApplication { get; set; }
        public string TournamentName { get; set; }
        public string RoundDescription { get; set; }
        public string RoundTypeDescription { get; set; }
        public string TeamName { get; set; }
        /// <summary>
        /// If <see langword="true"/>, specific content for the registering person is shown.
        /// </summary>
        public bool IsRegisteringUser { get; set; }
        public string RegisteredByName { get; set; }
        public string RegisteredByEmail { get; set; }
        public string UrlToEditApplication { get; set; }
    }
}
