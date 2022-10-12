using Spine.Common.Attributes;
using Spine.Common.Data.Interfaces;
using Spine.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Filter
{
    public class PromotionalCodeQuery : ISortedRequest
    {
        public DateTime? CreatedOn { get; set; }
        public string Search { get; set; }
        [StringRange(new[] {
                nameof(Model.PromotionCode),
                nameof(Model.DateCreated),
                nameof(Model.UserName)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }
    public class FilterPromotionalCode
    {
        public DateTime? CreatedOn { get; set; }
        public string Search { get; set; }
        public string SortBy { get; set; }
        public string OrderBy { get; set; }
        public string SortByAndOrder { get; set; }
    }
}
