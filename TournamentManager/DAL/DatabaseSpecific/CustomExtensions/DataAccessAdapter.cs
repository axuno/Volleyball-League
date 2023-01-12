
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses.Adapter;
using TournamentManager.DAL.DatabaseSpecific.CustomExtensions;

namespace TournamentManager.DAL.DatabaseSpecific
{
    public partial class DataAccessAdapter
    {
        private QueryCreationManagerExt _queryCreationManager;

        protected override QueryCreationManager CreateQueryCreationManager(IPersistenceInfoProvider persistenceInfoProvider)
        {
            _queryCreationManager = new QueryCreationManagerExt(this, persistenceInfoProvider);
            return _queryCreationManager;
        }

        /// <summary>
        /// Gets the source column name for an <see cref="IEntityField2"/> in the persistent storage.
        /// </summary>
        /// <param name="field">The field to get the column name for.</param>
        /// <returns>Returns the source column name for a <see cref="IEntityField2"/> in the persistent storage.</returns>
        public string GetPersistentFieldName(IEntityField2 field)
        {
            return _queryCreationManager.GetPersistentFieldName(field);
        }

        /// <summary>
        /// Gets the source table name of an <see cref="IEntity2"/> belongs to in the persistent storage.
        /// </summary>
        /// <param name="entity">The <see cref="IEntity2"/> to get the table name for.</param>
        /// <returns>Returns the source table name an <see cref="IEntity2"/> belongs to in the persistent storage.</returns>
        public string GetPersistentTableName(IEntity2 entity)
        {
            return _queryCreationManager.GetPersistentTableName(entity);
        }
    }
}

