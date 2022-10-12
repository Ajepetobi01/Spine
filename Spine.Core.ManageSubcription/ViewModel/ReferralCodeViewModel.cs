using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class ReferralCodeViewModel
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Percentage { get; set; }
        public string DateCreated { get; set; }
    }

    public class CreateReferralCodeViewModel
    {
        public bool Status { get; set; }
        public decimal Percentage { get; set; }
    }
}
