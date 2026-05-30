# Address Catalog Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a paginated address catalog in ASP.NET Core Razor Pages con modal de edición para `City` y `StateProvince`, conectado a Azure SQL `SalesLT.Address`.

**Architecture:** Razor Pages → IndexModel (PageModel) → IAddressService → AppDbContext (EF Core) → Azure SQL. La única página (Index) maneja tanto el GET (lista) como el POST (edición via modal). Patrón PRG (Post/Redirect/Get) para evitar reenvío del formulario al recargar.

**Tech Stack:** .NET 8, ASP.NET Core Razor Pages, Entity Framework Core 8, Microsoft.EntityFrameworkCore.SqlServer, Bootstrap 5 (incluido en la plantilla), xUnit + EF InMemory (tests)

---

## Mapa de archivos

| Archivo | Responsabilidad |
|---------|----------------|
| `AddressCatalog/Data/Entities/Address.cs` | Entidad EF Core — mapea `SalesLT.Address` |
| `AddressCatalog/Data/AppDbContext.cs` | DbContext — punto de acceso a la BD |
| `AddressCatalog/Models/AddressEditDto.cs` | DTO para el formulario del modal, con validaciones |
| `AddressCatalog/Services/IAddressService.cs` | Contrato del servicio |
| `AddressCatalog/Services/AddressService.cs` | Implementación: paginación, dropdowns, update |
| `AddressCatalog/Pages/Addresses/Index.cshtml.cs` | PageModel: carga la lista y maneja el POST de edición |
| `AddressCatalog/Pages/Addresses/Index.cshtml` | Vista: tabla paginada + modal Bootstrap |
| `AddressCatalog/Program.cs` | Registro de DI, EF Core, ruta raíz |
| `AddressCatalog/appsettings.json` | Connection string |
| `AddressCatalog.Tests/Services/AddressServiceTests.cs` | Tests unitarios del servicio |

---

### Task 1: Scaffold — solución, proyectos y .gitignore

**Files:**
- Create: `AddressCatalog.sln`
- Create: `AddressCatalog/AddressCatalog.csproj`
- Create: `AddressCatalog.Tests/AddressCatalog.Tests.csproj`
- Create: `.gitignore`

- [ ] **Step 1.1: Crear solución y proyectos**

Desde la raíz del repositorio (`interview-pg-calendar/`):

```powershell
dotnet new webapp -n AddressCatalog -f net8.0
dotnet new xunit -n AddressCatalog.Tests -f net8.0
dotnet new sln -n AddressCatalog
dotnet sln add AddressCatalog/AddressCatalog.csproj
dotnet sln add AddressCatalog.Tests/AddressCatalog.Tests.csproj
```

- [ ] **Step 1.2: Referenciar el proyecto principal desde el de tests**

```powershell
dotnet add AddressCatalog.Tests/AddressCatalog.Tests.csproj reference AddressCatalog/AddressCatalog.csproj
```

- [ ] **Step 1.3: Agregar paquetes NuGet al proyecto principal**

```powershell
dotnet add AddressCatalog/AddressCatalog.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add AddressCatalog/AddressCatalog.csproj package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
```

- [ ] **Step 1.4: Agregar paquetes NuGet al proyecto de tests**

```powershell
dotnet add AddressCatalog.Tests/AddressCatalog.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
```

- [ ] **Step 1.5: Crear .gitignore para .NET**

```powershell
dotnet new gitignore
```

- [ ] **Step 1.6: Verificar que el build pase**

```powershell
dotnet build AddressCatalog.sln
```

Esperado: `Build succeeded.`

- [ ] **Step 1.7: Commit**

```
git add .
git commit -m "chore: scaffold solution with Razor Pages and xUnit projects"
```

---

### Task 2: Entidad Address y DbContext

**Files:**
- Create: `AddressCatalog/Data/Entities/Address.cs`
- Create: `AddressCatalog/Data/AppDbContext.cs`
- Modify: `AddressCatalog/appsettings.json`

- [ ] **Step 2.1: Crear `Address.cs`**

