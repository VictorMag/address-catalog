using AddressCatalog.Data.Entities;
using AddressCatalog.Models;
using AddressCatalog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AddressCatalog.Pages.Addresses;

public class IndexModel : PageModel
{
    private readonly IAddressService _service;
    private const int PageSize = 20;

    public IndexModel(IAddressService service) => _service = service;

    public IEnumerable<Address> Addresses { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int ItemsPerPage => PageSize;
    public SelectList Cities { get; set; } = null!;
    public SelectList States { get; set; } = null!;

    [BindProperty]
    public AddressEditDto EditModel { get; set; } = new();

    public async Task OnGetAsync(int p = 1)
    {
        CurrentPage = p;
        var (items, total) = await _service.GetPagedAsync(p, PageSize);
        Addresses = items;
        TotalCount = total;
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostEditAsync(int currentPage = 1)
    {
        if (!ModelState.IsValid)
        {
            CurrentPage = currentPage;
            var (items, total) = await _service.GetPagedAsync(currentPage, PageSize);
            Addresses = items;
            TotalCount = total;
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            await LoadDropdownsAsync();
            return Page();
        }

        var updated = await _service.UpdateAsync(EditModel);
        if (!updated) return NotFound();

        return RedirectToPage(new { p = currentPage });
    }

    private async Task LoadDropdownsAsync()
    {
        var cities = await _service.GetDistinctCitiesAsync();
        var states = await _service.GetDistinctStatesAsync();
        Cities = new SelectList(cities);
        States = new SelectList(states);
    }
}
