using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TournamentManager.DAL.TypedViewClasses;

public partial class CompletedMatchRow
{
    public class CompletedMatchSet
    {
        public CompletedMatchSet(int home, int guest)
        {
            Home = home;
            Guest = guest;
        }

        [XmlIgnore]
        public int Home { get; set; }
        [XmlIgnore]
        public int Guest { get; set; }
    }

    private List<CompletedMatchSet> _set = null;

    private void FillSet()
    {
        _set = new List<CompletedMatchSet>();
        if (string.IsNullOrWhiteSpace(SetResults))
        {
            return;
        }

        foreach (var set in SetResults.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries))
        {
            var s = set.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length == 2)
            {
                _set.Add(new CompletedMatchSet(int.Parse(s[0]), int.Parse(s[1])));
            }
        }
    }

    [XmlIgnore]
    public List<CompletedMatchSet> Set {
        get
        {
            if (_set == null) FillSet();
            return _set;
        }
    }
}