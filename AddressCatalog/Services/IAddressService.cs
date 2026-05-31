using AddressCatalog.Data.Entities;
using AddressCatalog.Models;

namespace AddressCatalog.Services;

public interface IAddressService
{
    Task<(IEnumerable<Address> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<string>> GetDistinctCitiesAsync();
    Task<IEnumerable<string>> GetDistinctStatesAsync();
    Task<bool> UpdateAsync(AddressEditDto dto);
}
