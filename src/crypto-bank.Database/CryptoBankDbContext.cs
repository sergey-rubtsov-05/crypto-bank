using crypto_bank.Database.Features.Accounts;
using crypto_bank.Database.Features.Users;
using crypto_bank.Domain.Features.Accounts.Models;
using crypto_bank.Domain.Features.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database;

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
