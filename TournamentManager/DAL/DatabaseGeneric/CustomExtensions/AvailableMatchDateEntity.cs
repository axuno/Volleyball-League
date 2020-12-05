using System;
using TournamentManager.DAL.HelperClasses;

#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class AvailableMatchDateEntity
	{
		/// <summary>
		/// MinTimeDiff is not a database field. 
		/// It is filled by an algorithm in order to determine the time span between matches.
		/// </summary>
		public TimeSpan MinTimeDiff { get; set; }

		protected override void OnBeforeEntitySave()
        {
            var now = DateTime.UtcNow;
            if (IsNew && !Fields[AvailableMatchDateFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
            if (IsDirty && !Fields[AvailableMatchDateFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
            base.OnBeforeEntitySave();
        }
	}
}
