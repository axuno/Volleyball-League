using System;
#if !CF
#endif
using System.Linq;

namespace TournamentManager.DAL.EntityClasses
{
	public partial class TeamEntity
	{
		/// <summary>
		/// Trims all string fields, and sets ':' as a time separator.
		/// </summary>
		public void Normalize()
		{
			Name = Name.Trim();
			ClubName = ClubName.Trim();
			PhotoFilename = PhotoFilename.Trim();
		}

		/// <summary>
		/// Converts the team name property into a string that can be used in an Url.
		/// All letters and digits are kept as they are, all other characters are replaced with hyphens.
		/// </summary>
		/// <returns>Returns the team name as a string that can be used in an Url.</returns>
		public string UrlSanitizedName()
		{
			return UrlSanitizedName(Name);
		}

		/// <summary>
		/// Converts the a name into a string that can be used in an Url.
		/// All letters and digits are kept as they are, all other characters are replaced with hyphens.
		/// </summary>
		/// <param name="name">Name to url sanitize.</param>
		/// <returns>Returns the team name as a string that can be used in an Url.</returns>
		public static string UrlSanitizedName(string name)
		{
			return new string(name.Select(c => Char.IsLetterOrDigit(c) ? c : '-').ToArray()).ToLowerInvariant().Replace("--", "-");
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
