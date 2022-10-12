using System;
using System.ComponentModel.DataAnnotations;

namespace Spine.Common.Attributes
{
    public class DifferentFromAttribute : ValidationAttribute
    {
        private string DependentProperty { get; }

        public DifferentFromAttribute(string dependentProperty)
        {
            if (string.IsNullOrEmpty(dependentProperty))
            {
                throw new ArgumentNullException(nameof(dependentProperty));
            }
            DependentProperty = dependentProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var otherProperty = validationContext.ObjectInstance.GetType().GetProperty(DependentProperty);
                var otherPropertyValue = otherProperty.GetValue(validationContext.ObjectInstance, null);
                if (value.Equals(otherPropertyValue))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }

}
