using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string NotificationUserId { get; set; }
        [NotMapped]
        public ApplicationUser NotificationUser { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public bool wasRead { get; set; }

        [NotMapped]
        public int Number { get; set; }
    }
}