Crear `AddressCatalog/Data/Entities/Address.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddressCatalog.Data.Entities;

[Table("Address", Schema = "SalesLT")]
public class Address
{
    [Key]
    public int AddressID { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string CountryRegion { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public Guid rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

- [ ] **Step 2.2: Crear `AppDbContext.cs`**

Crear `AddressCatalog/Data/AppDbContext.cs`:

```csharp
using AddressCatalog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AddressCatalog.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Address> Addresses => Set<Address>();
}
```

- [ ] **Step 2.3: Agregar connection string a `appsettings.json`**

Reemplazar el contenido de `AddressCatalog/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:pruebadevsrv.database.windows.net,1433;Initial Catalog=testdb7;Persist Security Info=False;User ID=pruebadev;Password=Prueba.2025!#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 2.4: Verificar build**

```powershell
dotnet build AddressCatalog.sln
```

Esperado: `Build succeeded.`

- [ ] **Step 2.5: Commit**

```
git add AddressCatalog/Data/ AddressCatalog/appsettings.json
git commit -m "feat: add Address entity and AppDbContext"
```

---

### Task 3: DTO e interfaz del servicio

**Files:**
- Create: `AddressCatalog/Models/AddressEditDto.cs`
- Create: `AddressCatalog/Services/IAddressService.cs`

- [ ] **Step 3.1: Crear `AddressEditDto.cs`**

Crear `AddressCatalog/Models/AddressEditDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace AddressCatalog.Models;

public class AddressEditDto
{
    [Required]
    public int AddressID { get; set; }

    [Required(ErrorMessage = "La ciudad es requerida")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado / provincia es requerido")]
    public string StateProvince { get; set; } = string.Empty;
}
```

- [ ] **Step 3.2: Crear `IAddressService.cs`**

Crear `AddressCatalog/Services/IAddressService.cs`:

```csharp
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
```

- [ ] **Step 3.3: Build**

```powershell
dotnet build AddressCatalog.sln
```

Esperado: `Build succeeded.`

- [ ] **Step 3.4: Commit**

```
git add AddressCatalog/Models/ AddressCatalog/Services/IAddressService.cs
git commit -m "feat: add AddressEditDto and IAddressService interface"
```

---

### Task 4: Tests unitarios del servicio (TDD — primero los tests)

**Files:**
- Delete: `AddressCatalog.Tests/UnitTest1.cs`
- Create: `AddressCatalog.Tests/Services/AddressServiceTests.cs`

- [ ] **Step 4.1: Eliminar el archivo de test por defecto**

```powershell
Remove-Item AddressCatalog.Tests/UnitTest1.cs
```

- [ ] **Step 4.2: Crear `AddressServiceTests.cs`**

Crear `AddressCatalog.Tests/Services/AddressServiceTests.cs`:

```csharp
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
```

- [ ] **Step 4.3: Correr tests — deben FALLAR (AddressService no existe aún)**

```powershell
dotnet test AddressCatalog.Tests/AddressCatalog.Tests.csproj
```

Esperado: **error de compilación** — `AddressService` no está definido. Esto confirma que los tests están bien y que ahora hay que implementar el servicio.

- [ ] **Step 4.4: Commit de los tests fallidos**

```
git add AddressCatalog.Tests/
git commit -m "test: add failing AddressService unit tests (TDD red)"
```

---

### Task 5: Implementar AddressService (hacer pasar los tests)

**Files:**
- Create: `AddressCatalog/Services/AddressService.cs`

- [ ] **Step 5.1: Crear `AddressService.cs`**

Crear `AddressCatalog/Services/AddressService.cs`:

```csharp
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
```

- [ ] **Step 5.2: Correr tests — todos deben PASAR**

```powershell
dotnet test AddressCatalog.Tests/AddressCatalog.Tests.csproj --verbosity normal
```

Esperado:
```
Passed!  - Failed: 0, Passed: 5, Skipped: 0
```

- [ ] **Step 5.3: Commit**

```
git add AddressCatalog/Services/AddressService.cs
git commit -m "feat: implement AddressService — all tests green"
```

---

### Task 6: Registrar dependencias en Program.cs

**Files:**
- Modify: `AddressCatalog/Program.cs`

- [ ] **Step 6.1: Reemplazar `Program.cs`**

Reemplazar el contenido de `AddressCatalog/Program.cs`:

```csharp
using AddressCatalog.Data;
using AddressCatalog.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAddressService, AddressService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Addresses"));

app.Run();
```

