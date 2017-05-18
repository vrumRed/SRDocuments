using System.ComponentModel.DataAnnotations;

namespace SRDocuments.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
