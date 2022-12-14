using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Spine.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class RequiredIfAnyAttribute : ValidationAttribute
    {
        #region Properties

        /// <summary>
        /// Gets or sets the other property name that will be used during validation.
        /// </summary>
        /// <value>
        /// The other property name.
        /// </value>
        public string OtherProperty { get; private set; }

        /// <summary>
        /// Gets or sets the display name of the other property.
        /// </summary>
        /// <value>
        /// The display name of the other property.
        /// </value>
        public string OtherPropertyDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the other property values that will be relevant for validation.
        /// </summary>
        /// <value>
        /// The other property value.
        /// </value>
        public object[] OtherPropertyValues { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether other property's value should match or differ from provided other property's value (default is <c>false</c>).
        /// </summary>
        /// <value>
        ///   <c>true</c> if other property's value validation should be inverted; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// How this works
        /// - true: validated property is required when other property matches none of the provided values
        /// - false: validated property is required when other property matches any of the provided values
        /// </remarks>
        public bool IsInverted { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the attribute requires validation context.
        /// </summary>
        /// <returns><c>true</c> if the attribute requires validation context; otherwise, <c>false</c>.</returns>
        public override bool RequiresValidationContext => true;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAnyAttribute"/> class.
        /// </summary>
        /// <param name="otherProperty">The other property.</param>
        /// <param name="otherPropertyValues">The other property values.</param>
        public RequiredIfAnyAttribute(string otherProperty, params object[] otherPropertyValues)
            : base("'{0}' is required because '{1}' has a value {3}'{2}'.")
        {
            OtherProperty = otherProperty;
            OtherPropertyValues = otherPropertyValues;
            IsInverted = false;
        }

        #endregion

        /// <summary>
        /// Applies formatting to an error message, based on the data field where the error occurred.
        /// </summary>
        /// <param name="name">The name to include in the formatted message.</param>
        /// <returns>
        /// An instance of the formatted error message.
        /// </returns>
        public override string FormatErrorMessage(string name)
        {
            if (OtherPropertyValues.Length > 1)
            {
                OtherPropertyValues[OtherPropertyValues.Length - 1] = $"or {OtherPropertyValues[OtherPropertyValues.Length - 1]}";
            }

            var otherValuesString = string.Join(OtherPropertyValues.Length > 2 ? ", " : " ", OtherPropertyValues);
            return string.Format(
                CultureInfo.CurrentCulture,
                ErrorMessageString,
                name,
                OtherPropertyDisplayName ?? OtherProperty,
                otherValuesString,
                IsInverted ? "other than " : "of ");
        }

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException("validationContext");
            }

            var otherProperty = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherProperty == null)
            {
                return new ValidationResult(
                    string.Format(CultureInfo.CurrentCulture, "Could not find a property named '{0}'.", OtherProperty));
            }

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            // check if this value is actually required and validate it
            if (!IsInverted && OtherPropertyValues.Any(a => Equals(a, otherValue)) ||
                IsInverted && OtherPropertyValues.All(a => !Equals(a, otherValue)))
            {
                if (value == null)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }

                // additional check for strings so they're not empty
                if (value is string val && val.Trim().Length == 0)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }

                //Additional check for lists so that they're not empty
                switch (value)
                {
                    case Array array:
                        if (array.Length == 0)
                        {
                            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                        }

                        break;

                    case IList list:
                        if (list.Count == 0)
                        {
                            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                        }

                        break;

                    case ICollection collection:
                        if (collection.Count == 0)
                        {
                            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                        }

                        break;

                    case HashSet<Guid> set:
                        if (set.Count == 0)
                        {
                            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                        }

                        break;
                }
            }

            return ValidationResult.Success;
        }
    }
}
