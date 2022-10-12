using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("DocumentTemplate")]
    public class DocumentTemplate
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Subject { get; set; }
        [Column(TypeName = "text")]
        public string Body { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
