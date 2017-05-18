using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(80)]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Required")]
        [StringLength(80)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "Password must be between 6 and 20 characters long", MinimumLength = 6)]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords Must Match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF must have length of 11 characters")]
        public string CPF { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}
