using Spine.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Filter
{
    public class Model
    {
        [Sortable("FullName")]
        public string FullName { get; set; }
        [Sortable("EmailAddress")]
        public string EmailAddress { get; set; }
        [Sortable("PlanName")]
        public string PlanName { get; set; }
        [Sortable("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [Sortable("BusinessType")]
        public string BusinessType { get; set; }
        [Sortable("OpeningBalance")]
        public string OpeningBalance { get; set; }
        [Sortable("Status")]
        public string Status { get; set; }
        [Sortable("ID")]
        public Guid ID { get; set; }
        [Sortable("Role")]
        public string Role { get; set; }
        [Sortable("Description")]
        public string Description { get; set; }
        //[Sortable("GetCreatedOn", IsDefault = true)]
        //public DateTime GetCreatedOn { get; set; }
        [Sortable("Username")]
        public string Username { get; set; }
        [Sortable("Device")]
        public string Device { get; set; }
        public TimeSpan Time { get; set; }

        public DateTime? ReminderDate { get; set; }
        public TimeSpan ReminderTime { get; set; }
        public bool? IsRead { get; set; }

        [Sortable("UserName")]
        public string UserName { get; set; }
        [Sortable("CreatedOn")]
        public string CreatedOn { get; set; }
        
        [Sortable("PlanDuration")]
        public string PlanDuration { get; set; }
        [Sortable("IncludePromotion")]
        public string IncludePromotion { get; set; }

        [Sortable("DateCreated")]
        public string DateCreated { get; set; }
        [Sortable("PromotionCode")]
        public string PromotionCode { get; set; }

        


        //Role
        [Sortable("description")]
        public string description { get; set; }

        //Birthdate
        [Sortable("Name")]
        public string Name { get; set; }
        [Sortable("BeginDate")]
        public string BeginDate { get; set; }
        [Sortable("EndDate")]
        public string EndDate { get; set; }
    }
}
