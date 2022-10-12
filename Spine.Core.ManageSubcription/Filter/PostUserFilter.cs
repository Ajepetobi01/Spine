using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Filter
{
    public class PostUserFilter
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public string CreatedOn { get; set; }
        public string Status { get; set; }
        public string SortBy { get; set; }
        public string OrderBy { get; set; }
        public string SortByAndOrder { get; set; }
    }
}
