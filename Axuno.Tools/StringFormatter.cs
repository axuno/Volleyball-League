namespace Axuno.Tools;

public class StringFormatter
{
    /// <summary>
    /// Converts a list string with valid characters into its group representation.
    /// First alphanumeric groups like "a-h", then single non-alphanumeric characters.
    /// </summary>
    /// <example>
    /// "abcdefgh987654231#" will convert to "1-9 a-h #"};
    /// </example>
    /// <param name="allValidCharacters">Valid characters, will be sorted before grouping.</param>
    /// <returns>The grouped representation of the valid characters.</returns>
    public static string GroupedValidCharacters(string allValidCharacters)
    {
        return
            string.Join(" ", GroupedListOfValidCharacters(allValidCharacters).Where(g => g.Length > 1)) + " " +
            string.Join(" ", GroupedListOfValidCharacters(allValidCharacters).Where(g => g.Length == 1));
    }

    /// <summary>
    /// Converts a list string with valid characters into a list of strings.
    /// </summary>
    /// <example>
    /// "abcdefgh987654231#" will convert to new List&lt;string&gt; {"a-h", "1-9", "#"};
    /// </example>
    /// <param name="allValidCharacters">Valid characters, will be sorted before grouping.</param>
    /// <returns>The grouped representation of the valid characters.</returns>
    public static List<string> GroupedListOfValidCharacters(string allValidCharacters)
    {
        var chars = allValidCharacters.ToList();
        chars.Sort();

        var length = chars.Count;
        var groups = new List<string>();

        for (var i = 0; i < length; i++)
        {
            var firstChar = chars[i];

            // Determine last character in the sorted sequence
            int k;
            for (k = i + 1; k < length; k++)
            {
                var lastChar = chars[k - 1];
                // current char is the next in the sequence, both must be alphanumeric
                if (chars[k] == (char)(Convert.ToUInt16(lastChar) + 1) && char.IsLetterOrDigit(firstChar) && char.IsLetterOrDigit(lastChar)) continue;

                groups.Add(firstChar == lastChar
                    ? firstChar.ToString() // not alphanumeric
                    : string.Format($"{firstChar}-{lastChar}")); // alphanumeric sequence

                break;
            }

            // Add last character in input string to the final group
            if (k == length)
            {
                var lastChar = chars[k - 1];
                groups.Add(firstChar == lastChar
                    ? firstChar.ToString() // not alphanumeric
                    : string.Format($"{firstChar}-{lastChar}")); // alphanumeric sequence
            }

            i = k - 1;
        }
        return groups;
    }
}