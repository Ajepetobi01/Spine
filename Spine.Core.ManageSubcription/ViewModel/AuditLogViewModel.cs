using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class AuditLogViewModel
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int EntityType { get; set; }
        public int Action { get; set; }
        public string Description { get; set; }
        public string MACAddress { get; set; }
        public string Device { get; set; }
        public string CreatedOn { get; set; }
        public DateTime GetCreatedOn { get; set; }
        public string Time { get; set; }
    }
}
