using System.Collections.Generic;

namespace Spine.Services.EmailTemplates.Models
{
    public class LowStock : BaseClass, ITemplateModel
    {
        public string Description { get; set; }
        public List<LowStockModel> Model { get; set; }
    }

    public class LowStockModel
    {
        public string Item { get; set; }
        public string Description { get; set; }
        public string LastRestockDate { get; set; }
        public int Quantity { get; set; }
    }

}
