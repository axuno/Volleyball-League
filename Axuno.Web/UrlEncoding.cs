using Microsoft.IdentityModel.Tokens;

namespace Axuno.Web;

public static class UrlEncoding
{
    /// <summary>
    /// Converts a string into a "RFC4648 Base64url-encoded" string, which can safely be used in an URL.
    /// Convert back to the original string with <see cref="Base64UrlDecode"/>
    /// </summary>
    public static string Base64UrlEncode(this string code)
    {
        return Base64UrlEncoder.Encode(code);
    }

    /// <summary>
    /// Converts a string that has been encoded with <see cref="Base64UrlEncode"/> for transmission in an URL into a decoded string.
    /// </summary>
    /// <returns>Returns the decoded string if it could be decoded, otherwise null.</returns>
    public static string? Base64UrlDecode(this string encodedString)
    {
        try
        {
            return Base64UrlEncoder.Decode(encodedString);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
