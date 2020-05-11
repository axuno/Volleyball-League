using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace League.Helpers
{
    /// <summary>
    /// Extension for <see cref="ViewDataDictionary"/> that stores and restores values from it.
    /// </summary>
    public static class ViewDataDictionaryExtensions
    {
        /// <summary>
        /// Sets the value used for the page title tag.
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="title"></param>
        public static void Title(this ViewDataDictionary viewData, string title)
        {
            viewData[nameof(Title)] = title;
        }

        /// <summary>
        /// Gets the value used for the page title tag.
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns>Returns the value used for the page title tag</returns>
        public static string Title(this ViewDataDictionary viewData)
        {
            return viewData[nameof(Title)]?.ToString();
        }

        /// <summary>
        /// Sets the value used for the page meta description tag.
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="description"></param>
        public static void Description(this ViewDataDictionary viewData, string description)
        {
            viewData[nameof(Description)] = description;
        }

        /// <summary>
        /// Gets the value used for the page meta description tag.
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns>Returns the value used for the page meta description tag</returns>
        public static string Description(this ViewDataDictionary viewData)
        {
            return viewData[nameof(Description)]?.ToString();
        }
    }
}
