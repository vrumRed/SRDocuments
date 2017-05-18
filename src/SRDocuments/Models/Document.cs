using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [StringLength(160)]
        public string Name { get; set; }

        [Required]
        [StringLength(999)]
        public string Description { get; set; }

        [Required]
        [StringLength(15)]
        public string SentDate { get; set; }

        [StringLength(15)]
        public string RequiredDate { get; set; }

        [StringLength(15)]
        public string VisualizationDate { get; set; }

        [StringLength(15)]
        public string AnswerDate { get; set; }

        [StringLength(15)]
        public string ConclusionDate { get; set; }

        [Required]
        public bool Finished { get; set; }

        [Required]
        public bool NotAccepted { get; set; }
        
        [Required]
        public string SentById { get; set; }
        [NotMapped]
        public ApplicationUser SentBy { get; set; }
        
        [Required]
        public string SentToId { get; set; }
        [NotMapped]
        public ApplicationUser SentTo { get; set;}

        [NotMapped]
        public List<DocumentImage> DocumentImages { get; set; }

        public string SentImagesRarLocale { get; set; }

        public string ReceivedImagesRarLocale { get; set; }
    }
}