- [ ] **Step 6.2: Build**

```powershell
dotnet build AddressCatalog/AddressCatalog.csproj
```

Esperado: `Build succeeded.`

- [ ] **Step 6.3: Commit**

```
git add AddressCatalog/Program.cs
git commit -m "feat: configure DI, EF Core and default route in Program.cs"
```

---

### Task 7: Index PageModel

**Files:**
- Create: `AddressCatalog/Pages/Addresses/Index.cshtml.cs`

- [ ] **Step 7.1: Crear `Index.cshtml.cs`**

Crear `AddressCatalog/Pages/Addresses/Index.cshtml.cs`:

```csharp
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
    public SelectList Cities { get; set; } = null!;
    public SelectList States { get; set; } = null!;

    [BindProperty]
    public AddressEditDto EditModel { get; set; } = new();

    public async Task OnGetAsync(int page = 1)
    {
        CurrentPage = page;
        var (items, total) = await _service.GetPagedAsync(page, PageSize);
        Addresses = items;
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
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            await LoadDropdownsAsync();
            return Page();
        }

        var updated = await _service.UpdateAsync(EditModel);
        if (!updated) return NotFound();

        return RedirectToPage(new { page = currentPage });
    }

    private async Task LoadDropdownsAsync()
    {
        var cities = await _service.GetDistinctCitiesAsync();
        var states = await _service.GetDistinctStatesAsync();
        Cities = new SelectList(cities);
        States = new SelectList(states);
    }
}
```

- [ ] **Step 7.2: Build**

```powershell
dotnet build AddressCatalog/AddressCatalog.csproj
```

Esperado: `Build succeeded.`

- [ ] **Step 7.3: Commit**

```
git add AddressCatalog/Pages/Addresses/Index.cshtml.cs
git commit -m "feat: add Index PageModel with GET list and POST edit handler"
```

---

### Task 8: Vista — tabla paginada y modal

> **Nota para la implementación:** Invocar el skill `frontend-design:frontend-design` para elevar la calidad visual de esta tarea antes de escribir el HTML.

**Files:**
- Create: `AddressCatalog/Pages/Addresses/Index.cshtml`

- [ ] **Step 8.1: Crear `Index.cshtml`**

Crear `AddressCatalog/Pages/Addresses/Index.cshtml`:

