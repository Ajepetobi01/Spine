using System;
using System.ComponentModel.DataAnnotations;

namespace Spine.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequiredNonDefaultAttribute : ValidationAttribute
    {
        public RequiredNonDefaultAttribute()
            : base(@"{0} is required.")
        {
        }

        public RequiredNonDefaultAttribute(string errorMessage) : base(errorMessage)
        {

        }

        public override bool IsValid(object value)
        {
            return value != null && !Equals(value, Activator.CreateInstance(value.GetType()));
        }
    }

}
