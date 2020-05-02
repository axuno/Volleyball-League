using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace League.Helpers
{
    public class MetaDataHelper
    {
        private readonly IModelMetadataProvider _metadataProvider;

        public MetaDataHelper(IModelMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
        }

        /// <summary>
        /// Get the (not localized) DisplayName for the property of a model with <see cref="System.ComponentModel.DataAnnotations"/>.
        /// </summary>
        /// <typeparam name="T">The type of the class.</typeparam>
        /// <param name="fieldName">The field name to get the raw (not localized) DisplayName.</param>
        /// <example>
        /// var displayName = MetaDataHelper.GetDisplayName(typeof(ChangeUsernameViewModel), nameof(ChangeUsernameViewModel.Username));
        /// </example>
        /// <returns>Returns the DisplayName, or NULL if the name was not found.</returns>
        public string GetRawDisplayName<T>(string fieldName)
        {
            // First look into attributes of a type and its parents
            // Note: There is also a DisplayNameAttribute
            var attr = typeof(T).GetProperty(fieldName)?.GetCustomAttribute<DisplayAttribute>(true);
            return attr?.Name;
        }

        /// <summary>
        /// Get the localized DisplayName for the property of a model with <see cref="System.ComponentModel.DataAnnotations"/>.
        /// </summary>
        /// <typeparam name="T">The type of the class.</typeparam>
        /// <param name="propertyName">The field name to get the localized DisplayName.</param>
        /// <returns>Returns the localized DisplayName, or NULL if the name was not found</returns>
        public string GetDisplayName<T>(string propertyName)
        {
            var mdp = _metadataProvider.GetMetadataForProperties(typeof(T));
            return mdp.FirstOrDefault(d => d.Name == propertyName)?.DisplayName; // localized if resource file is present
        }
    }
}