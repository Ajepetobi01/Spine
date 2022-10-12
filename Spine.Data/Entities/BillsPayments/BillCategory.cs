using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities.BillsPayments
{
    public class BillCategory
    {
        [Key]
        [MaxLength(5)]
        public string CategoryId { get; set; }

        [MaxLength(50)]
        public string CategoryName { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

    }
}
