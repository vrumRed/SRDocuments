using System.ComponentModel.DataAnnotations;

namespace SRDocuments.Models.AccountViewModels
{
    public class ReSendConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
