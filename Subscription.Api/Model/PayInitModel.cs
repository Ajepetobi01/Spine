using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Subscription.Api.Model
{
    public class PayInitModel
    {
        public string email { get; set; }

        public int amount { get; set; }

        public string callbackUrl { get; set; }
    }
}
