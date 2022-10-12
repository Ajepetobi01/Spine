using System.Collections.Generic;

namespace Spine.Common.Models
{
    public class PagedResult<TItemType>
    {
        public int ItemCount { get; set; }
        public int PageLength { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public IList<TItemType> Items { get; set; }
    }
}
