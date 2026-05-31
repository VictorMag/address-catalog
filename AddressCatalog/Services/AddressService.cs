using AddressCatalog.Data;
using AddressCatalog.Data.Entities;
using AddressCatalog.Models;
using Microsoft.EntityFrameworkCore;

namespace AddressCatalog.Services;

public class AddressService : IAddressService
{
    private readonly AppDbContext _db;

    public AddressService(AppDbContext db) => _db = db;

    public async Task<(IEnumerable<Address> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.Addresses.AsNoTracking().OrderBy(a => a.AddressID);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<IEnumerable<string>> GetDistinctCitiesAsync()
        => await _db.Addresses.AsNoTracking()
            .Select(a => a.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    public async Task<IEnumerable<string>> GetDistinctStatesAsync()
        => await _db.Addresses.AsNoTracking()
            .Select(a => a.StateProvince)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

    public async Task<bool> UpdateAsync(AddressEditDto dto)
    {
        var address = await _db.Addresses.FindAsync(dto.AddressID);
        if (address is null) return false;

        address.City = dto.City;
        address.StateProvince = dto.StateProvince;
        address.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }
}
