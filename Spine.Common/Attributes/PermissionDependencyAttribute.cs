using System;
using System.Collections.Generic;
using System.Linq;
using Spine.Common.Enums;

namespace Spine.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PermissionDependencyAttribute : Attribute
    {
        public Permissions Permission;
        public List<Permissions> Dependencies;

        public PermissionDependencyAttribute(params Permissions[] permissions)
        {
            Dependencies = permissions.ToList();
            Permission = Dependencies.First();
            Dependencies.RemoveAt(0); // remove main permission from dependencies list
        }
    }
}
