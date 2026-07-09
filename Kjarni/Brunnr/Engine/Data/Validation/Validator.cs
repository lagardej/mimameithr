using Kjarni.Brunnr.Engine.Data.Normalization;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Brunnr.Engine.Data.Validation;

/// <summary>
///     Validates constraints declared via attributes on a record's properties.
///     Throws <see cref="ValidationException" /> on the first violation.
///     Run <see cref="Normalizer.Normalize{T}" /> before validating.
/// </summary>
public static class Validator
{
    /// <summary>Validates constraints declared via attributes on a record's properties.</summary>
    /// <typeparam name="T">Record type with properties to validate.</typeparam>
    /// <param name="record">Record instance to validate.</param>
    /// <exception cref="ValidationException">Thrown on the first constraint violation.</exception>
    public static void Validate<T>(T record) where T : notnull =>
        global::System.ComponentModel.DataAnnotations.Validator.ValidateObject(
            record, new ValidationContext(record), true);
}
