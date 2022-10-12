using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class SubscriberBillingViewModel
    {
        public int ID_Billing { get; set; }
        public Guid ID_Company { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ID_Country { get; set; }
        public string ID_State { get; set; }
        public string PostalCode { get; set; }
        public string DateCreated { get; set; }
    }
    public class SubscriberBillingDTO
    {
        public int ID_Billing { get; set; }
        public Guid ID_Company { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ID_Country { get; set; }
        public string ID_State { get; set; }
        public string PostalCode { get; set; }
    }
}
