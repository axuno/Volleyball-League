using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.DatabaseSpecific;
using YAXLib;

namespace TournamentManager.Data
{
    /// <summary>
    /// Provides database-specific settings.
    /// Usually instances are part of a <see cref="DbContextList"/>, created from a configuration file.
    /// </summary>
    public class DbContext : IDbContext
    {
        private readonly object _locker = new object();
        private bool _cacheIsRegistered = false;

        /// <summary>
        /// The organization key the settings refer to.
        /// </summary>
        public string OrganizationKey { get; set; } = string.Empty;
        /// <summary>
        /// The connection key used to retrieve the <see cref="ConnectionString"/>.
        /// </summary>
        public string ConnectionKey { get; set; }
        /// <summary>
        /// The connection string for the database.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public string ConnectionString { get; set; }
        /// <summary>
        /// The catalog aka database name.
        /// </summary>
        public string Catalog { get; set; }
        /// <summary>
        /// The schema inside the database.
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Gets or sets the timeout value to use with the command object(s) created by <see cref="IDataAccessAdapter"/>s.
        /// Default is 30 seconds
        /// </summary>
        /// <remarks>
        /// Set this prior to calling a method which executes database logic.
        /// </remarks>
        [YAXLib.YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int CommandTimeOut { get; set; } = 30;
        /// <summary>
        /// Gets a new instance of an <see cref="IDataAccessAdapter"/> which will be used to access repositories.
        ///  </summary>
        /// <returns>Returns a new instance of an <see cref="IDataAccessAdapter"/> which will be used to access repositories.</returns>
        public IDataAccessAdapter GetNewAdapter()
        {
            lock (_locker)
            {
                if (!_cacheIsRegistered)
                {
                    var csb = new SqlConnectionStringBuilder(ConnectionString);
                    // see docs: https://www.llblgen.com/Documentation/4.2/LLBLGen%20Pro%20RTF/Using%20the%20generated%20code/gencode_resultsetcaching.htm
                    if (csb.PersistSecurityInfo == false) { csb.Password = string.Empty; }
                    // if connection string exists, the method simply returns without creating a new cache
                    CacheController.RegisterCache(csb.ConnectionString, new ResultsetCache(TimeSpan.FromDays(1).Seconds));
                    _cacheIsRegistered = true;
                }
                
                return new DataAccessAdapter(ConnectionString)
                {
                    KeepConnectionOpen = true,
                    CompatibilityLevel = SqlServerCompatibilityLevel.SqlServer2012,
                    CommandTimeOut = CommandTimeOut,
                    CatalogNameOverwrites =
                        new CatalogNameOverwriteHashtable(new Dictionary<string, string> { { "*", Catalog } })
                        {
                            CatalogNameUsageSetting = CatalogNameUsage.ForceName
                        },
                    SchemaNameOverwrites =
                        new SchemaNameOverwriteHashtable(new Dictionary<string, string> { { "*", Schema } })
                        {
                            SchemaNameUsageSetting = SchemaNameUsage.ForceName
                        }
                };
            }
        }
    }
}