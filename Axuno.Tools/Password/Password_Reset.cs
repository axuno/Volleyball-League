using System.Globalization;

namespace Axuno.Tools.Password;

public class Reset
{
    // exactly 24 bytes for Encryption Key
    private const string _key = "!?aht97+<good$key$#+*&12";
    // exactly 8 bytes for Initialization Vector
    private const string _iv = "!Ö*a1°~Q";
    private readonly ExpiringAesEncryptor<ResetModel> _tripleDes = new(_key, _iv);

    private Reset()
    {
        _tripleDes.ToContainer = ToContainer;
        _tripleDes.ToText = ToText;
    }

    internal static Reset Current { get; } = new();

    private string ToText(ResetModel m)
    {
        return string.Join("\0", m.Id.ToString(CultureInfo.InvariantCulture),
            m.UsernameCrc32.ToString(CultureInfo.InvariantCulture),
            m.PasswordCrc32.ToString(CultureInfo.InvariantCulture));
    }

    private static ResetModel ToContainer(string text, ResetModel m)
    {
        var split = (text ?? string.Empty).Split(new[] { '\0' }, 3);
        m.Id = long.Parse(split[0]);
        m.UsernameCrc32 = uint.Parse(split[1]);
        m.PasswordCrc32 = uint.Parse(split[2]);
        return m;
    }

    public string GetResetKey(ResetModel model, DateTime expiresOn)
    {
        return _tripleDes.Encrypt(model, expiresOn: expiresOn);
    }

    public ExpiringAesEncryptor<ResetModel>.DecryptionResult DecryptResetKey(string encrypted, ResetModel model)
    {
        return _tripleDes.Decrypt(encrypted, model);
    }
}

public class ResetModel
{
    public long Id { get; set; }
    public uint UsernameCrc32 { get; set; }
    public uint PasswordCrc32 { get; set; }
}
