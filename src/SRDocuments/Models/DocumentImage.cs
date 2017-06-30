using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SRDocuments.Models
{
    public class DocumentImage
    {
        public int DocumentImageID { get; set; }

        [Required]
        [MaxLength(160)]
        public string Name { get; set; }

        [Required]
        public string Locale { get; set; }

        [Required]
        public int DocumentID { get; set; }
        public virtual Document Document { get; set; }

        [Required]
        public bool Original { get; set; }

    }
}
