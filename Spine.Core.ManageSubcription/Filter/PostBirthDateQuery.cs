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
    public class PostBirthDateQuery : ISortedRequest
    {
        public string Name { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        [StringRange(new[] {
                nameof(Model.Name),
                nameof(Model.BeginDate),
                nameof(Model.EndDate)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }
}
