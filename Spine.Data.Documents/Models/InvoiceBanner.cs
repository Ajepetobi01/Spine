using System;
using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Documents.Models
{
    public class InvoiceBanner
    {
        [Key]
        public Guid Id { get; set; }
        public string Base64string { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
