using System;
using TournamentManager.DAL.EntityClasses;

namespace League.Models.UploadViewModels;

public class TeamPhotoViewModel
{
    public string PhotoFileUrl { get; set; }

    public DateTime? PhotoFileDate { get; set; }

    public TeamEntity Team { get; set; }
}