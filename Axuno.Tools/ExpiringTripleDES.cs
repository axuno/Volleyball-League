using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.Cryptography;


namespace Axuno.Tools
{
	/// <summary>
	/// Encrypt and decrypt strings and byte arrays using TripleDES and Base64
	/// Returned strings are Url-safe (no +/= characters)
	/// </summary>
	/// <remarks>
	/// Inspired by: "Making TripleDES Simple" by Tony Selke
	/// http://www.codeproject.com/
	/// 
	/// Base64 uses "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".
	/// The "+" and "/" characters would disrupt a URL. Therefore, we URL-encode the base64 output.
	/// The Base64 encoding algorithm may also include one or two "=" characters at the very end of 
	/// the encoded data, and this would also disrupt a URL.
	/// Base64 works like this: The input string is processed in 3-byte blocks which are split 
	/// into 6-bits pieces. Hence, we have 2^6 == 64 possible values per piece which translates to a single 
	/// character into our base64 alphabet. So, every three byte block is translated to a 4 characters string. 
	/// When the last block has less than three bytes, it's padded with zeros and one equal sign is added for
	/// every padding byte. Thus base64 encoded strings always have a length divisible by 4. 
	/// </remarks>
	public class ExpiringTripleDES<T> where T : class
	{
		/// <summary>
		/// Class used as a result of decryption by ExpiringTripleDES.
		/// </summary>
		/// <remarks>Generic type arguments are inherited from ExpiringTripleDES.</remarks>
		public class DecryptionResult
		{
			/// <summary>
			/// Gets the starting date and time of the ExpiringTripleDES, which is set after successful decryption.
			/// </summary>
			public DateTime StartsOn { get; internal set; }
			/// <summary>
			/// Gets the expring date and time of the ExpiringTripleDES, which is set after successful decryption.
			/// </summary>
			public DateTime ExpiresOn { get; internal set; }
			/// <summary>
			/// Gets the decrypted text, which is set after successful decryption.
			/// </summary>
			public string RawText { get; internal set; }
			/// <summary>
			/// Gets whether the decryption result is valid.
			/// </summary>
			public bool IsValid { get; internal set; }
			/// <summary>
			/// Gets the Exception that occured during decryption, or null.
			/// </summary>
			public Exception Exception { get; internal set; }
			/// <summary>
			/// Gets whether the decryption date is between start and expiring date
			/// </summary>
			/// <param name="decryptionDate"></param>
			/// <returns>Return true, if the decryption date is between start and expiring date, else false.</returns>
			public bool IsTimeInRange(DateTime decryptionDate)
            {
                return (decryptionDate >= StartsOn) && (decryptionDate <= ExpiresOn);
            }
			/// <summary>
			/// Sets the properties of a class which implements ICryptoConvert based on raw decrypted text,
			/// </summary>
			public T Container
			{ get; internal set; }
		}


		// define the triple des provider
		private readonly TripleDES _tripleDES = TripleDES.Create();

		// define the string handler
		private readonly UTF8Encoding _utf8Encoding = new();

		// characters to use for random Key and IV
		private const string _charsToUse = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		public ExpiringTripleDES()
		{
			_tripleDES.Padding = PaddingMode.PKCS7;
			GenerateKeyAndIV();
		}

		public ExpiringTripleDES(byte[] key, byte[] iv)
		{
			_tripleDES.Key = key;
			_tripleDES.IV = iv;
		}

		public ExpiringTripleDES(string key, string iv)
		{
			Key = key;
			IV = iv;
		}

        /// <summary>
        /// Generates random, alphanumeric Initialization Vector and Symmetric Algorithm Key.
        /// </summary>
        /// <remarks>
        /// The use of the Random class makes this unsuitable for anything security related,
        /// such as creating passwords or tokens. Use the RNGCryptoServiceProvider class if a
        /// strong random number generator is required.
        /// </remarks>
        private void GenerateKeyAndIV()
		{
		    var random = new Random();
            IV = new string(Enumerable.Repeat(_charsToUse, 8).Select(s => s[random.Next(s.Length)]).ToArray());
            Key = new string(Enumerable.Repeat(_charsToUse, 24).Select(s => s[random.Next(s.Length)]).ToArray());
		}

		/// <summary>
		/// Gets or set the System.Security.Cryptography.SymmetricAlgorithm.Key (UTF8 is used. Characters exceeding 24 bytes are truncated, less will be filled.)
		/// </summary>
		public string Key
		{
			get { return _utf8Encoding.GetString(_tripleDES.Key); }
			set
			{
				_tripleDES.Key = _utf8Encoding.GetBytes(value + _charsToUse).TakeWhile((b, index) => index < 24).ToArray();
			}
		}

		/// <summary>
		/// Gets or set the System.Security.Cryptography.SymmetricAlgorithm.IV initalization vector (UTF8 is used. Characters exceeding 8 bytes are truncated, less will be filled.)
		/// </summary>
		public string IV
		{
			get { return _utf8Encoding.GetString(_tripleDES.IV); }
			set
			{
				_tripleDES.IV = _utf8Encoding.GetBytes(value + _charsToUse).TakeWhile((b, index) => index < 8).ToArray();
			}
		}

