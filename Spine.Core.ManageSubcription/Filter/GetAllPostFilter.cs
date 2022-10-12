using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spine.Common.Data.Interfaces;
using Spine.Common.Extensions;

namespace Spine.Core.ManageSubcription.Filter
{
    public class GetAllPostFilter
    {
        public string Name { get; set; }
        public string PlanName { get; set; }
        public string Email { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public Guid CompanyId { get; set; }
        public int daysTwoExpired { get; set; }
        public string referralcode { get; set; }
        public string Ref_ReferralCode { get; set; }
        public string Search { get; set; }
        public string SortBy { get; set; }
        public string OrderBy { get; set; }
        public string SortByAndOrder { get; set; }
    }
}
