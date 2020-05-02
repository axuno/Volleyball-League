using System;
#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class MatchRuleEntity
	{
        /// <summary>
        /// Gets the maximum number of sets to play.
        /// </summary>
        /// <returns>Returns the maximum number of sets to play.</returns>
        public int MaxNumOfSets()
        {
            return BestOf ? NumOfSets * 2 - 1 : NumOfSets;
        }

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
            var now = _dateModifiedCreatedOn ?? DateTime.UtcNow;
            if (IsNew) CreatedOn = now;
            if (IsDirty) ModifiedOn = now;
            base.OnBeforeEntitySave();
        }
	}
}
