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
    public class QueryAuditLog : ISortedRequest
    {
        public string Username { get; set; }
        public string Device { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Search { get; set; }
        [StringRange(new[] {
                nameof(Model.Username),
                nameof(Model.Device),
                nameof(Model.Time)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }

    public class FilterAuditLog
    {
        public string Username { get; set; }
        public string Device { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Search { get; set; }
        public string SortBy { get; set; }
        public string OrderBy { get; set; }
        public string SortByAndOrder { get; set; }
    }
}
