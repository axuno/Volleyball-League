using System.Collections.Generic;

namespace TournamentManager.Data
{
    /// <summary>
    /// Contains a list of <see cref="DbContext"/>, which can be serialized and deserialized from/to an XML file.
    /// </summary>
    [YAXLib.YAXSerializeAs("DbContextList")]
    public class DbContextList : List<DbContext>
    {
        /// <summary>
        /// Deserializes an XML file to an instance of <see cref="DbContextList"/>.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DbContextList DeserializeFromFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(DbContextList));
            return (DbContextList)s.DeserializeFromFile(filename);
        }

        /// <summary>
        /// Deserializes an instance of <see cref="DbContextList"/> to an XML file.
        /// </summary>
        /// <param name="filename"></param>
        public void SerializeToFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(DbContextList));
            s.SerializeToFile(this, filename);
        }
    }
}