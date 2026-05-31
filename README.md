# Address Catalog

Web application for managing an address catalog. Displays paginated records from an existing database and allows editing the city and state/province fields via a modal interface with dropdown selectors. Tracks the last modification date on every update.

## Tech Stack

- **Framework:** ASP.NET Core 9 — Razor Pages
- **ORM:** Entity Framework Core 9
- **Database:** Azure SQL (SalesLT schema)
- **Testing:** xUnit + EF Core InMemory
- **UI:** Bootstrap 5 + custom CSS (dark theme)

## Architecture

```
Pages/Addresses/Index (PageModel)
        ↓
IAddressService → AddressService
        ↓
AppDbContext (EF Core)
        ↓
Azure SQL — SalesLT.Address
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- Access to the Azure SQL database

## Local Development

**1. Clone the repo**

```bash
git clone https://github.com/VictorMag/address-catalog.git
cd address-catalog
```

**2. Set the connection string via User Secrets**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING" --project AddressCatalog/AddressCatalog.csproj
```

**3. Run the app**

```bash
dotnet run --project AddressCatalog/AddressCatalog.csproj
```

Open `https://localhost:PORT/Addresses` in your browser.

## Running Tests

```bash
dotnet test AddressCatalog.sln --verbosity normal
```

All 5 unit tests cover the service layer:

| Test | Description |
|------|-------------|
| `GetPagedAsync_FirstPage_Returns20Items` | Page 1 of 25 returns 20 items |
| `GetPagedAsync_SecondPage_ReturnsRemainder` | Page 2 of 25 returns remaining 5 |
| `GetDistinctCitiesAsync_ReturnsUniqueSortedCities` | Deduplicates and sorts alphabetically |
| `UpdateAsync_UpdatesCityStateAndModifiedDate` | Updates 3 fields and sets ModifiedDate |
| `UpdateAsync_ReturnsFalse_WhenIdNotFound` | Returns false for non-existent ID |

## Deployment

**1. Publish the app**

```bash
dotnet publish AddressCatalog/AddressCatalog.csproj -c Release -o ./publish
```

**2. Set the connection string as environment variable on the server**

```powershell
[System.Environment]::SetEnvironmentVariable(
  "ConnectionStrings__DefaultConnection",
  "YOUR_CONNECTION_STRING",
  "Machine"
)
```

**3. Run with Kestrel**

```bash
dotnet AddressCatalog.dll --urls "http://0.0.0.0:5000"
```

## Project Structure

```
AddressCatalog/
├── Data/
│   ├── AppDbContext.cs
│   └── Entities/
│       └── Address.cs
├── Models/
│   └── AddressEditDto.cs
├── Services/
│   ├── IAddressService.cs
│   └── AddressService.cs
├── Pages/
│   └── Addresses/
│       ├── Index.cshtml
│       └── Index.cshtml.cs
└── Program.cs

AddressCatalog.Tests/
└── Services/
    └── AddressServiceTests.cs
```
