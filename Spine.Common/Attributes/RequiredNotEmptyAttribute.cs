using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Spine.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequiredNotEmptyAttribute : ValidationAttribute

    {
        public RequiredNotEmptyAttribute()
            : base(@"{0} is required.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value != null)
            {
                switch (value)
                {
                    case Array array:
                        return array.Length > 0;
                    case IList list:
                        return list.Count > 0;
                    case ICollection collection:
                        return collection.Count > 0;
                    case HashSet<Guid> set:
                        return set.Count > 0;
                    case string str:
                        return str != string.Empty;
                }
            }

            return false;
        }
    }

}
