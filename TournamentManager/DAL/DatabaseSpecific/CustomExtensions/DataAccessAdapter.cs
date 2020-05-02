using System;
using System.Linq;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace TournamentManager.DAL.DatabaseSpecific
{
    public partial class DataAccessAdapter
    {
        /// <summary>
        /// Gets the source column name for an <see cref="IEntityField2"/> in the persistent storage.
        /// </summary>
        /// <param name="field">The field to get the column name for.</param>
        /// <returns>Returns the source column name for a <see cref="IEntityField2"/> in the persistent storage.</returns>
        public string GetPersistentFieldName(IEntityField2 field)
        {
            var i = GetFieldPersistenceInfo(field);
            return i.SourceColumnName;
        }

        /// <summary>
        /// Gets the source table name of an <see cref="IEntity2"/> belongs to in the persistent storage.
        /// </summary>
        /// <param name="entity">The <see cref="IEntity2"/> to get the table name for.</param>
        /// <returns>Returns the source table name an <see cref="IEntity2"/> belongs to in the persistent storage.</returns>
        public string GetPersistentTableName(IEntity2 entity)
        {
            var i = GetFieldPersistenceInfo(entity.PrimaryKeyFields.First());
            return i.SourceObjectName;
        }
    }
}

