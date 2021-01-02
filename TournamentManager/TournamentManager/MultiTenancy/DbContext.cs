﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.DatabaseSpecific;

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// Provides database-specific data and methods.
    /// </summary>
    public class DbContext : IDbContext
    {
        private readonly object _locker = new object();
        private bool _cacheIsRegistered = false;

        public DbContext()
        {
            AppDb = new AppDb(this);
        }
        
        /// <summary>
        /// Gets or sets the <see cref="ITenant"/> this context refers to.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public ITenant? Tenant { get; set; }
        
        /// <summary>
        /// The connection key used to retrieve the <see cref="ConnectionString"/>.
        /// </summary>
        [YAXLib.YAXComment("The connection key used to retrieve the ConnectionString")]
        public virtual string ConnectionKey { get; set; } = string.Empty;

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public virtual string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// The catalog aka database name.
        /// </summary>
        [YAXLib.YAXComment("The catalog aka database name")]
        public virtual string Catalog { get; set; } = string.Empty;

        /// <summary>
        /// The schema inside the database.
        /// </summary>
        [YAXLib.YAXComment("The schema inside the database")]
        public virtual string Schema { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the timeout value to use with the command object(s) created by <see cref="IDataAccessAdapter"/>s.
        /// Default is 30 seconds
        /// </summary>
        /// <remarks>
        /// Set this prior to calling a method which executes database logic.
        /// </remarks>
        [YAXLib.YAXComment("The timeout value to use with database commands")]
        public virtual int CommandTimeOut { get; set; } = 30;
        
        /// <summary>
        /// Gets a new instance of an <see cref="IDataAccessAdapter"/> which will be used to access repositories.
        ///  </summary>
        /// <returns>Returns a new instance of an <see cref="IDataAccessAdapter"/> which will be used to access repositories.</returns>
        public virtual IDataAccessAdapter GetNewAdapter()
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
        
        /// <summary>
        /// Gives access to the repositories.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public virtual AppDb AppDb { get; }
    }
}