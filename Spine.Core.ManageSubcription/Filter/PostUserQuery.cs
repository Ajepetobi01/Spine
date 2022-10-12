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
    public class PostUserQuery : ISortedRequest
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public string CreatedOn { get; set; }
        public string Status { get; set; }
        [StringRange(new[] {
                nameof(Model.Username),
                nameof(Model.Role),
                nameof(Model.Status),
                nameof(Model.CreatedOn)
            })]
        public string SortBy { get; set; }

        [StringRange(new[] { "asc", "ascending", "desc", "descending" })]
        public string Order { get; set; } = "asc";

        [JsonIgnore]
        public string SortByAndOrder => this.FindSortingAndOrder<Model>();
    }
}
