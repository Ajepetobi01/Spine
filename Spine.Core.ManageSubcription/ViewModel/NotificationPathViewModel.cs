using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class NotificationPathViewModel
    {
        public Guid Id { get; set; }
        public string PathDesscription { get; set; }
        public bool IsActive { get; set; }
    }
    public class CreateNotificationPathViewModel
    {
        public string PathName { get; set; }
        public bool IsActive { get; set; }
    }
}
