using System;
using TournamentManager.DAL.HelperClasses;

#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class ExcludeMatchDateEntity
	{
        protected override void OnBeforeEntitySave()
        {
            var now = DateTime.UtcNow;
            if (IsNew && !Fields[ExcludeMatchDateFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
            if (IsDirty && !Fields[ExcludeMatchDateFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
            base.OnBeforeEntitySave();
        }
	}
}
