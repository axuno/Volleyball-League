using System;
using TournamentManager.DAL.HelperClasses;

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

        protected override void OnBeforeEntitySave()
        {
            var now = DateTime.UtcNow;
            if (IsNew && !Fields[MatchRuleFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
            if (IsDirty && !Fields[MatchRuleFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
            base.OnBeforeEntitySave();
        }
	}
}