```cshtml
@page
@model AddressCatalog.Pages.Addresses.IndexModel
@{
    ViewData["Title"] = "Catálogo de Direcciones";
}

<div class="container-fluid py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h3 mb-0">Catálogo de Direcciones</h1>
        <span class="text-muted small">Página @Model.CurrentPage de @Model.TotalPages</span>
    </div>

    <div class="card shadow-sm">
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover mb-0">
                    <thead class="table-dark">
                        <tr>
                            <th>#</th>
                            <th>Dirección</th>
                            <th>Ciudad</th>
                            <th>Estado / Provincia</th>
                            <th>País</th>
                            <th>C.P.</th>
                            <th>Última modificación</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var address in Model.Addresses)
                        {
                            <tr>
                                <td class="text-muted small align-middle">@address.AddressID</td>
                                <td class="align-middle">@address.AddressLine1</td>
                                <td class="align-middle fw-semibold">@address.City</td>
                                <td class="align-middle">@address.StateProvince</td>
                                <td class="align-middle">@address.CountryRegion</td>
                                <td class="align-middle">@address.PostalCode</td>
                                <td class="align-middle text-muted small">
                                    @address.ModifiedDate.ToString("dd/MM/yyyy HH:mm")
                                </td>
                                <td class="align-middle text-end">
                                    <button type="button"
                                            class="btn btn-sm btn-outline-primary"
                                            onclick="openEditModal(@address.AddressID,
                                                '@Html.Raw(address.City.Replace("'", "\\'"))',
                                                '@Html.Raw(address.StateProvince.Replace("'", "\\'"))')">
                                        ✏ Editar
                                    </button>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    @* Controles de paginación *@
    @if (Model.TotalPages > 1)
    {
        <nav class="mt-4" aria-label="Paginación">
            <ul class="pagination justify-content-center">
                <li class="page-item @(Model.CurrentPage == 1 ? "disabled" : "")">
                    <a class="page-link" asp-page="./Index" asp-route-page="@(Model.CurrentPage - 1)">
                        ‹ Anterior
                    </a>
                </li>

                @for (int i = Math.Max(1, Model.CurrentPage - 2); i <= Math.Min(Model.TotalPages, Model.CurrentPage + 2); i++)
                {
                    <li class="page-item @(i == Model.CurrentPage ? "active" : "")">
                        <a class="page-link" asp-page="./Index" asp-route-page="@i">@i</a>
                    </li>
                }

                <li class="page-item @(Model.CurrentPage == Model.TotalPages ? "disabled" : "")">
                    <a class="page-link" asp-page="./Index" asp-route-page="@(Model.CurrentPage + 1)">
                        Siguiente ›
                    </a>
                </li>
            </ul>
        </nav>
    }
</div>

@* Modal de edición *@
<div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <form method="post" asp-page-handler="Edit" asp-route-currentPage="@Model.CurrentPage">
                <div class="modal-header">
                    <h5 class="modal-title" id="editModalLabel">
                        Editar Dirección <span id="modalAddressId" class="text-muted fw-normal small"></span>
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" asp-for="EditModel.AddressID" id="EditModel_AddressID" />

                    <div class="mb-3">
                        <label asp-for="EditModel.City" class="form-label fw-semibold">Ciudad</label>
                        <select asp-for="EditModel.City"
                                asp-items="Model.Cities"
                                class="form-select"
                                id="EditModel_City">
                            <option value="">-- Selecciona una ciudad --</option>
                        </select>
                        <span asp-validation-for="EditModel.City" class="text-danger small"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="EditModel.StateProvince" class="form-label fw-semibold">Estado / Provincia</label>
                        <select asp-for="EditModel.StateProvince"
                                asp-items="Model.States"
                                class="form-select"
                                id="EditModel_StateProvince">
                            <option value="">-- Selecciona un estado --</option>
                        </select>
                        <span asp-validation-for="EditModel.StateProvince" class="text-danger small"></span>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                    <button type="submit" class="btn btn-primary">Guardar cambios</button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        function openEditModal(id, city, state) {
            document.getElementById('EditModel_AddressID').value = id;
            document.getElementById('EditModel_City').value = city;
            document.getElementById('EditModel_StateProvince').value = state;
            document.getElementById('modalAddressId').textContent = '#' + id;
            new bootstrap.Modal(document.getElementById('editModal')).show();
        }
    </script>
}
```

- [ ] **Step 8.2: Levantar la app y verificar visualmente**

```powershell
dotnet run --project AddressCatalog/AddressCatalog.csproj
```

Abrir en el browser: `https://localhost:<PORT>/Addresses`

Verificar:
- [ ] Tabla carga con 20 registros
- [ ] Controles de paginación visibles y funcionales
- [ ] Botón "Editar" en cada fila

- [ ] **Step 8.3: Probar el flujo de edición**

1. Click "Editar" en cualquier fila
2. El modal se abre con City y StateProvince pre-seleccionados
3. Cambiar ambos valores
4. Click "Guardar cambios"
5. Verificar que la fila se actualiza y `ModifiedDate` muestra la hora actual

- [ ] **Step 8.4: Commit**

```
git add AddressCatalog/Pages/Addresses/Index.cshtml
git commit -m "feat: add paginated address table with Bootstrap edit modal"
```

---

### Task 9: Verificación final

- [ ] **Step 9.1: Correr todos los tests**

```powershell
dotnet test AddressCatalog.sln --verbosity normal
```

Esperado: `Passed! — Failed: 0, Passed: 5, Skipped: 0`

- [ ] **Step 9.2: Smoke test completo**

Con la app corriendo (`dotnet run`), verificar:

- [ ] `/` redirige a `/Addresses`
- [ ] Lista muestra 20 registros por página
- [ ] Paginación navega de página 1 a 23 (450 / 20 = 22.5 → 23 páginas)
- [ ] Modal abre con datos pre-seleccionados
- [ ] Guardar actualiza City, StateProvince y ModifiedDate en la BD
- [ ] Cancelar cierra el modal sin cambios
- [ ] Al recargar la página después de guardar, los cambios persisten

- [ ] **Step 9.3: Commit final**

```
git add -A
git commit -m "chore: smoke test complete — catalog ready for deployment"
```
