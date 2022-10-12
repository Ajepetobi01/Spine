using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Payment.Paystack.Models
{
    public class AccountNumberValidationResponseModel
    {
        public bool status { get; set; }
        public string message { get; set; }
        public AccountNumberData data { get; set; }
    }

    public class AccountNumberData
    {
        public string account_number { get; set; }
        public string account_name { get; set; }
        public int bank_id { get; set; }
    }
}
