using Microsoft.AspNetCore.Mvc;

namespace League.Models.RoleViewModels
{
    public class RoleRemoveModel
    {
        [HiddenInput]
        public string UserName { get; set; }
        [HiddenInput]
        public string ClaimType { get; set; }
        [HiddenInput]
        public long UserId { get; set; }
        [HiddenInput]
        public long TeamId { get; set; }
        [HiddenInput]
        public string ReturnUrl { get; set; }
    }
}
