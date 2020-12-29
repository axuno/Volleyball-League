using System;
using System.Security.Cryptography;
using System.Text;

namespace Axuno.Tools.Password
{
	public class Password
	{
		// Depending on the performance requirements, the value can be reduced. 
		// A minimum value should be around 1000:
		private const int _numOfIerations = 1000;

		/// <summary>
		/// Utility class to check passwords.
		/// </summary>
		public static Checker Checker { get { return Checker.Current; } }

		/// <summary>
		/// Utility class to for resetting passwords.
		/// </summary>
		public static Reset Reset { get { return Reset.Current; } }

		/// <summary>
		/// Computes the hashcode for a password using Rfc2898DeriveBytes
		/// and encodes it to a Base64 string (length: 48)
		/// </summary>
		/// <param name="password">User password.</param>
		/// <returns>Returns the hashcode for a password.</returns>
		public static string GetHashCode(string password)
		{
			if (password == null)
				throw new ArgumentNullException("password");

			// Create the salt value with a cryptographic PRNG
			var salt = new byte[16];
			new RNGCryptoServiceProvider().GetBytes(salt);

			// Create the Rfc2898DeriveBytes and get the hash value
			var pbkdf2 = new Rfc2898DeriveBytes(new UTF8Encoding(false).GetBytes(password), salt, _numOfIerations);
			var hash = pbkdf2.GetBytes(20);

			// Combine the salt and password bytes for later use:
			var hashBytes = new byte[36];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 20);

			// Length of the string returned: ((num of hashBytes + 2) / 3) * 4
			// (Base64 returns 4 bytes output for 3 bytes input, rounded up (padded with =) to the next 4-byte boundary)
			return Convert.ToBase64String(hashBytes);
		}

		/// <summary>
		/// Verfies the password a user entered against the hashcode of 
		/// the originally stored user password.
		/// </summary>
		/// <param name="password">Password a user entered.</param>
		/// <param name="hashcode">Hashcode of the originally stored user password.</param>
		/// <param name="throwOnIllegalArgs">If True, Exceptions will be thrown</param>
		public static bool Verify(string password, string hashcode, bool throwOnIllegalArgs = false)
		{
			try
			{
				// extract the bytes
				var hashBytes = Convert.FromBase64String(hashcode);
			
				// Get the salt
				var salt = new byte[16];
				Array.Copy(hashBytes, 0, salt, 0, 16);

				// Compute the hash on the password
				var pbkdf2 = new Rfc2898DeriveBytes(new UTF8Encoding(false).GetBytes(password), salt, _numOfIerations);
				var hash = pbkdf2.GetBytes(20);
			
				// Compare the results
				for (var i=0; i < 20; i++)
					if (hashBytes[i + 16] != hash[i])
						return false;
			}
			catch (Exception)
			{
				if (throwOnIllegalArgs)
					throw;

				return false;
			}

			return true;
		}
	}
}
