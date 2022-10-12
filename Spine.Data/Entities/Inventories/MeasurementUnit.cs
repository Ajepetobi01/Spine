using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities.Inventories
{
    public class MeasurementUnit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(10)]
        public string Unit { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }
    }
}
