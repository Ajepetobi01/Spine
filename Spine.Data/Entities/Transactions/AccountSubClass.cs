using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities.Transactions
{
    public class AccountSubClass
    {
        [Key]
        public int Id { get; set; }
        public int AccountClassId { get; set; }
        public string SubClass { get; set; }

    }
}
