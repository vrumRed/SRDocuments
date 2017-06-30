using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models
{
    public class Message
    {
        public int MessageID { get; set; }

        [Required]
        public int ChatID { get; set; }
        public virtual Chat Chat { get; set; }

        [Required]
        public int SentByID { get; set; }
        public virtual ApplicationUser SentBy { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public string SentDate { get; set; }

        public string VisualizationDate { get; set; }

    }
}
