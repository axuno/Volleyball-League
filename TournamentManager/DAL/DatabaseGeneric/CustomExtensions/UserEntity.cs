using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
#if !CF
#endif
using System.Xml.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.DAL.EntityClasses;

public partial class UserEntity
{
    [XmlIgnore]
    public string CompleteName => (string.Join(" ", new[] { Title, FirstName, MiddleName, LastName }).Replace("  ", " ").Trim());

    [XmlIgnore]
    public string CompleteNameWithNickName
    {
        get
        {
            if (!string.IsNullOrEmpty(Nickname) && Nickname != FirstName)
                return string.Join(" ", new[] { Title, FirstName, MiddleName, "(" + Nickname + ")", LastName }).Replace("  ", " ").Trim();

            return CompleteName;
        }
    }

    [XmlIgnore]
    public ReadOnlyDictionary<string, string> DataErrorInfoPerField => new(base.DataErrorInfoErrorsPerField ?? new Dictionary<string, string>());

    [XmlIgnore]
    public bool IsTeamManager => ManagerOfTeams.Any();

    [XmlIgnore]
    public bool IsPlayer => PlayerInTeams.Any();

    [XmlIgnore]
    public bool DataCompleteAsManager => (new[] {FirstName, LastName, PhoneNumber, Email}).All(s => !string.IsNullOrWhiteSpace(s));

    [XmlIgnore]
    public bool DataCompleteAsPerson => (new[] { FirstName, LastName, Email }).All(s => !string.IsNullOrWhiteSpace(s));

    protected override void OnSetValue(int fieldIndex, object valueToSet, out bool cancel)
    {
        switch (fieldIndex)
        {
            case (int) UserFieldIndex.Email:
            case (int)UserFieldIndex.Email2:
                var emailToSet = (string) valueToSet ?? string.Empty;
                cancel = (!string.IsNullOrEmpty(emailToSet) && !emailToSet.Contains('@'));
                break;
            default:
                cancel = false;
                break;
        }
    }

    protected override void OnBeforeEntitySave()
    {   
        var now = DateTime.UtcNow;
        if (IsNew && !Fields[UserFields.CreatedOn.FieldIndex].IsChanged) CreatedOn = now;
        if (IsDirty && !Fields[UserFields.ModifiedOn.FieldIndex].IsChanged) ModifiedOn = now;
        if (IsNew && !Fields[UserFields.Guid.FieldIndex].IsChanged) Guid = System.Guid.NewGuid().ToString("N"); // should be set by UserStore of Identity

        if (Gender.Length == 0 || !new[] {'m', 'f', 'u'}.Contains(Gender[0]))
        {
            Gender = "u";
        }
            
        if (!string.IsNullOrEmpty(FirstName))
        {
            FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(FirstName?.ToLower(CultureInfo.CurrentCulture));
        }

        if (!string.IsNullOrEmpty(MiddleName))
        {
            MiddleName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MiddleName.ToLower(CultureInfo.CurrentCulture));
        }

        if (!string.IsNullOrEmpty(LastName))
        {
            LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(LastName.ToLower(CultureInfo.CurrentCulture));
        }

        if (!string.IsNullOrEmpty(Nickname))
        {
            Nickname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Nickname.ToLower(CultureInfo.CurrentCulture));
        }

        if (string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrEmpty(PhoneNumber2))
        {
            PhoneNumber = PhoneNumber2;
            PhoneNumber2 = string.Empty;
        }

        if (string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Email2))
        {
            Email = Email2;
            Email2 = string.Empty;
        }

        if (!string.IsNullOrEmpty(Email))
        {
            Email = Email.ToLowerInvariant();
        }

        if (!string.IsNullOrEmpty(Email2))
        {
            Email2 = Email2.ToLowerInvariant();
        }

        base.OnBeforeEntitySave();
    }
}