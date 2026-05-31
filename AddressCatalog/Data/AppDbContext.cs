using AddressCatalog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AddressCatalog.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Address> Addresses => Set<Address>();
}
