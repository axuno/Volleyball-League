namespace Axuno.Tools;

/// <summary>
/// This class represents a German holiday.
/// </summary>
public class GermanHoliday
{
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="id">A well-known <see cref="GermanHolidays.Id"/> or <see langword="null"/> </param>
    /// <param name="type">HolidayType</param>
    /// <param name="name">Holiday name</param>
    /// <param name="calcDateFunc">Function for date calculation</param>
    internal GermanHoliday(GermanHolidays.Id? id, GermanHolidays.Type type, string name,
        Func<DateTime> calcDateFunc)
    {
        Id = id;
        Type = type;
        Name = name;
        CalcDateFunc = calcDateFunc;

        PublicHolidayStateIds = id.HasValue
            ? GermanHolidays.GetPublicHolidayStates(id.Value)
            : new List<GermanFederalStates.Id>();
    }

    /// <summary>
    /// Constructor for usage from class GermanHolidays.
    /// </summary>
    /// <param name="id">HolidayId</param>
    /// <param name="type">HolidayType</param>
    /// <param name="name">Holiday name</param>
    /// <param name="calcDateFunc">Function for date calculation</param>
    internal GermanHoliday(GermanHolidays.Id id, GermanHolidays.Type type, string name, Func<DateTime> calcDateFunc)
        : this((GermanHolidays.Id?) id, type, name, calcDateFunc)
    {
    }

    /// <summary>
    /// Constructor for creating custom holidays.
    /// </summary>
    /// <param name="name">Holiday name</param>
    /// <param name="calcDateFunc">Function for date calculation</param>
    public GermanHoliday(string name, Func<DateTime> calcDateFunc)
        : this(null, GermanHolidays.Type.Custom, name, calcDateFunc)
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
    /// Function to use for the calculation of the holiday.
    /// </summary>
    /// <returns>Date of the holiday.</returns>
    public Func<DateTime> CalcDateFunc { get; set; }

    /// <summary>
    /// List of federal states where this is a holiday.
    /// </summary>
    public List<GermanFederalStates.Id> PublicHolidayStateIds { get; set; }

    /// <summary>
    /// Calls the function which does the calculation of the holiday.
    /// </summary>
    public DateTime Date => CalcDateFunc();

    /// <summary>
    /// Checks whether this is a holiday specific to a federal state.
    /// </summary>
    /// <param name="stateId">The federal state id to check.</param>
    /// <returns>Return true, if this is a holiday of this federal state, else false.</returns>
    public bool IsPublicHoliday(GermanFederalStates.Id stateId)
    {
        return PublicHolidayStateIds.Exists(s => s == stateId);
    }

    // Note: We don't have operator overloading for == and != because they should not be used on reference types.
    
    public static bool operator <(GermanHoliday? h1, GermanHoliday? h2)
    {
        if (h1 is null || h2 is null) return false;
        return h1.Date < h2.Date && string.Compare(h1.Name, h2.Name, StringComparison.Ordinal) < 0;
    }

    public static bool operator >(GermanHoliday? h1, GermanHoliday? h2)
    {
        if (h1 is null || h2 is null) return false;
        return h1.Date > h2.Date && string.Compare(h1.Name, h2.Name, StringComparison.Ordinal) > 0;
    }

    protected bool Equals(GermanHoliday other)
    {
        return Id == other.Id && Type == other.Type && Name == other.Name &&
               Equals(PublicHolidayStateIds, other.PublicHolidayStateIds) && Equals(CalcDateFunc, other.CalcDateFunc);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((GermanHoliday) obj);
    }

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(Id, (int) Type, Name, PublicHolidayStateIds, CalcDateFunc);
    }
}
