using System;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateDateGreaterThan:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime _birthJoin = Convert.ToDateTime(value);
                if (_birthJoin < DateTime.Now)
                {
                    return new ValidationResult("Date must be greater than current date.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
