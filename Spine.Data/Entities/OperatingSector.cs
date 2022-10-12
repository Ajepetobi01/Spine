using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities
{
    public class OperatingSector
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string Sector { get; set; }

    }
}
