using Brunnr.Data.Validation;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using Xunit;
using Validator = Brunnr.Data.Validation.Validator;

namespace Brunnr.Tests.Data.Validation;

public sealed class ValidatorTests
{
    [Fact]
    public void Validate_ValidRecord_DoesNotThrow()
    {
        var record = new ValidRecord(Length.FromMeters(1));
        Validator.Validate(record);
    }

    [Fact]
    public void Validate_InvalidRecord_ThrowsValidationException()
    {
        var record = new InvalidRecord(Length.FromMeters(-1));
        Assert.Throws<ValidationException>(() => Validator.Validate(record));
    }

    private record ValidRecord(
        [property: RequirePositive] Length Size
    );

    private record InvalidRecord(
        [property: RequirePositive] Length Size
    );
}
