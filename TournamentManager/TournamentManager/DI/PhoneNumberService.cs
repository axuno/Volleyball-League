using System;
using Microsoft.Extensions.Logging;
using PhoneNumbers;

namespace TournamentManager.DI
{
    /// <summary>
    /// The service to validate and to format phone numbers.
    /// </summary>
    /// <remarks>
    /// Recommended read about common phone number falsehoods:
    /// https://github.com/google/libphonenumber/blob/master/FALSEHOODS.md
    /// </remarks>
    public class PhoneNumberService
    {
        private readonly PhoneNumbers.PhoneNumberUtil _fnu;
        private static readonly object Locker = new();
        private readonly ILogger _logger = AppLogging.CreateLogger<PhoneNumber>();

        public PhoneNumberService(PhoneNumbers.PhoneNumberUtil phoneNumberUtility)
        {
            _fnu = phoneNumberUtility;
        }

        /// <summary>
        /// Tests whether a phone number matches a valid pattern. Note this doesn't verify the number
        /// is actually in use, which is impossible to tell by just looking at a number itself.
        /// </summary>
        /// <param name="phoneNumber">The phone number in national or international format, that we want to validate.</param>
        /// <param name="defaultRegion">The region to use, if the phoneNumber is in national format.</param>
        /// <returns>A bool that indicates whether the number is of a valid pattern.</returns>
        public bool IsValid(string phoneNumber, string defaultRegion)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

            lock (Locker)
            {
                try
                {
                    var number = _fnu.Parse(phoneNumber, defaultRegion);
                    return _fnu.IsValidNumber(number);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "{@phoneNumber}", phoneNumber);
                    return false;
                }
            }
        }

        /// <summary>
        /// Formats a phone number to display formats.
        /// If it belongs to the region, the national format is returned,
        /// else the international format is returned.
        /// </summary>
        /// <param name="phoneNumber">A phone number which belongs to the region, or a phone number in international format.</param>
        /// <param name="region">The region to use for formatting.</param>
        /// <returns>Returns the formatted string, if the phoneNumber is valid, else the original phoneNumber</returns>
        public string Format(string phoneNumber, string region)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return phoneNumber;

            lock (Locker)
            {
                try
                {
                    var number = _fnu.Parse(phoneNumber, region);
                    if (!_fnu.IsValidNumber(number)) return phoneNumber;
                    return _fnu.Format(number,
                        _fnu.GetRegionCodeForNumber(number).Equals(region.ToUpperInvariant())
                            ? PhoneNumbers.PhoneNumberFormat.NATIONAL
                            : PhoneNumbers.PhoneNumberFormat.INTERNATIONAL);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "{@phoneNumber}", phoneNumber);
                    return phoneNumber;
                }
            }
        }

        /// <summary>
        /// Formats a phone number in E164 format. Used storing in the database.
        /// </summary>
        /// <param name="phoneNumber">A phone number which belongs to the region, or a phone number in international format.</param>
        /// <param name="region">The region to use for formatting.</param>
        /// <returns>Returns the formatted string, if the phoneNumber is valid, else the original phoneNumber</returns>
        public string FormatForStorage(string phoneNumber, string region)
        {
            lock (Locker)
            {
                try
                {
                    var number = _fnu.Parse(phoneNumber, region);
                    if (!_fnu.IsValidNumber(number)) return phoneNumber;
                    return _fnu.Format(number, PhoneNumberFormat.E164);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "{@phoneNumber}", phoneNumber);
                    return phoneNumber;
                }
            }
        }

        /// <summary>
        /// Formats a phone number as a telephone Uri (e.g.: tel:tel:+1-212-555-0101).
        /// </summary>
        /// <param name="phoneNumber">A phone number which belongs to the region, or a phone number in international format.</param>
        /// <param name="region">The region to use for formatting.</param>
        /// <returns>Returns the formatted string, if the phoneNumber is valid, else the original phoneNumber with "tel:" prepended.</returns>
        public string FormatAsTelephoneUri(string phoneNumber, string region)
        {
            lock (Locker)
            {
                try
                {
                    var number = _fnu.Parse(phoneNumber, region);
                    if (!_fnu.IsValidNumber(number)) return "tel:" + phoneNumber;
                    return _fnu.Format(number, PhoneNumberFormat.RFC3966);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "{@phoneNumber}", phoneNumber);
                    return "tel:" + phoneNumber;
                }
            }
        }

        /// <summary>
        /// Checks whether 2 phone numbers of the same region are valid and equal.
        /// </summary>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="defaultRegion"></param>
        /// <returns>Returns true, if the phone numbers are valid and equal, else false.</returns>
        public bool IsMatch(string number1, string number2, string defaultRegion)
        {
            if (!IsValid(number1, defaultRegion) || !IsValid(number2, defaultRegion))
            {
                return false;
            }

            lock (Locker)
            {
                return _fnu.IsNumberMatch(_fnu.Parse(number1, defaultRegion), _fnu.Parse(number2, defaultRegion)) ==
                       PhoneNumberUtil.MatchType.EXACT_MATCH;
            }
        }
    }
}