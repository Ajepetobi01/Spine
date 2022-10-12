using System;
using Microsoft.AspNetCore.Authorization;
using Spine.Common.Enums;
using Spine.Common.Extensions;

namespace Transactions.Api.Authorizations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Permissions permission) : base(permission.GetStringValue())
        {
        }
    }
}
