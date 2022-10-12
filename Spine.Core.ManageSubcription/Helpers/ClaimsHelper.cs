using Microsoft.AspNetCore.Identity;
using Spine.Common.Helpers;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Helpers
{
    public static class ClaimsHelper
    {
        public static void GetPermissions(this List<RoleClaimsViewModel> allPermissions, Type policy, string roleId)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo fi in fields)
            {
                //allPermissions.Add(new RoleClaimsViewModel { Value = fi.GetValue(null).ToString(), Type = Constants.PermissionClaim });
                //allPermissions.Add(new RoleClaimsViewModel { Value = fi.GetValue(null).ToString()});
                
                allPermissions.Add(new RoleClaimsViewModel { value = (int)fi.GetValue(null), text = fi.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().Single().Description });
            }
        }

        public static async Task AddPermissionClaim(this RoleManager<IdentityRole> roleManager, IdentityRole role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == Constants.PermissionClaim && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim(Constants.PermissionClaim, permission));
            }
        }
    }
}
