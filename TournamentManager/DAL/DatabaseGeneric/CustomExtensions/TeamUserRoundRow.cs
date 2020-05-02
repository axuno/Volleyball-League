using System.Xml.Serialization;

namespace TournamentManager.DAL.TypedViewClasses
{
	public partial class TeamUserRoundRow
    {
        [XmlIgnore]
        public string CompleteName => (string.Join(" ", new[] { Title, FirstName, MiddleName, LastName }).Replace("  ", " ").Trim());

        [XmlIgnore]
        public string CompleteNameWithNickName
        {
            get
            {
                if (!string.IsNullOrEmpty(Nickname) && Nickname != FirstName)
                    return string.Join(" ", new[] { Title, FirstName, MiddleName, "(" + Nickname + ")", LastName }).Replace("  ", " ").Trim();

                return CompleteName;
            }
        }
    }
}
