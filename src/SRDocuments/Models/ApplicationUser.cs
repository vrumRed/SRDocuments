using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(80)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(80)]
        public string LastName { get; set; }

        [StringLength(1200)]
        public string Info { get; set; }

        [StringLength(160)]
        public string FullName { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [Key]
        public string CPF { get; set; }

        [Required]
        public bool IsBlocked { get; set; }
        
        public string BlockToken { get; set; }

        public string UnblockToken { get; set; }
    }
}
