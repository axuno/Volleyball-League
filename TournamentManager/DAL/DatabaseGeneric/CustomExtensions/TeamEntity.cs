using System;
#if !CF
#endif
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.DAL.EntityClasses;

public partial class TeamEntity
{
    protected override void OnBeforeEntitySave()
    {
        Name = Name?.Trim();
        ClubName = ClubName?.Trim();

        var now = DateTime.UtcNow;
        if (IsNew && !Fields[TeamFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
        if (IsDirty && !Fields[TeamFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
        base.OnBeforeEntitySave();
    }
}