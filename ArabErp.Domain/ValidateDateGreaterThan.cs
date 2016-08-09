using System;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateDateGreaterThan:ValidationAttribute
    {
        private string DateToCompareToFieldName { get; set; }
        public ValidateDateGreaterThan(string OtherProperty)
        {
            DateToCompareToFieldName = OtherProperty;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime _field = Convert.ToDateTime(value);
                DateTime _compareTo = (DateTime)validationContext.ObjectType.GetProperty(DateToCompareToFieldName).GetValue(validationContext.ObjectInstance, null);
                if (_field < _compareTo)
                {
                    return new ValidationResult("Date must be greater than." + DateToCompareToFieldName);
                }
            }
            return ValidationResult.Success;
        }
    }
}
