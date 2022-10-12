using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Spine.Data.Documents.Models
{
    [Index(nameof(CompanyId))]
    public class CompanyInvoiceSignature
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Base64string { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
