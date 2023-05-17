using crypto_bank.Database.Maps;
using crypto_bank.Domain;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database;

public class CryptoBankDbContext : DbContext
{
    public CryptoBankDbContext(DbContextOptions<CryptoBankDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        UserMap.Create(modelBuilder);
    }
}
