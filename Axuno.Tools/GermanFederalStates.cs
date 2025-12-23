using System.Collections.ObjectModel;

namespace Axuno.Tools;

public sealed class GermanFederalState
{
    public GermanFederalStates.Id StateId { get; }

    public string Abbreviation { get; }

    public string Name { get; }

    public GermanFederalState(GermanFederalStates.Id state, string abbreviation, string name)
    {
        StateId = state;
        Abbreviation = abbreviation;
        Name = name;
    }
}

public sealed class GermanFederalStates : Collection<GermanFederalState>
{
    public enum Id
    {
        BadenWuerttemberg,
        Bayern,
        Berlin,
        Brandenburg,
        Bremen,
        Hamburg,
        Hessen,
        MecklenburgVorpommern,
        Niedersachsen,
        NordrheinWestfalen,
        RheinlandPfalz,
        Saarland,
        Sachsen,
        SachsenAnhalt,
        SchleswigHolstein,
        Thueringen
    }

    public GermanFederalStates()
    {
        Add(new(Id.BadenWuerttemberg, "BW", "Baden-Württemberg"));
        Add(new(Id.Bayern, "BY", "Bayern"));
        Add(new(Id.Berlin, "BE", "Berlin"));
        Add(new(Id.Brandenburg, "BB", "Brandenburg"));
        Add(new(Id.Bremen, "BR", "Bremen"));
        Add(new(Id.Hamburg, "HH", "Hamburg"));
        Add(new(Id.Hessen, "HE", "Hessen"));
        Add(new(Id.MecklenburgVorpommern, "MV", "Mecklenburg-Vorpommern"));
        Add(new(Id.Niedersachsen, "NI", "Niedersachsen"));
        Add(new(Id.NordrheinWestfalen, "NW", "Nordrhein-Westfalen"));
        Add(new(Id.RheinlandPfalz, "RP", "Rheinland-Pfalz"));
        Add(new(Id.Saarland, "SL", "Saarland"));
        Add(new(Id.Sachsen, "SN", "Sachsen"));
        Add(new(Id.SachsenAnhalt, "ST", "Sachsen-Anhalt"));
        Add(new(Id.SchleswigHolstein, "SH", "Schleswig-Holstein"));
        Add(new(Id.Thueringen, "TH", "Thüringen"));

#if DEBUG
        // Make sure every state in enum GermanFederalStateId is handled in this collection
        foreach (var stateId in Enum.GetValues<Id>())
        {
            try
            {
                _ = Contains(this[stateId]);
            }
            catch
            {
                throw new InvalidOperationException($"GermanFederalStateId \"{stateId}\" is not included in \"{GetType()}\".");
            }
        }
#endif
    }

    /// <summary>
    /// Get a GermanFederalState object.
    /// </summary>
    /// <param name="stateId">State id.</param>
    /// <returns>Returns the GermanFederalState for the id.</returns>
    public GermanFederalState this[Id stateId] => this.First(fs => fs.StateId == stateId);

    /// <summary>
    /// Get a GermanFederalState object.
    /// </summary>
    /// <param name="nameOrAbbreviation">State abbreviation or state name.</param>
    /// <returns>Returns the GermanFederalState for the state abbreviation or state name.</returns>
    public GermanFederalState this[string nameOrAbbreviation]
    {
        get
        {
            if (this.Any(fs => fs.Abbreviation == nameOrAbbreviation))
                return this.First(fs => fs.Abbreviation == nameOrAbbreviation);

            return this.First(fs => fs.Name == nameOrAbbreviation);
        }
    }
}
