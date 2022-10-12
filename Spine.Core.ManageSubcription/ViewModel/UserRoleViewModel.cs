using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class UserRoleViewModel
    {
        public UserRoleViewModel()
        {
            this.permission = new List<RoleClaimsViewModel>();
        }
        public Guid ID { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public bool IsSubscriberRole { get; set; }
        public IList<RoleClaimsViewModel> permission { get; set; }
    }

    public class ListRoleViewModel
    {
        public Guid ID { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public bool Expose { get; set; }
        public bool IsSubscriberRole { get; set; }
    }

    public class RoleViewModel
    {
        public string Role { get; set; }
        public bool IsSubscriberRole { get; set; }
        public string Description { get; set; }
        public IList<CreateRoleClaimsViewModel> RoleClaims { get; set; }
    }

    public class CreateRoleClaimsViewModel
    {
        public int value { get; set; }
        public bool selected { get; set; }
    }

    public class RoleClaimsViewModel
    {
        //public string Type { get; set; }
        public string text { get; set; }
        public int value { get; set; }
        public bool selected { get; set; }
    }

    public class PermissionViewModel
    {
        public Guid RoleId { get; set; }
        public IList<RoleClaimsViewModel> RoleClaims { get; set; }
    }

    public class GetDropDowmRole
    {
        public Guid roleId { get; set; }
        public string roleName { get; set; }
    }

    public class GetRoleViewModel
    {
        public Guid roleId { get; set; }
        public string roleName { get; set; }
        public string normalizedName { get; set; }
        public string concurrencyStamp { get; set; }
        public bool isDeleted { get; set; }
        public DateTime? _modifiedOn { get; set; }
        public Guid? createdBy { get; set; }
        public DateTime _createdOn { get; set; }
        public string description { get; set; }
        public bool isOwnerRole { get; set; }
        public bool isSystemDefined { get; set; }
        public string modifiedOn
        {
            get
            {
                return _modifiedOn.HasValue
? _modifiedOn.Value.ToString("dd/MM/yyyy")
: "";
            }
        }
        public string createdOn { get { return this._createdOn.ToString("dd/MM/yyyy"); } }
    }

}
