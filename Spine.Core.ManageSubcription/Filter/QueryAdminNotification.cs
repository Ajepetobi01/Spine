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
    public class QueryAdminNotification : ISortedRequest
    {
        [StringRange(new[] {
                nameof(Model.ReminderDate),
                nameof(Model.ReminderTime),
                nameof(Model.IsRead)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }
    public class FilterAdminNotification
    {
        public string SortBy { get; set; }
        public string OrderBy { get; set; }
        public string SortByAndOrder { get; set; }
    }
}
