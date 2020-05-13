using System;
using TournamentManager.DAL.HelperClasses;

#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class PlayerInTeamEntity
	{
        protected override void OnBeforeEntitySave()
        {
            var now = DateTime.UtcNow;
            if (IsNew && !Fields[PlayerInTeamFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
            if (IsDirty && !Fields[PlayerInTeamFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
            base.OnBeforeEntitySave();
        }
	}
}
