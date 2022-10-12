using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities.Transactions
{
    public class AccountClass
    {
        [Key]
        public int Id { get; set; }
        
        public string Class { get; set; }
        /// <summary>
        /// B is Balance Sheet, P is Profit and Loss account
        /// </summary>
        public char Type { get; set; }
        /// <summary>
        /// will be useful for year end process.....
        /// Income and Expenses accounts will be zerorised to retained earnings account
        /// </summary>
        public char AccountTreatment { get; set; }


    }
}
