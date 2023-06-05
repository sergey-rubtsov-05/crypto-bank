using CryptoBank.Database.ModelMaps;
using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database;

public class CryptoBankDbContext : DbContext
{
    public CryptoBankDbContext(DbContextOptions<CryptoBankDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AccountMap.Create(modelBuilder);
        UserMap.Create(modelBuilder);
    }
}
