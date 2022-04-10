using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace League.DataProtection
{
    /// <summary>
    /// <see cref="DataProtector"/> encrypts and decrypts objects using the <see cref="IDataProtectionProvider"/>.
    /// </summary>
    public class DataProtector
    {
        private readonly IDataProtector _protector;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="provider">The <see cref="IDataProtectionProvider"/> to use.</param>
        public DataProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(GetType().FullName);
        }

        /// <summary>
        /// Encrypts an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">An object which must be serializable with <see cref="JsonConvert"/>.</param>
        /// <returns>Returns an encrypted string representation of the object.</returns>
        public string Encrypt<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);

            return Encrypt(json);
        }

        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="plaintext">The string to encrypt.</param>
        /// <returns>Returns an encrypted string representation.</returns>
        public string Encrypt(string plaintext)
        {
            return _protector.Protect(plaintext);
        }

        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="plaintext">The string to encrypt.</param>
        /// <param name="expiration">The expiration of the protected string.</param>
        /// <returns>Returns an encrypted string representation.</returns>
        public string Encrypt(string plaintext, DateTimeOffset expiration)
        {
            return _protector.ToTimeLimitedDataProtector().Protect(plaintext, expiration);
        }

        /// <summary>
        /// Tries to decrypt the encrypted string to an object of the given type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="encryptedText">The string to decrypt.</param>
        /// <param name="obj">The decrypted object, if the decryption succeeded.</param>
        /// <returns>Returns <c>true</c> if the decryption succeeded, else <c>false</c>.</returns>
        public bool TryDecrypt<T>(string encryptedText, out T obj)
        {
            if (TryDecrypt(encryptedText, out var json))
            {
                obj = JsonConvert.DeserializeObject<T>(json);
                return true;
            }

            obj = default;
            return false;
        }

        /// <summary>
        /// Tries to decrypt the encrypted string.
        /// </summary>
        /// <param name="encryptedText">The string to decrypt.</param>
        /// <param name="decryptedText">The decrypted string, if the decryption succeeded.</param>
        /// <returns>Returns <c>true</c> if the decryption succeeded, else <c>false</c>.</returns>
        public bool TryDecrypt(string encryptedText, out string decryptedText)
        {
            try
            {
                decryptedText = _protector.Unprotect(encryptedText);
                return true;
            }
            catch (CryptographicException)
            {
                decryptedText = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to decrypt the encrypted string having an expiration time.
        /// </summary>
        /// <param name="encryptedText">The string to decrypt.</param>
        /// <param name="decryptedText">The decrypted string, if the decryption succeeded.</param>
        /// <param name="expiration">The expiration time of the decrypted string.</param>
        /// <returns>Returns <c>true</c> if not yet expired and the decryption succeeded, else <c>false</c>.</returns>
        public bool TryDecrypt(string encryptedText, out string decryptedText, out DateTimeOffset expiration)
        {
            try
            {
                decryptedText = _protector.ToTimeLimitedDataProtector().Unprotect(encryptedText, out expiration);
                return true;
            }
            catch (CryptographicException)
            {
                decryptedText = null;
                expiration = DateTimeOffset.MinValue;
                return false;
            }
        }
    }
}
