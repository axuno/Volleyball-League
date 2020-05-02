using System.Collections.Generic;

namespace League.DI
{
    [YAXLib.YAXSerializeAs("OrganizationSiteList")]
    public class OrganizationSiteList : List<OrganizationSite>
    {
        /// <summary>
        /// Deserializes a list of <see cref="OrganizationSite"/> from an XML file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Returns an instance of <see cref="OrganizationSiteList"/>.</returns>
        public static OrganizationSiteList DeserializeFromFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(OrganizationSiteList));
            return (OrganizationSiteList)s.DeserializeFromFile(filename);
        }

        /// <summary>
        /// Serializes the <see cref="OrganizationSiteList"/> to an XML file.
        /// </summary>
        /// <param name="filename"></param>
        public void SerializeToFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(OrganizationSiteList));
            s.SerializeToFile(this, filename);
        }
    }
}