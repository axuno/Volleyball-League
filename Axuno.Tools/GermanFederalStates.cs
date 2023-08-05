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
    public enum Id : int
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
        Add(new GermanFederalState(Id.BadenWuerttemberg, "BW", "Baden-Württemberg"));
        Add(new GermanFederalState(Id.Bayern, "BY", "Bayern"));
        Add(new GermanFederalState(Id.Berlin, "BE", "Berlin"));
        Add(new GermanFederalState(Id.Brandenburg, "BB", "Brandenburg"));
        Add(new GermanFederalState(Id.Bremen, "BR", "Bremen"));
        Add(new GermanFederalState(Id.Hamburg, "HH", "Hamburg"));
        Add(new GermanFederalState(Id.Hessen, "HE", "Hessen"));
        Add(new GermanFederalState(Id.MecklenburgVorpommern, "MV", "Mecklenburg-Vorpommern"));
        Add(new GermanFederalState(Id.Niedersachsen, "NI", "Niedersachsen"));
        Add(new GermanFederalState(Id.NordrheinWestfalen, "NW", "Nordrhein-Westfalen"));
        Add(new GermanFederalState(Id.RheinlandPfalz, "RP", "Rheinland-Pfalz"));
        Add(new GermanFederalState(Id.Saarland, "SL", "Saarland"));
        Add(new GermanFederalState(Id.Sachsen, "SN", "Sachsen"));
        Add(new GermanFederalState(Id.SachsenAnhalt, "ST", "Sachsen-Anhalt"));
        Add(new GermanFederalState(Id.SchleswigHolstein, "SH", "Schleswig-Holstein"));
        Add(new GermanFederalState(Id.Thueringen, "TH", "Thüringen"));

#if DEBUG
        // Make sure every state in enum GermanFederalStateId is handled in this collection
        foreach (Id stateId in Enum.GetValues(typeof(Id)))
        {
            try
            {
                _ = Contains(this[stateId]);
            }
            catch
            {
                throw new Exception($"GermanFederalStateId \"{stateId}\" is not included in \"{GetType()}\".");
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
