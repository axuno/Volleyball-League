using System.Collections.Generic;

namespace League.DI
{
    [YAXLib.YAXSerializeAs("SiteList")]
    public class SiteList : List<Site>
    {
        /// <summary>
        /// Deserializes a list of <see cref="Site"/> from an XML file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Returns an instance of <see cref="SiteList"/>.</returns>
        public static SiteList DeserializeFromFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(SiteList));
            return (SiteList)s.DeserializeFromFile(filename);
        }

        /// <summary>
        /// Serializes the <see cref="SiteList"/> to an XML file.
        /// </summary>
        /// <param name="filename"></param>
        public void SerializeToFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(SiteList));
            s.SerializeToFile(this, filename);
        }
    }
}