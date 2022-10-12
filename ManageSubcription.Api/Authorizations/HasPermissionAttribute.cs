using Microsoft.AspNetCore.Authorization;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using System;

namespace ManageSubcription.Api.Authorizations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Permissions permission) : base(permission.GetStringValue())
        {
        }
    }
}
