using System;
using System.ComponentModel.DataAnnotations;

namespace UniScan.Client.App.UI.Validation;

public class ParsableValidatorAttribute<T> : ValidationAttribute
where T : IParsable<T>
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string v && T.TryParse(v, null, out _))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult(ErrorMessage);
    }
}