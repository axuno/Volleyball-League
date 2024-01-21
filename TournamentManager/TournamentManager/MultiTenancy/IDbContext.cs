using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// Interface for database-specific data and methods.
/// </summary>
public interface IDbContext
{
    /// <summary>
    /// Gets or sets the <see cref="ITenant"/> this context refers to.
    /// </summary>
    ITenant? Tenant { get; set; }
        
    /// <summary>
    /// The connection key used to retrieve the <see cref="ConnectionString"/>.
    /// </summary>
    string ConnectionKey { get; set; }

    /// <summary>
    /// The connection string for the database.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// The catalog aka database name.
    /// </summary>
    string Catalog { get; set; }

    /// <summary>
    /// The schema inside the database.
    /// </summary>
    string Schema { get; set; }

    /// <summary>
    /// Gets or sets the timeout value to use with the command object(s) created by <see cref="IDataAccessAdapter"/>s.
    /// Default is 30 seconds
    /// </summary>
    /// <remarks>
    /// Set this prior to calling a method which executes database logic.
    /// </remarks>
    int CommandTimeOut { get; set; }

    /// <summary>
    /// Gives access to the repositories.
    /// </summary>
    IAppDb AppDb { get; }

    /// <summary>
    /// Gets a new instance of an <see cref="IDataAccessAdapter"/> which will be used to access repositories.
    ///  </summary>
    /// <returns>Returns a new instance of an <see cref="IDataAccessAdapter"/> which will be used to access repositories.</returns>
    IDataAccessAdapter GetNewAdapter();
}
