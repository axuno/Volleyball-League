using System;
#if !CF
#endif

namespace TournamentManager.DAL.EntityClasses
{
	public partial class MatchEntity
	{
		/// <summary>
		/// Gets the current date/time of PlannedStart and RealStart
		/// </summary>
		/// <returns>Returns the current DateTime of PlannedStart and RealStart</returns>
		public DateTime? GetCurrentStart()
		{
			if (RealStart.HasValue)
				return RealStart; 

			return PlannedStart;
		}

		/// <summary>
		/// Gets the current date/time of PlannedEnd, AuxPlannedEnd and RealEnd
		/// </summary>
		/// <returns>Returns the current DateTime of PlannedEnd and RealEnd</returns>
		public DateTime? GetCurrentEnd()
		{
			if (RealEnd.HasValue)
				return RealEnd;

			return PlannedEnd;
		}

		/// <summary>
		/// Gets the match points for the match
		/// </summary>
		/// <returns>Returns the match points for the match if a result is present, else an empty string</returns>
		public string GetMatchPoints()
		{
			return HomePoints.HasValue && GuestPoints.HasValue
			       	? string.Format("{0}:{1}", HomePoints.Value, GuestPoints.Value)
			       	: string.Empty;
		}

		/// <summary>
		/// Sets the planned start and end date of the match.
		/// If a new date is set, the original is stored to OrigPlannedStart and OrigPlannedEnd.
		/// Current data must be loaded from database before calling the method.
		/// </summary>
		/// <param name="newDate">New planned date for the match.</param>
		/// <param name="duration">Duration of the match.</param>
		public void SetPlannedStart(DateTime? newDate, TimeSpan duration)
		{
			if (newDate == PlannedStart) return;

			if (!OrigPlannedStart.HasValue)
			{
				OrigPlannedStart = PlannedStart;
				OrigPlannedEnd = PlannedEnd;
			}

			PlannedStart = newDate;
			PlannedEnd = newDate?.Add(duration);

			if (PlannedStart == OrigPlannedStart)
				OrigPlannedStart = OrigPlannedEnd = null;
		}

        /// <summary>
        /// Sets the real start and end date of the match.
        /// Current data must be loaded from database before calling the method.
        /// </summary>
        /// <param name="newDate">New real date for the match.</param>
        /// <param name="duration">Duration of the match.</param>
        public void SetRealStart(DateTime? newDate, TimeSpan duration)
        {
            if (newDate == RealStart && newDate?.Add(duration) == RealEnd) return;

            RealStart = newDate;
            RealEnd = newDate?.Add(duration);
        }

		/// <summary>
		/// Sets the venue of the match.
		/// If a new venue is set, the original is stored to OrigVenueId.
		/// Current data must be loaded from database before calling the method.
		/// </summary>
		/// <param name="venueId">New venue id</param>
		public void SetVenueId(long? venueId)
		{
			if (venueId == VenueId) return;

			if (!OrigVenueId.HasValue)
				OrigVenueId = VenueId;

			VenueId = venueId;

			if (VenueId == OrigVenueId)
				OrigVenueId = null;
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
