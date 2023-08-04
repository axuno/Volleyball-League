using System;
using System.Runtime.CompilerServices;
using TournamentManager.DAL.HelperClasses;

#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class SetEntity
	{
        protected override void OnBeforeEntitySave()
        {
            var now = DateTime.UtcNow;
            if (IsNew && !Fields[SetFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
            if (IsDirty && !Fields[SetFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
            base.OnBeforeEntitySave();
        }
    }
}
