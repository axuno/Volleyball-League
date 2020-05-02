﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Axuno.Tools
{
    public sealed class GermanFederalState
    {
        private readonly GermanFederalStates.Id _state;
        private readonly string _abbreviation;
        private readonly string _name;

        public GermanFederalStates.Id StateId
        {
            get { return _state; }
        }

        public string Abbreviation
        {
            get { return _abbreviation; }
        }

        public string Name
        {
            get { return _name; }
        }

        public GermanFederalState(GermanFederalStates.Id state, string abbreviation, string name)
        {
            _state = state;
            _abbreviation = abbreviation;
            _name = name;
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
                    Contains(this[stateId]);
                }
                catch
                {
                    throw new Exception(string.Format("GermanFederalStateId \"{0}\" wird in \"{1}\" nicht abgebildet.", stateId, GetType()));
                }
            }
#endif
        }

        /// <summary>
        /// Get a GermanFederalState object.
        /// </summary>
        /// <param name="stateId">State id.</param>
        /// <returns>Returns the GermanFederalState for the id.</returns>
        public GermanFederalState this[Id stateId]
        {
            get
            {
                return this.First(fs => fs.StateId == stateId);
            }
        }

        /// <summary>
        /// Get a GermanFederalState object.
        /// </summary>
        /// <param name="nameOrAbbreviation">State abbriviation or state name.</param>
        /// <returns>Returns the GermanFederalState for the state abbriviation or state name.</returns>
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
}
