using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Axuno.Tools.Password;

/// <summary>
/// Derived from http://www.codeproject.com/Articles/59186/Password-Strength-Control
/// By Peter Tewkesbury, 22 Feb 2010 
/// Heavily modified by NB
/// </summary>
/// <remarks>
/// Additions
/// 
/// In the additions section of the code, we add to the overall score for things which make the password 'good'. In my code, we check the following:
/// 
/// Score += (Password Length *4)
/// Score += ((Password Length - Number of Upper Case Letters)*2)
/// Score += ((Password Length - Number of Lower Case Letters)*2)
/// Score += (Number of Digits * 4)
/// Score += (Number of Symbols * 6)
/// Score += (Number of Digits or Symbols in the Middle of the Password) * 2
/// If (Number of Requirements Met > 3) then Score += (Number of Requirements Met * 2)
/// 
/// Requirements are:
/// 
/// Password Length >= 8
/// Contains Uppercase Letters (A-Z)
/// Contains Lowercase Letters (a-z)
/// Contains Digits (0-9)
/// Contains Symbols (Char.IsSymbol(ch) or Char.IsPunctuation(ch))
/// 
/// Deductions
/// 
/// In the deductions section of the code, we subtract from the overall score for things which make the password 'weak'. 
/// In my code, we check the following:
/// 
/// IF Password is all letters THEN Score -= (Password length)
/// IF Password is all digits THEN Score -= (Password length)
/// IF Password has repeated characters THEN Score -= (Number of repeated characters * (Number of repeated characters -1)
/// IF Password has consecutive uppercase letters THEN Score -= (Number of consecutive uppercase characters * 2)
/// IF Password has consecutive lowercase letters THEN Score -= (Number of consecutive lowercase characters * 2)
/// IF Password has consecutive digits THEN Score -= (Number of consecutive digits * 2)
/// IF Password has sequential letters THEN Score -= (Number of sequential letters * 3) E.g.: ABCD or DCBA.
/// IF Password has sequential digits THEN Score -= (Number of sequential digits * 3) E.g.: 1234 or 4321.
/// </remarks>
public class Checker
{
    private Checker()
    { }

    internal static Checker Current { get; } = new();


    /// <summary>
    /// Checks the password and determines the score.
    /// </summary>
    /// <param name="req">Requirements for the password.</param>
    /// <param name="pwd">Password to check.</param>
    public static async Task<PasswordStrength> CheckAsync(Requirements? req, string pwd)
    {
        req ??= new Requirements();
        return await Task.Run(() => DoCheck(req, pwd));
    }

    /// <summary>
    /// Checks the password and determines the score.
    /// </summary>
    /// <param name="req">Requirements for the password.</param>
    /// <param name="pwd">Password to check.</param>
    public static PasswordStrength Check(Requirements? req, string pwd)
    {
        req ??= new Requirements();
        return DoCheck(req, pwd);
    }

