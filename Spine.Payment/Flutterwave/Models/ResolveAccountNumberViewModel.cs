using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Payment.Flutterwave.Models
{
    public class ResolveAccountNumberViewModel
    {
        public string status { get; set; }
        public string message { get; set; }
        public RaveAccountResolveData data { get; set; }
    }

    public class RaveAccountResolveData
    {
        public string account_number { get; set; }
        public string account_name { get; set; }
    }
}
