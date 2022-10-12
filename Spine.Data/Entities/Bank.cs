using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities
{
    public class Bank
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string BankName { get; set; }
        public string BankCode { get; set; }
    }
}
