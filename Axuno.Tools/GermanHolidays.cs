using System.Globalization;
using System.Xml.Linq;

namespace Axuno.Tools;

public class GermanHolidays : List<GermanHoliday>
{
    private DateTime _easterSunday;

    /// <summary>
    /// CTOR. Creates an empty list of <see cref="GermanHoliday"/>.
    /// </summary>
    /// <param name="year">Year to use for holiday calculations (1583 - 4099).</param>
    public GermanHolidays(int year)
    {
        // Plausibility check
        if (year < 1583 || year > 4099)
            throw new ArgumentException("Year must be between 1583 and 4099.", nameof(year));

        Year = year;
    }

    /// <summary>
    /// Adds a collection of German public holidays and commemoration days to the list of <see cref="GermanHoliday"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void GenerateGermanHolidays()
    {
        _easterSunday = GetEasterSunday();

        Add(new(Id.Neujahr, Type.Public, "Neujahr", () => new(Year, 1, 10, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.HeiligeDreiKoenige, Type.Public, "Heilige Drei Kˆnige",
            () => new(Year, 1, 6, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.RosenMontag, Type.Commemoration, "Rosenmontag", () => _easterSunday.AddDays(-48)));
        Add(new(Id.FaschingsDienstag, Type.Commemoration, "Faschingsdienstag",
            () => _easterSunday.AddDays(-47)));
        Add(new(Id.AscherMittwoch, Type.Commemoration, "Aschermittwoch",
            () => _easterSunday.AddDays(-46)));
        Add(new(Id.GruenDonnerstag, Type.Commemoration, "Gr¸ndonnerstag",
            () => _easterSunday.AddDays(-3)));
        Add(new(Id.KarFreitag, Type.Public, "Karfreitag", () => _easterSunday.AddDays(-2)));
        Add(new(Id.KarSamstag, Type.Commemoration, "Karsamstag", () => _easterSunday.AddDays(-1)));
        Add(new(Id.OsterSonntag, Type.Public, "Ostersonntag", () => _easterSunday));
        Add(new(Id.OsterMontag, Type.Public, "Ostermontag", () => _easterSunday.AddDays(1)));
        Add(new(Id.Maifeiertag, Type.Public, "Maifeiertag", () => new(Year, 5, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.ChristiHimmelfahrt, Type.Public, "Christi Himmelfahrt",
            () => _easterSunday.AddDays(39)));
        Add(new(Id.Muttertag, Type.Commemoration, "Muttertag", GetMuttertag));
        Add(new(Id.PfingstSonntag, Type.Public, "Pfingstsonntag", () => _easterSunday.AddDays(49)));
        Add(new(Id.PfingstMontag, Type.Public, "Pfingstmontag", () => _easterSunday.AddDays(50)));
        Add(new(Id.Fronleichnam, Type.Public, "Fronleichnam", () => _easterSunday.AddDays(60)));
        Add(new(Id.MariaHimmelfahrt, Type.Public, "Maria Himmelfahrt",
            () => new(Year, 8, 15, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.AugsburgerFriedensfest, Type.Commemoration, "Augsburger Friedensfest",
            () => new(Year, 8, 8, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.TagDerDeutschenEinheit, Type.Public, "Tag der deutschen Einheit",
            () => new(Year, 10, 3, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.Reformationstag, Type.Public, "Reformationstag",
            () => new(Year, 10, 31, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.Allerheiligen, Type.Public, "Allerheiligen", () => new(Year, 11, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.BussUndBettag, Type.Public, "Buﬂ- und Bettag", GetBussUndBettag));
        Add(new(Id.Volkstrauertag, Type.Commemoration, "Volkstrauertag",
            () => GetAdventDate(1).AddDays(-14)));
        Add(new(Id.TotenSonntag, Type.Commemoration, "Totensonntag",
            () => GetAdventDate(1).AddDays(-7)));
        Add(new(Id.Nikolaus, Type.Commemoration, "Nikolaus", () => new(Year, 12, 6, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.Advent1, Type.Commemoration, "1. Advent", () => GetAdventDate(1)));
        Add(new(Id.Advent2, Type.Commemoration, "2. Advent", () => GetAdventDate(2)));
        Add(new(Id.Advent3, Type.Commemoration, "3. Advent", () => GetAdventDate(3)));
        Add(new(Id.Advent4, Type.Commemoration, "4. Advent", () => GetAdventDate(4)));
        Add(new(Id.HeiligerAbend, Type.Commemoration, "Heiliger Abend",
            () => new(Year, 12, 24, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.Weihnachtsfeiertag1, Type.Public, "1. Weihnachtsfeiertag",
            () => new(Year, 12, 25, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.Weihnachtsfeiertag2, Type.Public, "2. Weihnachtsfeiertag",
            () => new(Year, 12, 26, 0, 0, 0, DateTimeKind.Unspecified)));
        Add(new(Id.Silvester, Type.Commemoration, "Silvester", () => new(Year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified)));
#if DEBUG
        // Make sure every holiday in enum HolidayId is handled in this collection
        foreach (var holidayId in Enum.GetValues<Id>())
            try
            {
                _ = Contains(this[holidayId]!);
            }
            catch
            {
                throw new InvalidOperationException($"HolidayId \"{holidayId}\" was not processed in \"{GetType()}\".");
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
    public GermanHoliday? this[Id holidayId]
    {
        get
        {
            bool HolidayFilter(GermanHoliday h) => h.Id.HasValue && h.Id.Value == holidayId;
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
        var remainder19 = Year % 19;

        // Calculate Paschal Full Moon
        var temp = (firstDigits - 15) / 2 + 202 - 11 * remainder19;
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
        if (temp == 28 && remainder19 > 10)
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

        return new(Year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Calculates the date of the required Advent
    /// </summary>
    /// <param name="num">Advent (1 to 4)</param>
    /// <returns>Advent date</returns>
    private DateTime GetAdventDate(int num)
    {
        if (num is < 1 or > 4)
            throw new InvalidOperationException("Only Advents 1 to 4 are allowed.");

        // 4th Advent is the latest Sunday before 25th December
        var firstChristmasDay = new DateTime(Year, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);

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
        if (germanHoliday.CalcDateFunc().Year == Year)
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
        var muttertag = new DateTime(Year, 5, 1, 0, 0, 0, DateTimeKind.Unspecified).AddDays(7);
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
            // national holidays, are those where all federal states have the same public holidays defined
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
                publicHolidayStates.Add(GermanFederalStates.Id.SachsenAnhalt);
                publicHolidayStates.Add(GermanFederalStates.Id.Sachsen);
                publicHolidayStates.Add(GermanFederalStates.Id.Thueringen);
                // since 2018:
                publicHolidayStates.Add(GermanFederalStates.Id.Bremen);
                publicHolidayStates.Add(GermanFederalStates.Id.Hamburg);
                publicHolidayStates.Add(GermanFederalStates.Id.Niedersachsen);
                publicHolidayStates.Add(GermanFederalStates.Id.SchleswigHolstein);
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
    /// All element and attribute names/values must match XML standards, but are not case-sensitive.
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
        var holidays = XElement.Load(path).Elements("Holiday");

        foreach (var holiday in holidays)
        {
            var holidayId = GetHolidayId(holiday);
            var action = GetActionType(holiday);
            var (dateFrom, dateTo) = GetDateRange(holiday);
            var holidayType = GetHolidayType(holiday);
            var name = holiday.Element("Name")?.Value ?? string.Empty;
            var stateIds = GetStateIds(holiday);

            ProcessHoliday(holidayId, action, dateFrom, dateTo, holidayType, name, stateIds);
        }
    }

    private static Id? GetHolidayId(XElement holiday)
    {
        if (holiday.Attribute("Id") != null && Enum.TryParse<Id>(holiday.Attribute("Id")?.Value, true, out var id))
            return id;
        return null;
    }

    private static ActionType GetActionType(XElement holiday)
    {
        if (Enum.TryParse<ActionType>(holiday.Attribute("Action")?.Value, true, out var action))
            return action;
        return ActionType.Merge;
    }

    private static (DateTime, DateTime) GetDateRange(XElement holiday)
    {
        var dateFrom = DateTime.MinValue;
        var dateTo = DateTime.MinValue;

        if (holiday.Element("DateFrom") != null && holiday.Element("DateTo") != null)
        {
            dateFrom = DateTime.ParseExact(holiday.Element("DateFrom")?.Value!, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            dateTo = DateTime.ParseExact(holiday.Element("DateTo")?.Value!, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);

            if (dateFrom > dateTo)
                (dateFrom, dateTo) = (dateTo, dateFrom);
        }

        return (dateFrom, dateTo);
    }

    private static Type GetHolidayType(XElement holiday)
    {
        if (Enum.TryParse<Type>(holiday.Element("Type")?.Value, true, out var type))
            return type;
        throw new InvalidOperationException("Missing or invalid holiday type.");
    }

    private static List<GermanFederalStates.Id> GetStateIds(XElement holiday)
    {
        var stateIds = new List<GermanFederalStates.Id>();

        var publicHolidayStateIds = holiday.Element("PublicHolidayStateIds");
        if (publicHolidayStateIds is { HasElements: true })
        {
            stateIds.AddRange(publicHolidayStateIds.Elements().Select(stateId =>
                Enum.Parse<GermanFederalStates.Id>(stateId.Value, true)));
        }

        return stateIds;
    }

    private void ProcessHoliday(Id? holidayId, ActionType action, DateTime dateFrom, DateTime dateTo, Type holidayType,
        string name, List<GermanFederalStates.Id> stateIds)
    {
        if (action == ActionType.Remove && holidayId.HasValue)
        {
            RemoveHoliday(holidayId.Value);
            return;
        }

        ValidateDateRange(action, dateFrom, dateTo);

        var dateIsSet = dateFrom != DateTime.MinValue && dateTo != DateTime.MinValue;
        if (holidayId.HasValue && !dateIsSet && Exists(h => h.Id == holidayId))
        {
            dateFrom = dateTo = this[holidayId.Value]!.CalcDateFunc();
        }

        while (dateFrom <= dateTo)
        {
            var tmpDateFrom = dateFrom; // no capture of modified closure
            var germanHoliday = new GermanHoliday(holidayId, holidayType, name, () => tmpDateFrom)
            {
                PublicHolidayStateIds = stateIds
            };

            switch (action)
            {
                case ActionType.Merge:
                    MergeHoliday(germanHoliday);
                    break;
                case ActionType.Add:
                    AddHoliday(germanHoliday);
                    break;
                case ActionType.Replace:
                    ReplaceHoliday(holidayId, germanHoliday);
                    break;
            }

            dateFrom = dateFrom.AddDays(1);
        }
    }

    private static void ValidateDateRange(ActionType action, DateTime dateFrom, DateTime dateTo)
    {
        if (action != ActionType.Replace && (dateFrom == DateTime.MinValue || dateTo == DateTime.MinValue))
            throw new InvalidOperationException("Missing 'date from' and/or 'date to' in XML data.");
    }

    private void RemoveHoliday(Id holidayId)
    {
        Remove(this[holidayId]!.Name);
    }

    private void MergeHoliday(GermanHoliday germanHoliday)
    {
        Merge(germanHoliday);
    }

    private void AddHoliday(GermanHoliday germanHoliday)
    {
        Add(germanHoliday);
    }

    private void ReplaceHoliday(Id? holidayId, GermanHoliday newHoliday)
    {
        if (!holidayId.HasValue || this[holidayId.Value] == null)
            throw new InvalidOperationException("Holiday to replace not found.");
        
        var existingHoliday = this[holidayId.Value]!;

        // Replace the existing holiday with the new one
        existingHoliday.CalcDateFunc = newHoliday.CalcDateFunc;
        existingHoliday.Type = newHoliday.Type;
        existingHoliday.Name = newHoliday.Name;
        existingHoliday.PublicHolidayStateIds = newHoliday.PublicHolidayStateIds;
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
