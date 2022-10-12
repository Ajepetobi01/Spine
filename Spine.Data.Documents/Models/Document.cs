using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Spine.Data.Documents.Models
{
    [Index(nameof(CompanyId))]
    public class Document
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Base64string { get; set; }
        [MaxLength(256)]
        public string DocumentName { get; set; }
        [MaxLength(256)]
        public string FileType { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
