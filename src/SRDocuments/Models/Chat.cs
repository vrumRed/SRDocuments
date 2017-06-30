using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models
{
    public class Chat
    {
        public int ChatID { get; set; }

        [Required]
        public int Person1ID { get; set; }
        public virtual ApplicationUser Person1 { get; set; }

        [Required]
        public int Person2ID { get; set; }
        public virtual ApplicationUser Person2 { get; set; }
        
        public int DocumentID { get; set; }
        public virtual Document Document { get; set; }
    }
}
