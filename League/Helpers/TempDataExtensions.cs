using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace League.Helpers;

/// <summary>
/// Extension for <see cref="ITempDataDictionary"/> that stores and restores values from it.
/// </summary>
public static class TempDataExtensions
{
    /// <summary>
    /// Stores the instance of type <see ref="T"/> to the <see cref="ITempDataDictionary"/>.
    /// The type must be serializable by <see cref="JsonConvert"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tempData"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonConvert.SerializeObject(value);
    }

    /// <summary>
    /// Restore a new instance of type <see ref="T"/> from the <see cref="ITempDataDictionary"/>.
    /// The type must be deserializable by <see cref="JsonConvert"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tempData"></param>
    /// <param name="key"></param>
    /// <returns>Returns a new instance of type <see ref="T"/> from the <see cref="ITempDataDictionary"/></returns>
    public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
    {
        tempData.TryGetValue(key, out var obj);
        return obj == null ? null : JsonConvert.DeserializeObject<T>((string)obj);
    }
}