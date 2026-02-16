using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Tests;

public class ApplicationDbContextModelTests
{
    [Fact]
    public void DepartmentName_HasUniqueIndex()
    {
        using var context = CreateContext();

        var departmentEntity = context.Model.FindEntityType(typeof(Department));
        Assert.NotNull(departmentEntity);

        var nameProperty = departmentEntity!.FindProperty(nameof(Department.Name));
        Assert.NotNull(nameProperty);

        var hasUniqueNameIndex = departmentEntity
            .GetIndexes()
            .Any(i => i.IsUnique && i.Properties.Count == 1 && i.Properties[0] == nameProperty);

        Assert.True(hasUniqueNameIndex);
    }

    [Fact]
    public void BillAmount_HasConfiguredPrecisionAndScale()
    {
        using var context = CreateContext();

        var billEntity = context.Model.FindEntityType(typeof(Bill));
        Assert.NotNull(billEntity);

        var amountProperty = billEntity!.FindProperty(nameof(Bill.Amount));
        Assert.NotNull(amountProperty);

        Assert.Equal(12, amountProperty!.GetPrecision());
        Assert.Equal(2, amountProperty.GetScale());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new ApplicationDbContext(options);
    }
}
