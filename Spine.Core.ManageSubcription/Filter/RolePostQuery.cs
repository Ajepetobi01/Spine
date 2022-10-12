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
    public class RolePostQuery : ISortedRequest
    {
        [StringRange(new[] {
                nameof(Model.Role),
                nameof(Model.description)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }
}
