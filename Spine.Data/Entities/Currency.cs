using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(3)]
        public string Code { get; set; }

        [MaxLength(3)]
        public string Symbol { get; set; }

    }
}
