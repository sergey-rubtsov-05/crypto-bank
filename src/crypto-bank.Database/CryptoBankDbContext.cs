using crypto_bank.Database.ModelMaps;
using crypto_bank.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace crypto_bank.Database;

public class CryptoBankDbContext : DbContext
{
    private readonly ILoggerFactory? _loggerFactory;

    public CryptoBankDbContext()
    {
    }

    public CryptoBankDbContext(ILoggerFactory? loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //todo move password to secrets
        var connectionString = "Host=localhost;Port=5432;Database=crypto_bank_db;Username=postgres;Password=nonsecret";
        optionsBuilder
            .UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseAdminDatabase("postgres"))
            .UseLoggerFactory(_loggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AccountMap.Create(modelBuilder);
        UserMap.Create(modelBuilder);
    }
}
