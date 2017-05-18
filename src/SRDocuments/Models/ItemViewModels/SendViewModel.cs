using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models.ItemViewModels
{
    public class SendViewModel
    {
        [Required]
        [StringLength(160)]
        public string Name { get; set; }

        [Required]
        [StringLength(999)]
        public string Description { get; set; }

        [Display(Name = " ")]
        [Required]
        public bool CheckRequiredDate { get; set; }

        [StringLength(15)]
        [DataType(DataType.Date)]
        public string RequiredDate { get; set; }

        [Required]
        public List<IFormFile> Files { get; set; }

        [Required]
        public string ReceiverID { get; set; }
    }
}