    private static PasswordStrength DoCheck(Requirements req, string pwd)
    {
        const int numOfSequChars = 3;
        var passwordStrength = new PasswordStrength();

        var forbiddenWordsShare = 0;
        const string alphaLower = "abcdefghijklmnopqrstuvwxyz";
        const string numeric = "01234567890";
        const string keyboard = "^1234567890ß´^!\"§$%&/()=?`qwertzuiopü+QWERTZUIOPÜ*asdfghjklöä#ASDFGHJKLÖÄ'<yxcvbnm,.->YXCVBNM;:_";
        var sequAlphaCount = 0;
        var sequDigitCount = 0;
        var sequKeyboardCount = 0;

        // count digits and special characters which are embedded between characters
        var embeddedCount = (new Regex("[a-zA-Z][^a-zA-Z]", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var digitCount = (new Regex("[0-9]", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var lowerCount = (new Regex("[a-z]", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var upperCount = (new Regex("[A-Z]", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var whiteSpaceCount = (new Regex(@"\s", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var specialCount = pwd.Length - digitCount - lowerCount - upperCount;

        var consecutiveUpperCount = (new Regex("[A-Z]{2,}", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var consecutiveLowerCount = (new Regex("[a-z]{2,}", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
        var consecutiveDigitCount = (new Regex("[0-9]{2,}", RegexOptions.CultureInvariant | RegexOptions.Compiled)).Matches(pwd).Count;
			
        // Check for sequential alpha string patterns (like "abcd", forward and reverse) 
        for (var s = 0; s < alphaLower.Length - numOfSequChars; s++)
        {
            var fwd = alphaLower.Substring(s, numOfSequChars);
            var rev = Reverse(fwd);
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(fwd, StringComparison.InvariantCulture) != -1) 
            {
                passwordStrength.SequCharsFound.Add(fwd);
                sequAlphaCount++;
            }
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(rev, StringComparison.InvariantCulture) != -1)
            {
                passwordStrength.SequCharsFound.Add(rev);
                sequAlphaCount++;
            }
        }

        // Check for sequential numeric string patterns (like "1234" forward and reverse)
        for (var s = 0; s < numeric.Length - numOfSequChars; s++)
        {
            var fwd = numeric.Substring(s, numOfSequChars);
            var rev = Reverse(fwd);
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(fwd, StringComparison.InvariantCulture) != -1)
            {
                passwordStrength.SequCharsFound.Add(fwd);
                sequDigitCount++;
            }
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(rev, StringComparison.InvariantCulture) != -1)
            {
                passwordStrength.SequCharsFound.Add(rev);
                sequDigitCount++;
            }
        }

        // Check for sequential German keyboard string patterns (like "asdfghjkl" forward and reverse)
        for (var s = 0; s < keyboard.Length - numOfSequChars; s++)
        {
            var fwd = keyboard.Substring(s, numOfSequChars);
            var rev = Reverse(fwd);
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(fwd, StringComparison.InvariantCulture) != -1)
            {
                passwordStrength.SequCharsFound.Add(fwd);
                sequKeyboardCount++;
            }
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(rev, StringComparison.InvariantCulture) != -1)
            {
                passwordStrength.SequCharsFound.Add(rev);
                sequKeyboardCount++;
            }
        }

        // Check for usage of forbidden words (forward and reverse)
        foreach (var word in req.ForbiddenWords)
        {
            var fwd = word.ToLower(CultureInfo.InvariantCulture);
            var rev = Reverse(word).ToLower(CultureInfo.InvariantCulture);
            if (pwd.ToLower(CultureInfo.InvariantCulture).IndexOf(fwd, StringComparison.InvariantCulture) != -1)
            {
                forbiddenWordsShare += (int) 100f/pwd.Length * word.Length;
                passwordStrength.MissedRequirements.ForbiddenWords.Add(fwd);
            }
            if (pwd.ToLower().IndexOf(rev, StringComparison.InvariantCulture) != -1)
            {
                forbiddenWordsShare += (int) 100f / pwd.Length * word.Length;
                passwordStrength.MissedRequirements.ForbiddenWords.Add(rev);
            }
        }

			
        // Score += 4 * Password Length
        AddAnalysisResult(passwordStrength, "Password Length", "(n*4)", pwd.Length, pwd.Length * 4, 0);

        // if we have uppercase letters Score += (number of uppercase letters *2)
        if (upperCount > 0)
        {
            AddAnalysisResult(passwordStrength, "Uppercase Letters", "+((len-n)*2)", upperCount, ((pwd.Length - upperCount) * 2), 0);
        }
        else
            AddAnalysisResult(passwordStrength, "Uppercase Letters", "+((len-n)*2)", upperCount, 0, 0);

        // if we have lowercase letters Score += (number of lowercase letters *2)
        if (lowerCount > 0)
        {
            AddAnalysisResult(passwordStrength, "Lowercase Letters", "+((len-n)*2)", lowerCount, ((pwd.Length - lowerCount)*2), 0);
        }
        else
        {
            AddAnalysisResult(passwordStrength, "Lowercase Letters", "+((len-n)*2)", lowerCount, 0, 0);
        }


        // Score += (Number of digits * 4)
        AddAnalysisResult(passwordStrength, "Numbers", "+(n*4)", digitCount, (digitCount * 4), 0);

        // Score += (Number of Symbols * 6)
        AddAnalysisResult(passwordStrength, "Symbols", "+(n*6)", specialCount, (specialCount * 6), 0);

        // Score += (Number of embedded digits or symbols in middle of password *2)
        AddAnalysisResult(passwordStrength, "Embedded Numbers or Symbols", "+(n*2)", embeddedCount, (embeddedCount * 2), 0);

        var requirements = (req[RequiredCriteria.MinLength] >= 0 ? 1 : 0) +
                           (req[RequiredCriteria.Uppercase] > 0 ? 1 : 0) +
                           (req[RequiredCriteria.Lowercase] > 0 ? 1 : 0) +
                           (req[RequiredCriteria.Digits] > 0 ? 1 : 0) +
                           (req[RequiredCriteria.Special] > 0 ? 1 : 0);
			
        if (pwd.Length >= req[RequiredCriteria.MinLength])
        {
            passwordStrength.Score += pwd.Length;
            AddAnalysisResult(passwordStrength, "Requirement. Length > " + req[RequiredCriteria.MinLength], "+(n)", pwd.Length, pwd.Length, 0);
        }
        else
        {
            passwordStrength.MissedRequirements.Add(RequiredCriteria.MinLength, pwd.Length);
        }

        if (req[RequiredCriteria.Uppercase] > 0)
        {
            if (upperCount >= req[RequiredCriteria.Uppercase])
            {
                AddAnalysisResult(passwordStrength, "Requirement. Uppercase", "+(n)", upperCount, upperCount, 0);
            }
            else
            {
                passwordStrength.MissedRequirements.Add(RequiredCriteria.Uppercase, upperCount);
            }
        }

        if (req[RequiredCriteria.Lowercase] > 0)
        {
            if (lowerCount >= req[RequiredCriteria.Lowercase])
            {
                AddAnalysisResult(passwordStrength, "Requirement. Lowercase", "+(n)", lowerCount, lowerCount, 0);
            }
            else
            {
                passwordStrength.MissedRequirements.Add(RequiredCriteria.Lowercase, lowerCount);
            }
        }

        if (req[RequiredCriteria.Digits] > 0)
        {
            if (digitCount >= req[RequiredCriteria.Digits])
            {
                AddAnalysisResult(passwordStrength, "Requirement. Digit", "+(n)", digitCount, digitCount, 0);
            }
            else
            {
                passwordStrength.MissedRequirements.Add(RequiredCriteria.Digits, digitCount);
            }
        }

        if (req[RequiredCriteria.Special] > 0)
        {
            if (specialCount >= req[RequiredCriteria.Special])
            {
                AddAnalysisResult(passwordStrength, "Requirement. Special", "+(2n)", specialCount, specialCount*2, 0);
            }
            else
            {
                passwordStrength.MissedRequirements.Add(RequiredCriteria.Special, specialCount);
            }
        }

        // special treatment, because this is not a quality requirement
        if (req.NoWhitespaceAllowed && whiteSpaceCount > 0)
        {
            AddAnalysisResult(passwordStrength, "Requirement. No Control or Whitespace", string.Empty, whiteSpaceCount, 0, 0);
            passwordStrength.MissedRequirements.NoWhitespaceAllowed = false;
        }

        // If we have more than 3 requirments then
        if (requirements > 3)
        {
            AddAnalysisResult(passwordStrength, "3 or more requirements", "+(n*2)", requirements, (requirements * 2), 0);
        }
        else
            AddAnalysisResult(passwordStrength, "Less than 3 requirements", "-(5-n)*2", requirements, 0, (5 - requirements) * 2);

        //
        // Deductions
        //

        // If only letters then score -=  password length
        if (digitCount == 0 && specialCount == 0)
        {
            AddAnalysisResult(passwordStrength, "Letters only", "-n", pwd.Length, 0, pwd.Length);
        }
        else
            AddAnalysisResult(passwordStrength, "Letters only", "-n", 0, 0, 0);

        // Check for number of distinct characters used in the password
        var distinctChars = pwd.ToCharArray().Distinct().Count();
        AddAnalysisResult(passwordStrength, "Distinct Characters", "-(% of not distinct chars)", distinctChars, 0, (100 - (int)(100f / pwd.Length * distinctChars)) / 4);

        // If only digits then score -=  password length
        if (digitCount == pwd.Length)
        {
            AddAnalysisResult(passwordStrength, "Numbers only", "-n", pwd.Length, 0, pwd.Length);
        }
        else
            AddAnalysisResult(passwordStrength, "Numbers only", "-n", 0, 0, 0);

        // If Consecutive uppercase letters then score -= (consecutiveUpperCount * 2);
        AddAnalysisResult(passwordStrength, "Consecutive Uppercase Letters", "-(n*2)", consecutiveUpperCount, 0, consecutiveUpperCount * 2);

        // If Consecutive lowercase letters then score -= (consecutiveLowerCount * 2);
        AddAnalysisResult(passwordStrength, "Consecutive Lowercase Letters", "-(n*2)", consecutiveLowerCount, 0, consecutiveLowerCount * 2);

        // If Consecutive digits used then score -= (consecutiveDigitCount * 2);
        AddAnalysisResult(passwordStrength, "Consecutive Numbers", "-(n*2)", consecutiveDigitCount, 0, consecutiveDigitCount * 2);

        // If password contains sequence of letters then score -= (100 / pwd.Length * sequAlphaCount)
        AddAnalysisResult(passwordStrength, "Sequential Letters (3+)", "-(% of pw length)", sequAlphaCount, 0, (int) 100f / pwd.Length * sequAlphaCount);

        // If password contains sequence of digits then score -= (100 / pwd.Length * sequDigitCount)
        AddAnalysisResult(passwordStrength, "Sequential Numbers (3+)", "-(% of pw length)", sequDigitCount, 0, (int) 100f / pwd.Length * sequDigitCount);

        // If password contains sequence of keyboard keys then score -= (100 / pwd.Length * sequDigitCount)
        AddAnalysisResult(passwordStrength, "Sequential Keyboard Keys (3+)", "-(% of pw length)", sequDigitCount, 0, (int)100f / pwd.Length * sequKeyboardCount);

        // If password contains sequence of digits then score -= forbiddenWordsShare
        AddAnalysisResult(passwordStrength, "Forbidden Words", "-(% of words of pw length)", forbiddenWordsShare, 0, forbiddenWordsShare);


        passwordStrength.Score = passwordStrength.Details.Sum(d => d.Bonus) - passwordStrength.Details.Sum(d => d.Malus);
			
        if (passwordStrength.Score > 100) { passwordStrength.Score = 100; } else if (passwordStrength.Score < 0) { passwordStrength.Score = 0; }
        if (passwordStrength.Score < 20) { passwordStrength.Rate = StrengthRate.Unsafe; }
        else if (passwordStrength.Score >= 20 && passwordStrength.Score < 40) { passwordStrength.Rate = StrengthRate.Weak; }
        else if (passwordStrength.Score >= 40 && passwordStrength.Score < 60) { passwordStrength.Rate = StrengthRate.Fair; }
        else if (passwordStrength.Score >= 60 && passwordStrength.Score < 80) { passwordStrength.Rate = StrengthRate.Strong; }
        else if (passwordStrength.Score >= 80) { passwordStrength.Rate = StrengthRate.Secure; }

        return passwordStrength;
    }

    /// <summary>
    /// Reverse a string.
    /// </summary>
    /// <param name="input">The string to reverse</param>
    /// <returns>A string</returns>
    private static string Reverse(string input)
    {
        if (input.Length <= 1)
            return input;

        var c = input.ToCharArray();
        var sb = new StringBuilder(c.Length);
        for (var i = c.Length - 1; i > -1; i--)
            sb.Append(c[i]);

        return sb.ToString();
    }

    /// <summary>
    /// Log results of analysis
    /// </summary>
    /// <param name="strength"></param>
    /// <param name="description"></param>
    /// <param name="rate"></param>
    /// <param name="count"></param>
    /// <param name="bonus"></param>
    /// <param name="malus"></param>
    /// <returns></returns>
    private static void AddAnalysisResult(PasswordStrength strength, string description, string rate, int count, int bonus, int malus)
    {
        var dr = new StrengthDetail
        {
            Description = description ?? "",
            Rate = rate ?? "",
            Count = count,
            Bonus = bonus,
            Malus = malus
        };
        strength.Details.Add(dr);
    }
}


public enum RequiredCriteria
{
    MinLength, Uppercase, Lowercase, Digits, Special
}


public class Requirements : Dictionary<RequiredCriteria, int>
{
    public Requirements()
    {
        Add(RequiredCriteria.MinLength, 5);
        Add(RequiredCriteria.Uppercase, 0);
        Add(RequiredCriteria.Lowercase, 0);
        Add(RequiredCriteria.Digits, 0);
        Add(RequiredCriteria.Special, 0);

        NoWhitespaceAllowed = true;
        ForbiddenWords = new List<string>();
    }

    /// <summary>
    /// Specify whether control characters or whitespace are allowed
    /// </summary>
    public bool NoWhitespaceAllowed { get; set; }

    /// <summary>
    /// Words or parts of words which are forbidden as password
    /// </summary>
    public List<string> ForbiddenWords { get; set; }
}

public class MissedRequirements : Requirements
{
    public MissedRequirements()
    {
        Clear();
    }
}


public class PasswordStrength
{
    public PasswordStrength()
    {
        Details = new ObservableCollection<StrengthDetail>();
        Rate = StrengthRate.Unknown;
        SequCharsFound = new List<string>();
        MissedRequirements = new MissedRequirements();
    }

    public ObservableCollection<StrengthDetail> Details { get; internal set; }

    public int Score { get; internal set; }
    public StrengthRate Rate { get; internal set; }
    public List<string> SequCharsFound { get; private set; }
    public MissedRequirements MissedRequirements { get; private set; }

    public bool RequirementsMet =>
        MissedRequirements.Count == 0 && MissedRequirements.ForbiddenWords.Count == 0 &&
        MissedRequirements.NoWhitespaceAllowed;

    public DataTable GetDetailsAsDatatable()
    {
        var dt = new DataTable("Details");
        dt.Columns.Add("Description", typeof(string));
        dt.Columns.Add("Rate", typeof(string));
        dt.Columns.Add("Count", typeof(int));
        dt.Columns.Add("Bonus", typeof(int));
        dt.Columns.Add("Malus", typeof(int));

        foreach (var d in Details)
        {
            dt.Rows.Add(d.Description, d.Rate, d.Count, d.Bonus, d.Malus);
        }

        return dt;
    }
}


/// <summary>
/// Analysis result for one rule
/// </summary>
public class StrengthDetail
{
    public string Description { get; set; } = string.Empty;
    public string Rate { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Bonus { get; set; }
    public int Malus { get; set; }
}

public enum StrengthRate
{
    Unknown, Unsafe, Weak, Fair, Strong, Secure
}
