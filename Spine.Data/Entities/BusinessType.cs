using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities
{
    public class BusinessType
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string Type { get; set; }

    }
}
