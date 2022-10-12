using System;
using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities
{
    public class InvoiceColorTheme
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Theme { get; set; }
        [MaxLength(256)]
        public string TextColor { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