		/// <summary>
		/// Function delegate for converting the fields and properties of the container object
		/// into a text string which shall be encrypted
		/// </summary>
		/// <returns>Returns a string with container fields and properties</returns>
		public Func<T, string> ToText { get; set; }

		/// <summary>
		/// Function delegate for converting a decrypted string back into fields and properties
		/// of the container object. It is the reverse procedure of ToText.
		/// </summary>
		/// <returns>Return the container object with fields and properties assigned from the text</returns>
		public Func<string, T, T> ToContainer { get; set; }

		/// <summary>
		/// Encrypts a class with values with TripleDES and Base64
		/// </summary>
		/// <param name="container">Class with values to encrypt</param>
		/// <param name="startsOn">Starting date and time for the text</param>
		/// <param name="expiresOn">Expiration date and time for the text</param>
		/// <returns>TripleDES encrypted Base64url string</returns>
		public string Encrypt(T container, DateTime? startsOn = null, DateTime? expiresOn = null)
		{
			if (!startsOn.HasValue) startsOn = DateTime.MinValue;
			if (!expiresOn.HasValue) expiresOn = DateTime.MaxValue;

			return Encrypt(ToText(container), startsOn, expiresOn);
		}

		/// <summary>
		/// Encrypts text with TripleDES and Base64
		/// </summary>
		/// <param name="text">Text to encrpyt</param>
		/// <param name="startsOn">Starting date and time for the text</param>
		/// <param name="expiresOn">Expiration date and time for the text</param>
		/// <returns>TripleDES encrypted Base64url string</returns>
		public string Encrypt(string text, DateTime? startsOn = null, DateTime? expiresOn = null)
		{
			if (!startsOn.HasValue) startsOn = DateTime.MinValue;
			if (!expiresOn.HasValue) expiresOn = DateTime.MaxValue;

			var textBytes = _utf8Encoding.GetBytes(text);
			var startsOnBytes = BitConverter.GetBytes(startsOn.Value.Ticks);
			var expiresOnBytes = BitConverter.GetBytes(expiresOn.Value.Ticks);
			var numOfInputBytes = new byte[startsOnBytes.LongLength + expiresOnBytes.LongLength + textBytes.LongLength];

			Array.Copy(startsOnBytes, 0, numOfInputBytes, 0, startsOnBytes.LongLength);
			Array.Copy(expiresOnBytes, 0, numOfInputBytes, startsOnBytes.LongLength, expiresOnBytes.LongLength);
			Array.Copy(textBytes, 0, numOfInputBytes, startsOnBytes.LongLength + expiresOnBytes.LongLength, textBytes.LongLength);

			var output = Transform(numOfInputBytes, _tripleDES.CreateEncryptor());

			// Make the string match RFC4648 Base64url and strip trailing '=' fillers (which are 0, 1 or 2)
			return Convert.ToBase64String(output).Replace('/', '_').Replace('+', '-').TrimEnd('=');
		}

		/// <summary>
		/// Decrypts text
		/// </summary>
		/// <param name="text">TripleDES encrypted Base64url string.</param>
		/// <param name="container">Container which will contain the decryption result.</param>
		/// <returns>Returns the decrypted text, or null if decryption failed or if text is out of the valid time span.</returns>
		public DecryptionResult Decrypt(string text, T container)
		{
			var result = new DecryptionResult();

			try
			{
				// Replace +/ with -_ and add = that were stripped by Url-safe Encryption
				var input = Convert.FromBase64String(text.Replace('_', '/').Replace('-', '+').PadRight(text.Length + (4 - text.Length % 4) % 4, '='));
				var output = Transform(input, _tripleDES.CreateDecryptor());

				// how many bytes are needed to convert from typeof(long)
				var longAsBytesLength = BitConverter.GetBytes((long) 0).Length;

				// first 8 bytes are the startOn ticks
				result.StartsOn = new DateTime(BitConverter.ToInt64(output, 0));
				// next 8 bytes are the expiresOn ticks
				result.ExpiresOn = new DateTime(BitConverter.ToInt64(output, longAsBytesLength));
				
				// The string starts after byte 16
				result.RawText = _utf8Encoding.GetString(output, longAsBytesLength * 2, output.Length - longAsBytesLength * 2);
				result.Container = ToContainer(result.RawText, container);

				result.Exception = null;
				result.IsValid = true;
				return result;
			}
			catch (Exception ex)
			{
				result.Exception = ex;
				result.Container = null;
				result.IsValid = false;
				result.RawText = string.Empty;
				return result;
			}
		}

		private static byte[] Transform(byte[] input, ICryptoTransform CryptoTransform)
		{
			// create the necessary streams
			var memStream = new MemoryStream();
			var cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write);

			// transform the bytes as requested
			cryptStream.Write(input, 0, input.Length);
			cryptStream.FlushFinalBlock();

			// Read the memory stream and
			// convert it back into byte array
			memStream.Position = 0;
			var result = memStream.ToArray();

			// close and release the streams
			memStream.Close();
			cryptStream.Close();

			// hand back the encrypted buffer
			return result;
		}
	}
}