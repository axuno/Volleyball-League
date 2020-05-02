using System;
using System.Runtime.CompilerServices;

#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class SetEntity
	{
        /// <summary>
        /// Assigns &quot;set points&quot; to the <see cref="SetEntity"/>.
        /// The <see cref="SetEntity.IsOverruled"/> property will be set to <see langword="false"/>
        /// </summary>
        /// <param name="setRule">The <see cref="SetRuleEntity"/> with the rules to apply.</param>
        public void CalculateSetPoints(SetRuleEntity setRule)
        {
            IsOverruled = false;
            if (HomeBallPoints > GuestBallPoints)
            {
                HomeSetPoints = setRule.PointsSetWon;
                GuestSetPoints = setRule.PointsSetLost;
            }
            else if (HomeBallPoints < GuestBallPoints)
            {
                HomeSetPoints = setRule.PointsSetLost;
                GuestSetPoints = setRule.PointsSetWon;
            }
            else
            {
                HomeSetPoints = GuestSetPoints = setRule.PointsSetTie;
            }
        }

        /// <summary>
        /// Sets home/guest ball points and home/guest set points.
        /// The <see cref="SetEntity.IsOverruled"/> property will be set to <see langword="true"/>,
        /// while <see cref="SetEntity.IsTieBreak"/> will be set to <see langword="false"/>.
        /// </summary>
        /// <param name="homeBallPoints"></param>
        /// <param name="guestBallPoints"></param>
        /// <param name="homeSetPoints"></param>
        /// <param name="guestSetPoints"></param>
        public void Overrule(int homeBallPoints, int guestBallPoints, int homeSetPoints, int guestSetPoints)
        {
            HomeBallPoints = homeBallPoints;
            GuestBallPoints = guestBallPoints;
            HomeSetPoints = homeSetPoints;
            GuestSetPoints = guestSetPoints;
            IsTieBreak = false;
            IsOverruled = true;
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
