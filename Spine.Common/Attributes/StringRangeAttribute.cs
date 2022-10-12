using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Spine.Common.Extensions;

namespace Spine.Common.Attributes
{
    public class StringRangeAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _allowedValues;

        public StringRangeAttribute(string[] allowedValues)
        {
            if (allowedValues.IsNullOrEmpty())
            {
                throw new ArgumentException("Allowed values cannot be null or empty");
            }

            _allowedValues = new HashSet<string>(allowedValues, StringComparer.InvariantCultureIgnoreCase);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            switch (value)
            {
                case null:
                case string target when _allowedValues.Contains(target):
                case string[] arrayTarget when arrayTarget.All(x => _allowedValues.Contains(x)):
                    return ValidationResult.Success;
                default:
                    return new ValidationResult($"Enter one of the allowed values: {string.Join(", ", _allowedValues)}.");
            }
        }

    }

}
