using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities.Invoices
{
    public class InvoiceType
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string Type { get; set; }

    }
}
