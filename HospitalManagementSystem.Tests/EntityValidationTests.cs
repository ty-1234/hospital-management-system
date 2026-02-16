using HospitalManagementSystem.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Tests;

public class EntityValidationTests
{
    [Fact]
    public void Department_RequiresName()
    {
        var model = new Department { Name = string.Empty };

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Department.Name)));
    }

    [Fact]
    public void Bill_AmountMustBePositive()
    {
        var model = new Bill
        {
            PatientId = 1,
            Amount = 0m,
            IssuedOn = DateTime.UtcNow.Date,
            PaymentDueDate = DateTime.UtcNow.Date.AddDays(1)
        };

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Bill.Amount)));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}
