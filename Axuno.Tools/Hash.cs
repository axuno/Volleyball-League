using System;
using System.Security.Cryptography;
using System.Text;

namespace Axuno.Tools
{
    /// <summary>
    /// Create and verify hashes.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// MD5 hash creation and verification.
        /// </summary>
        public static class Md5
        {
            /// <summary>
            /// Creates a hash of the string parameter.
            /// </summary>
            /// <param name="input">The string to create the hash for.</param>
            /// <param name="encoding">The <see cref="Encoding"/> to use. Default is <see cref="UTF8Encoding"/>.</param>
            /// <returns>Returns the hashed and hex string.</returns>
            public static string GetHash(string input, Encoding encoding = null)
            {
                using var md5 = MD5.Create();
                if (encoding == null) encoding = Encoding.UTF8;
                var inputBytes = encoding.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }

            /// <summary>
            /// Verifies an MD5 hash against a string.
            /// </summary>
            /// <param name="input">The input the hash was created for.</param>
            /// <param name="expectedHash">The which was originally computed for the expected input.</param>
            /// <param name="encoding">The <see cref="Encoding"/> to use. Default is <see cref="UTF8Encoding"/>.</param>
            /// <returns>Returns <c>true</c> if hash of the expected input equals the hash, else <c>false</c></returns>
            public static bool VerifyHash(string input, string expectedHash, Encoding encoding = null)
            {
                var hashOfInput = GetHash(input, encoding);
                var comparer = StringComparer.OrdinalIgnoreCase;

                return 0 == comparer.Compare(hashOfInput, expectedHash);
            }
        }
    }
}
