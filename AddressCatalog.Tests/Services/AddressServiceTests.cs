using AddressCatalog.Data;
using AddressCatalog.Data.Entities;
using AddressCatalog.Models;
using AddressCatalog.Services;
using Microsoft.EntityFrameworkCore;

namespace AddressCatalog.Tests.Services;

public class AddressServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Address MakeAddress(int id, string city = "TestCity", string state = "TestState") => new()
    {
        AddressID = id,
        AddressLine1 = $"Street {id}",
        City = city,
        StateProvince = state,
        CountryRegion = "US",
        PostalCode = "12345",
        rowguid = Guid.NewGuid(),
        ModifiedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    [Fact]
    public async Task GetPagedAsync_FirstPage_Returns20Items()
    {
        using var db = CreateContext();
        db.Addresses.AddRange(Enumerable.Range(1, 25).Select(i => MakeAddress(i)));
        await db.SaveChangesAsync();
        var service = new AddressService(db);

        var (items, total) = await service.GetPagedAsync(page: 1, pageSize: 20);

        Assert.Equal(25, total);
        Assert.Equal(20, items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_ReturnsRemainder()
    {
        using var db = CreateContext();
        db.Addresses.AddRange(Enumerable.Range(1, 25).Select(i => MakeAddress(i)));
        await db.SaveChangesAsync();
        var service = new AddressService(db);

        var (items, total) = await service.GetPagedAsync(page: 2, pageSize: 20);

        Assert.Equal(25, total);
        Assert.Equal(5, items.Count());
    }

    [Fact]
    public async Task GetDistinctCitiesAsync_ReturnsUniqueSortedCities()
    {
        using var db = CreateContext();
        db.Addresses.AddRange(
            MakeAddress(1, city: "Zebra"),
            MakeAddress(2, city: "Apple"),
            MakeAddress(3, city: "Apple"));
        await db.SaveChangesAsync();
        var service = new AddressService(db);

        var cities = (await service.GetDistinctCitiesAsync()).ToList();

        Assert.Equal(2, cities.Count);
        Assert.Equal("Apple", cities[0]);
        Assert.Equal("Zebra", cities[1]);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCityStateAndModifiedDate()
    {
        using var db = CreateContext();
        db.Addresses.Add(MakeAddress(1, city: "OldCity", state: "OldState"));
        await db.SaveChangesAsync();
        var service = new AddressService(db);
        var before = DateTime.UtcNow;

        var result = await service.UpdateAsync(new AddressEditDto
        {
            AddressID = 1,
            City = "NewCity",
            StateProvince = "NewState"
        });

        Assert.True(result);
        var updated = await db.Addresses.FindAsync(1);
        Assert.Equal("NewCity", updated!.City);
        Assert.Equal("NewState", updated.StateProvince);
        Assert.True(updated.ModifiedDate >= before);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenIdNotFound()
    {
        using var db = CreateContext();
        var service = new AddressService(db);

        var result = await service.UpdateAsync(new AddressEditDto
        {
            AddressID = 999,
            City = "City",
            StateProvince = "State"
        });

        Assert.False(result);
    }
}
