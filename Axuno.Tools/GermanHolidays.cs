using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Axuno.Tools
{
    /// <summary>
    /// This class represents a German holiday.
    /// </summary>
    public class GermanHoliday
    {
        /// <summary>
        /// Delegate to use for the calculation of the holiday.
        /// </summary>
        /// <returns>Date of the holiday.</returns>
        public delegate DateTime CalcDateCallback();

        /// <summary>
        /// CTOR.
        /// </summary>
        private GermanHoliday()
        {
        }

        /// <summary>
        /// Constructor for internal use.
        /// </summary>
        /// <param name="id">Nullable HolidayId</param>
        /// <param name="type">HolidayType</param>
        /// <param name="name">Holiday name</param>
        /// <param name="calcDateCallback">Delegate for date calculation</param>
        internal GermanHoliday(GermanHolidays.Id? id, GermanHolidays.Type type, string name,
            CalcDateCallback calcDateCallback) : this()
        {
            Id = id;
            Type = type;
            Name = name;
            DoCalcDate = calcDateCallback;

            PublicHolidayStateIds = id.HasValue
                ? GermanHolidays.GetPublicHolidayStates(id.Value)
                : new List<GermanFederalStates.Id>();
        }

        /// <summary>
        /// Constructor for usage from inside of class GermanHolidays.
        /// </summary>
        /// <param name="id">HolidayId</param>
        /// <param name="type">HolidayType</param>
        /// <param name="name">Holiday name</param>
        /// <param name="getDate">Delegate for date calculation</param>
        internal GermanHoliday(GermanHolidays.Id id, GermanHolidays.Type type, string name, CalcDateCallback getDate)
            : this((GermanHolidays.Id?) id, type, name, getDate)
        {
        }

        /// <summary>
        /// Constructor for creating custom holidays.
        /// </summary>
        /// <param name="name">Holiday name</param>
        /// <param name="getDate">Delegate for date calculation</param>
        public GermanHoliday(string name, CalcDateCallback getDate)
            : this(null, GermanHolidays.Type.Custom, name, getDate)
        {
        }

        /// <summary>
        /// Holiday id.
        /// </summary>
        public GermanHolidays.Id? Id { get; }

        /// <summary>
        /// Holiday type.
        /// </summary>
        public GermanHolidays.Type Type { get; set; }

        /// <summary>
        /// Holiday name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Holiday date.
        /// </summary>
        public DateTime Date => DoCalcDate();

        /// <summary>
        /// List of federal states where this is a holiday.
        /// </summary>
        public List<GermanFederalStates.Id> PublicHolidayStateIds { get; set; }

        /// <summary>
        /// Calls the function which does the calculation of the holiday.
        /// </summary>
        public CalcDateCallback DoCalcDate { get; set; }

        /// <summary>
        /// Checks whether this is a holiday specific to a federal state.
        /// </summary>
        /// <param name="stateId">The federal state id to check.</param>
        /// <returns>Return true, if this is a holiday of this federal state, else false.</returns>
        public bool IsPublicHoliday(GermanFederalStates.Id stateId)
        {
            return PublicHolidayStateIds.Exists(s => s == stateId);
        }

        public static bool operator ==(GermanHoliday h1, GermanHoliday h2)
        {
            if (h1 == null || h2 == null) return false;
            return h1.Equals(h2);
        }

        public static bool operator !=(GermanHoliday h1, GermanHoliday h2)
        {
            if (h1 == null || h2 == null) return true;
            return !h1.Equals(h2);
        }

        public static bool operator <(GermanHoliday h1, GermanHoliday h2)
        {
            if (h1 == null || h2 == null) return false;
            return h1.Date < h2.Date && string.Compare(h1.Name, h2.Name, StringComparison.Ordinal) < 0;
        }

        public static bool operator >(GermanHoliday h1, GermanHoliday h2)
        {
            if (h1 == null || h2 == null) return false;
            return h1.Date > h2.Date && string.Compare(h1.Name, h2.Name, StringComparison.Ordinal) > 0;
        }

        protected bool Equals(GermanHoliday other)
        {
            return Id == other.Id && Type == other.Type && Name == other.Name &&
                   Equals(PublicHolidayStateIds, other.PublicHolidayStateIds) && Equals(DoCalcDate, other.DoCalcDate);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GermanHoliday) obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return HashCode.Combine(Id, (int) Type, Name, PublicHolidayStateIds, DoCalcDate);
        }
    }


    public class GermanHolidays : List<GermanHoliday>
    {
        private readonly DateTime _easterSunday;

        /// <summary>
        /// CTOR.
        /// </summary>
        private GermanHolidays()
        {
        }

        /// <summary>
        /// Constructor. Creates a collection of German public holidays and commemoration days.
        /// </summary>
        /// <param name="year">Year to use for holiday calculations (1583 - 4099).</param>
        public GermanHolidays(int year)
        {
            // Plausibility check
            if (year < 1583 || year > 4099)
                throw new Exception("Year must be between 1583 and 4099.");

            Year = year;
            _easterSunday = GetEasterSunday();

            Add(new GermanHoliday(Id.Neujahr, Type.Public, "Neujahr", () => new DateTime(Year, 1, 1)));
            Add(new GermanHoliday(Id.HeiligeDreiKoenige, Type.Public, "Heilige Drei Kˆnige",
                () => new DateTime(Year, 1, 6)));
            Add(new GermanHoliday(Id.RosenMontag, Type.Commemoration, "Rosenmontag", () => _easterSunday.AddDays(-48)));
            Add(new GermanHoliday(Id.FaschingsDienstag, Type.Commemoration, "Faschingsdienstag",
                () => _easterSunday.AddDays(-47)));
            Add(new GermanHoliday(Id.AscherMittwoch, Type.Commemoration, "Aschermittwoch",
                () => _easterSunday.AddDays(-46)));
            Add(new GermanHoliday(Id.GruenDonnerstag, Type.Commemoration, "Gr¸ndonnerstag",
                () => _easterSunday.AddDays(-3)));
            Add(new GermanHoliday(Id.KarFreitag, Type.Public, "Karfreitag", () => _easterSunday.AddDays(-2)));
            Add(new GermanHoliday(Id.KarSamstag, Type.Commemoration, "Karsamstag", () => _easterSunday.AddDays(-1)));
            Add(new GermanHoliday(Id.OsterSonntag, Type.Public, "Ostersonntag", () => _easterSunday));
            Add(new GermanHoliday(Id.OsterMontag, Type.Public, "Ostermontag", () => _easterSunday.AddDays(1)));
            Add(new GermanHoliday(Id.Maifeiertag, Type.Public, "Maifeiertag", () => new DateTime(Year, 5, 1)));
            Add(new GermanHoliday(Id.ChristiHimmelfahrt, Type.Public, "Christi Himmelfahrt",
                () => _easterSunday.AddDays(39)));
            Add(new GermanHoliday(Id.Muttertag, Type.Commemoration, "Muttertag", GetMuttertag));
            Add(new GermanHoliday(Id.PfingstSonntag, Type.Public, "Pfingstsonntag", () => _easterSunday.AddDays(49)));
            Add(new GermanHoliday(Id.PfingstMontag, Type.Public, "Pfingstmontag", () => _easterSunday.AddDays(50)));
            Add(new GermanHoliday(Id.Fronleichnam, Type.Public, "Fronleichnam", () => _easterSunday.AddDays(60)));
            Add(new GermanHoliday(Id.MariaHimmelfahrt, Type.Public, "Maria Himmelfahrt",
                () => new DateTime(Year, 8, 15)));
            Add(new GermanHoliday(Id.AugsburgerFriedensfest, Type.Commemoration, "Augsburger Friedensfest",
                () => new DateTime(Year, 8, 8)));
            Add(new GermanHoliday(Id.TagDerDeutschenEinheit, Type.Public, "Tag der deutschen Einheit",
                () => new DateTime(Year, 10, 3)));
            Add(new GermanHoliday(Id.Reformationstag, Type.Public, "Reformationstag",
                () => new DateTime(Year, 10, 31)));
            Add(new GermanHoliday(Id.Allerheiligen, Type.Public, "Allerheiligen", () => new DateTime(Year, 11, 1)));
            Add(new GermanHoliday(Id.BussUndBettag, Type.Public, "Buﬂ- und Bettag", GetBussUndBettag));
            Add(new GermanHoliday(Id.Volkstrauertag, Type.Commemoration, "Volkstrauertag",
                () => GetAdventDate(1).AddDays(-14)));
            Add(new GermanHoliday(Id.TotenSonntag, Type.Commemoration, "Totensonntag",
                () => GetAdventDate(1).AddDays(-7)));
            Add(new GermanHoliday(Id.Nikolaus, Type.Commemoration, "Nikolaus", () => new DateTime(Year, 12, 6)));
            Add(new GermanHoliday(Id.Advent1, Type.Commemoration, "1. Advent", () => GetAdventDate(1)));
            Add(new GermanHoliday(Id.Advent2, Type.Commemoration, "2. Advent", () => GetAdventDate(2)));
            Add(new GermanHoliday(Id.Advent3, Type.Commemoration, "3. Advent", () => GetAdventDate(3)));
            Add(new GermanHoliday(Id.Advent4, Type.Commemoration, "4. Advent", () => GetAdventDate(4)));
            Add(new GermanHoliday(Id.HeiligerAbend, Type.Commemoration, "Heiliger Abend",
                () => new DateTime(Year, 12, 24)));
            Add(new GermanHoliday(Id.Weihnachtsfeiertag1, Type.Public, "1. Weihnachtsfeiertag",
                () => new DateTime(Year, 12, 25)));
            Add(new GermanHoliday(Id.Weihnachtsfeiertag2, Type.Public, "2. Weihnachtsfeiertag",
                () => new DateTime(Year, 12, 26)));
            Add(new GermanHoliday(Id.Silvester, Type.Commemoration, "Silvester", () => new DateTime(Year, 12, 31)));
#if DEBUG
            // Make sure every holiday in enum HolidayId is handled in this collection
            foreach (Id holidayId in Enum.GetValues(typeof(Id)))
                try
                {
                    _ = Contains(this[holidayId]);
                }
                catch
                {
                    throw new Exception(string.Format("HolidayId \"{0}\" wird in \"{1}\" nicht abgebildet.", holidayId,
                        GetType()));
                }
#endif
        }


        /// <summary>
        /// Return the year German holidays were calculated for.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the German holiday for the specified holiday id.
        /// </summary>
        /// <param name="holidayId">HolidayId</param>
        /// <returns>GermanHoliday</returns>
        public GermanHoliday this[Id holidayId]
        {
            get
            {
                bool HolidayFilter(GermanHoliday h) => h.Id.Value == holidayId;
                return Find(HolidayFilter);
            }
        }

        /// <summary>
        /// Gets a list of German holidays for the specified date.
        /// </summary>
        /// <param name="date">Holiday date</param>
        /// <returns>A list of GermanHoliday objects sorted by date></returns>
        public List<GermanHoliday> this[DateTime date]
        {
            get
            {
                bool DateFilter(GermanHoliday h) => h.Date.Date.Equals(date.Date);
                return FindAll(DateFilter).OrderBy(h => h.Date.Date).ToList();
            }
        }

        /// <summary>
        /// Calculation of Easter Sunday.
        /// Based on source from Ronald W. Mallen (www.assa.org.au/edm.html),
        /// </summary>
        /// <returns>Date of Easter Sunday</returns>
        private DateTime GetEasterSunday()
        {
            // Easter Sunday is the Sunday following the Paschal Full Moon
            // date for the year

            int tA, tB, tC, tD, tE; // tables A to E

            var firstDigits = Year / 100;
            var remainding19 = Year % 19;

            // Calculate Paschal Full Moon
            var temp = (firstDigits - 15) / 2 + 202 - 11 * remainding19;
            switch (firstDigits)
            {
                case 21:
                case 24:
                case 25:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 34:
                case 35:
                case 38:
                    temp -= 1;
                    break;
                case 33:
                case 36:
                case 37:
                case 39:
                case 40:
                    temp -= 2;
                    break;
            }

            temp %= 30;
            tA = temp + 21;
            if (temp == 29)
                tA -= 1;
            if (temp == 28 && remainding19 > 10)
                tA -= 1;

            // Calculate next Sunday
            tB = (tA - 19) % 7;
            tC = (40 - firstDigits) % 4;
            if (tC == 3)
                tC += 1;
            if (tC > 1)
                tC += 1;
            temp = Year % 100;
            tD = (temp + temp / 4) % 7;

            tE = (20 - tB - tC - tD) % 7 + 1;

            // Calculate the date
            var day = tA + tE;
            int month;
            if (day > 31)
            {
                day -= 31;
                month = 4;
            }
            else
            {
                month = 3;
            }

            return new DateTime(Year, month, day);
        }

        /// <summary>
        /// Calculates the date of the required Advent
        /// </summary>
        /// <param name="num">Advent (1 to 4)</param>
        /// <returns>Advent date</returns>
        private DateTime GetAdventDate(int num)
        {
            if (num < 1 || num > 4)
                throw new Exception("Only Advents 1 to 4 are allowed.");

            // 4th Advent is the latest Sunday before 25th December
            var firstChristmasDay = new DateTime(Year, 12, 25);

            return firstChristmasDay.DayOfWeek switch
            {
                DayOfWeek.Tuesday => firstChristmasDay.AddDays(-2).AddDays(-7 * (4 - num)),
                DayOfWeek.Wednesday => firstChristmasDay.AddDays(-3).AddDays(-7 * (4 - num)),
                DayOfWeek.Thursday => firstChristmasDay.AddDays(-4).AddDays(-7 * (4 - num)),
                DayOfWeek.Friday => firstChristmasDay.AddDays(-5).AddDays(-7 * (4 - num)),
                DayOfWeek.Saturday => firstChristmasDay.AddDays(-6).AddDays(-7 * (4 - num)),
                DayOfWeek.Sunday => firstChristmasDay.AddDays(-7).AddDays(-7 * (4 - num)),
                _ => firstChristmasDay.AddDays(-1).AddDays(-7 * (4 - num))
            };
        }

        /// <summary>
        /// Adds a holiday to the list of holidays, if the year is in the scope of the holiday list.
        /// Only the Date part (without time) will persist.
        /// </summary>
        /// <param name="germanHoliday">GermanHoliday</param>
        public new void Add(GermanHoliday germanHoliday)
        {
            if (germanHoliday.DoCalcDate().Year == Year)
                base.Add(germanHoliday);
        }

        /// <summary>
        /// Adds a holiday to the list of holidays, if no holidays already exists for that day.
        /// The year must be in the scope of the holiday list. Only the Date part (without time)
        /// will persist.
        /// </summary>
        /// <param name="germanHoliday">GermanHoliday</param>
        public void Merge(GermanHoliday germanHoliday)
        {
            if (this[germanHoliday.Date].Count == 0)
                base.Add(germanHoliday);
        }

        /// <summary>
        /// Removes the holiday with the specified name from the list of holidays.
        /// </summary>
        /// <param name="holidayName"></param>
        public void Remove(string holidayName)
        {
            Remove(this.First(holiday => holiday.Name == holidayName));
        }


        /// <summary>
        /// Calculates the date for Buﬂ- und Bettag
        /// </summary>
        /// <returns>Date of Buﬂ- und Bettag</returns>
        private DateTime GetBussUndBettag()
        {
            // Wednesday before the last Sunday of the church year that begins with the 1st Advent
            // 32 days before 4th Advent
            return GetAdventDate(4).AddDays(-32);
        }

        /// <summary>
        /// Calculates the date for Mother's Day
        /// </summary>
        /// <returns>Date of Mother's Day</returns>
        private DateTime GetMuttertag()
        {
            // second Sunday in May. If this is Whitsun, then 1 week earlier
            var muttertag = new DateTime(Year, 5, 1).AddDays(7);
            while (muttertag.DayOfWeek != DayOfWeek.Sunday) muttertag = muttertag.AddDays(1);
            if (muttertag != _easterSunday.AddDays(49))
                return muttertag;
            return muttertag.AddDays(-7);
        }


        /// <summary>
        /// Get a list of German federal states for the specified holiday.
        /// </summary>
        /// <param name="holidayId">HolidayId</param>
        /// <returns>List of GermanFederalStateIds</returns>
        internal static List<GermanFederalStates.Id> GetPublicHolidayStates(Id holidayId)
        {
            // Source: https://de.wikipedia.org/wiki/Feiertage_in_Deutschland
            var publicHolidayStates = new List<GermanFederalStates.Id>();

            switch (holidayId)
            {
                // general public holidays:
                case Id.Neujahr:
                case Id.KarFreitag:
                case Id.OsterSonntag:
                case Id.OsterMontag:
                case Id.Maifeiertag:
                case Id.ChristiHimmelfahrt:
                case Id.PfingstSonntag:
                case Id.PfingstMontag:
                case Id.TagDerDeutschenEinheit:
                case Id.Weihnachtsfeiertag1:
                case Id.Weihnachtsfeiertag2:
                    foreach (var state in new GermanFederalStates()) publicHolidayStates.Add(state.StateId);
                    break;
                case Id.HeiligeDreiKoenige:
                    publicHolidayStates.Add(GermanFederalStates.Id.BadenWuerttemberg);
                    publicHolidayStates.Add(GermanFederalStates.Id.Bayern);
                    publicHolidayStates.Add(GermanFederalStates.Id.SachsenAnhalt);
                    break;
                case Id.Fronleichnam:
                    publicHolidayStates.Add(GermanFederalStates.Id.BadenWuerttemberg);
                    publicHolidayStates.Add(GermanFederalStates.Id.Bayern);
                    publicHolidayStates.Add(GermanFederalStates.Id.Hessen);
                    publicHolidayStates.Add(GermanFederalStates.Id.NordrheinWestfalen);
                    publicHolidayStates.Add(GermanFederalStates.Id.RheinlandPfalz);
                    publicHolidayStates.Add(GermanFederalStates.Id.Saarland);
                    publicHolidayStates.Add(GermanFederalStates.Id.SachsenAnhalt);
                    break;
                case Id.Reformationstag:
                    publicHolidayStates.Add(GermanFederalStates.Id.Brandenburg);
                    publicHolidayStates.Add(GermanFederalStates.Id.MecklenburgVorpommern);
                    publicHolidayStates.Add(GermanFederalStates.Id.Saarland);
                    publicHolidayStates.Add(GermanFederalStates.Id.Sachsen);
                    publicHolidayStates.Add(GermanFederalStates.Id.SachsenAnhalt);
                    publicHolidayStates.Add(GermanFederalStates.Id.Thueringen);
                    break;
                case Id.Allerheiligen:
                    publicHolidayStates.Add(GermanFederalStates.Id.BadenWuerttemberg);
                    publicHolidayStates.Add(GermanFederalStates.Id.Bayern);
                    publicHolidayStates.Add(GermanFederalStates.Id.NordrheinWestfalen);
                    publicHolidayStates.Add(GermanFederalStates.Id.RheinlandPfalz);
                    publicHolidayStates.Add(GermanFederalStates.Id.Saarland);
                    publicHolidayStates.Add(GermanFederalStates.Id.SachsenAnhalt);
                    break;
                case Id.BussUndBettag:
                    publicHolidayStates.Add(GermanFederalStates.Id.Sachsen);
                    break;
            }

            return publicHolidayStates;
        }

        /// <summary>
        /// Returns a sorted list of GermanHoliday which match the selected criteria.
        /// </summary>
        /// <param name="filter">Predicate to apply for filtering holidays.</param>
        /// <example>
        /// Get all federal public holidays and public holidays for Bayern:
        /// List&lt;GermanHoliday&gt; filteredHoliday = gh.GetFiltered(h =&gt; (h.PublicHolidayStateIds.Count == 0 ||
        /// h.PublicHolidayStateIds.Contains(GermanFederalStateId.Bayern)));
        /// </example>
        /// <returns>A list of GermanHoliday sorted by date.</returns>
        public List<GermanHoliday> GetFiltered(Predicate<GermanHoliday> filter)
        {
            return FindAll(filter).OrderBy(h => h.Date.Date).ToList();
        }

        /// <summary>
        /// Loads holiday data for a year from an XML file and adds, merges, removes or replaces
        /// the standard German holidays which are calculated automatically.
        /// All element and attribute names/values must match XML standards, but are not case sensitive.
        /// Dates not in scope of the year given with CTOR will just be ignored.
        /// </summary>
        /// <remarks>
        /// "Add": a holiday is added, even when a holiday already exists for this date.
        /// "Merge": a holiday is added, unless there is already a holiday present for this date.
        /// "Replace": an existing standard holiday is completely replaced with the data loaded.
        /// "Remove:" remove a standard holiday from the list.
        /// </remarks>
        /// <param name="path">A URI string referencing the holidays XML file to load.</param>
        public void Load(string path)
        {
            // load holiday data from XML file
            var holidayQuery = from holiday in XElement.Load(path).Elements()
                where holiday.Name.ToString().ToLower() == "holiday"
                select holiday;

            foreach (var holiday in holidayQuery)
            {
                // get the standard German holiday id (if any)
                Id? holidayId = null;
                if (holiday.Attributes().Count(e => e.Name.ToString().ToLower() == "id") != 0)
                    holidayId = (Id) Enum.Parse(typeof(Id),
                        holiday.Attributes().First(
                            e => e.Name.ToString().ToLower() == "id").Value,
                        true);

                // what to do with this holiday? The default action is "Merge".
                var action = ActionType.Merge;
                if (holiday.Attributes().Count(e => e.Name.ToString().ToLower() == "action") != 0)
                    action = (ActionType) Enum.Parse(typeof(ActionType),
                        holiday.Attributes().First(
                            e => e.Name.ToString().ToLower() == "action").Value,
                        true);

                // remove an existing (standard German) holiday
                if (action == ActionType.Remove && holidayId.HasValue)
                {
                    Remove(this[holidayId.Value]);
                    continue;
                }

                // get the dates (if any)
                var dateFrom = DateTime.MinValue;
                var dateTo = DateTime.MinValue;
                if (holiday.Elements().Count(e => e.Name.ToString().ToLower() == "datefrom") != 0 &&
                    holiday.Elements().Count(e => e.Name.ToString().ToLower() == "dateto") != 0)
                {
                    dateFrom =
                        DateTime.Parse(
                            holiday.Elements().First(e => e.Name.ToString().ToLower() == "datefrom").Value);
                    dateTo =
                        DateTime.Parse(holiday.Elements().First(e => e.Name.ToString().ToLower() == "dateto").Value);

                    // Swap from/to dates, if mixed up
                    if (dateFrom > dateTo) (dateFrom, dateTo) = (dateTo, dateFrom);

                    // holiday must be within the year given by CTOR
                    if (dateFrom.Year < Year && dateTo.Year == Year)
                        dateFrom = new DateTime(Year, 1, 1);

                    if (dateFrom.Year == Year && dateTo.Year > Year)
                        dateTo = new DateTime(Year, 12, 31);

                    if (dateFrom.Year != Year && dateTo.Year != Year)
                        continue;
                }

                // get the holiday type
                var holidayType =
                    (Type)
                    Enum.Parse(typeof(Type),
                        holiday.Elements().First(e => e.Name.ToString().ToLower() == "type").Value, true);

                var name = holiday.Elements().First(e => e.Name.ToString().ToLower() == "name").Value;

                // get the federal state ids (if any)
                XElement stateIds = null;
                var germanFederalStateIds = new List<GermanFederalStates.Id>();
                if (holiday.Elements().Any(e => e.Name.ToString().ToLower() == "publicholidaystateids"))
                {
                    stateIds = holiday.Elements().First(e => e.Name.ToString().ToLower() == "publicholidaystateids");
                    if (stateIds.HasElements)
                        foreach (var stateId in stateIds.Elements())
                            germanFederalStateIds.Add(
                                (GermanFederalStates.Id) Enum.Parse(typeof(GermanFederalStates.Id), stateId.Value,
                                    true));
                }

                // Only with Replace action the dates may be missing
                if (action != ActionType.Replace && (dateFrom == DateTime.MinValue || dateTo == DateTime.MinValue))
                    throw new Exception("Missing 'date from' and/or 'date to' in XML data.");

                while (dateFrom <= dateTo)
                {
                    var tmpDateFrom = new DateTime(dateFrom.Ticks);
                    var germanHoliday = new GermanHoliday(holidayId, holidayType, name, () => tmpDateFrom)
                    {
                        PublicHolidayStateIds = germanFederalStateIds
                    };
                    switch (action)
                    {
                        case ActionType.Merge:
                            Merge(germanHoliday);
                            break;
                        case ActionType.Add:
                            Add(germanHoliday);
                            break;
                        case ActionType.Replace:
                            if (holidayId.HasValue)
                            {
                                // replace the existing standard date only if a new date was given
                                if (tmpDateFrom != DateTime.MinValue)
                                    this[holidayId.Value].DoCalcDate = () => tmpDateFrom;
                                this[holidayId.Value].Type = holidayType;
                                this[holidayId.Value].Name = name;
                                // replace state ids, if any were supplied
                                if (stateIds != null)
                                    this[holidayId.Value].PublicHolidayStateIds = germanFederalStateIds;
                            }

                            break;
                    }

                    dateFrom = dateFrom.AddDays(1);
                }
            }
        }

        #region *** Enums ***

        private enum ActionType
        {
            Add = 1,
            Merge,
            Replace,
            Remove
        }

        public enum Id
        {
            Neujahr = 1,
            HeiligeDreiKoenige,
            RosenMontag,
            FaschingsDienstag,
            AscherMittwoch,
            GruenDonnerstag,
            KarFreitag,
            KarSamstag,
            OsterSonntag,
            OsterMontag,
            Maifeiertag,
            ChristiHimmelfahrt,
            Muttertag,
            PfingstSonntag,
            PfingstMontag,
            Fronleichnam,
            MariaHimmelfahrt,
            AugsburgerFriedensfest,
            TagDerDeutschenEinheit,
            Reformationstag,
            Allerheiligen,
            BussUndBettag,
            Volkstrauertag,
            TotenSonntag,
            Nikolaus,
            Advent1,
            Advent2,
            Advent3,
            Advent4,
            HeiligerAbend,
            Weihnachtsfeiertag1,
            Weihnachtsfeiertag2,
            Silvester
        }

        public enum Type
        {
            Public = 1,
            School,
            Custom,
            Commemoration
        }

        #endregion
    }
}