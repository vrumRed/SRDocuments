using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
       
        public string Token { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Password must be between 6 and 20 characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }
    }
}
