using MailMergeLib;
using TournamentManager.MultiTenancy;

namespace League.Emailing;
internal static class MailMergeAddressCollectionExtensions
{
    public static void Add(this MailMergeAddressCollection addresses, MailKind mailKind, ITenantContext tenantContext)
    {
        if (mailKind == MailKind.None) return;

        var siteContext = tenantContext.SiteContext;

        // Map MailKind to MailMergeAddressType
        var addressType = mailKind switch
        {
            MailKind.GeneralFrom or MailKind.ContactFrom => MailAddressType.From,
            MailKind.GeneralTo or MailKind.ContactTo => MailAddressType.To,
            MailKind.GeneralBcc => MailAddressType.Bcc,
            _ => MailAddressType.To // Default fallback
        };

        if (addressType == MailAddressType.From)
        {
            var mailAddress = siteContext.MailAddresses.FirstOrDefault(a => a.Kind == mailKind);
            if (mailAddress == null) return;

            addresses.Add(new MailMergeAddress(
                addressType,
                displayName: mailAddress.DisplayName,
                address: mailAddress.Address
            ));
            return;
        }

        var mailAddresses = siteContext.MailAddresses
            .Where(a => a.Kind == mailKind);

        foreach (var mailAddress in mailAddresses)
        {
            addresses.Add(new MailMergeAddress(
                addressType,
                displayName: mailAddress.DisplayName,
                address: mailAddress.Address
            ));
        }
    }
}
