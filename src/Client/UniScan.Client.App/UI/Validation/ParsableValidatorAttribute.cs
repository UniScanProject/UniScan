using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

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

public interface IValidatorParser
{
    static abstract bool Validate([NotNullWhen(true)] string? s, IFormatProvider? provider);
}

public class ValidatorParserAttribute<TParser> : ValidationAttribute
    where TParser : IValidatorParser
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string v && TParser.Validate(v, null))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult(ErrorMessage);
    }
}