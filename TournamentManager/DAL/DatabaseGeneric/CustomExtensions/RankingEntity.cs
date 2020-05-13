using System;
using TournamentManager.DAL.HelperClasses;

#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class RankingEntity
	{
        private DateTime? _dateModifiedCreatedOn;

        /// <summary>
        /// Sets the date for <see cref="CreatedOn"/> and <see cref="ModifiedOn"/>.
        /// If it is not set, <see cref="DateTime.UtcNow"/> will be used.
        /// </summary>
        /// <param name="dateModifiedCreatedOn"></param>
        public void SetModifiedOnDate(DateTime dateModifiedCreatedOn)
        {
            _dateModifiedCreatedOn = dateModifiedCreatedOn;
        }

        protected override void OnBeforeEntitySave()
        {
            var now = DateTime.UtcNow;
            if (IsNew && !Fields[RankingFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
            if (IsDirty && !Fields[RankingFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = _dateModifiedCreatedOn ?? now;
            base.OnBeforeEntitySave();
        }
	}
}
