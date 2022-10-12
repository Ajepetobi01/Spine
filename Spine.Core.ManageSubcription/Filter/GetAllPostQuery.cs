using Microsoft.AspNetCore.Mvc;
using Spine.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spine.Common.Extensions;
using Spine.Common.Data.Interfaces;

namespace Spine.Core.ManageSubcription.Filter
{
    public class GetAllPostQuery : ISortedRequest
    {
        public string Name { get; set; }
        public string PlanName { get; set; }
        [FromQuery(Name = "email")]
        public string Email { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [FromQuery(Name = "get active or in-active user")]
        public string Status { get; set; }
        [FromQuery(Name = "companyId")]
        public Guid CompanyId { get; set; }
        [FromQuery(Name = "from number of days to expired")]
        public int daysTwoExpired { get; set; }
        [FromQuery(Name = "get company by referral code")]
        public string referralcode { get; set; }
        [FromQuery(Name = "get referrals by referralcode")]
        public string Ref_ReferralCode { get; set; }
        public string Search { get; set; }
        [StringRange(new[] {
                nameof(Model.FullName),
                nameof(Model.EmailAddress),
                nameof(Model.PlanName),
                nameof(Model.PhoneNumber),
                nameof(Model.BusinessType),
                nameof(Model.OpeningBalance),
                nameof(Model.Status)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }

}
