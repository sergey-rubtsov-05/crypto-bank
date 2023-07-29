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
    public DbSet<BitcoinBlockchainStatus> BitcoinBlockchainStatuses => Set<BitcoinBlockchainStatus>();
    public DbSet<CryptoDeposit> CryptoDeposits => Set<CryptoDeposit>();
    public DbSet<DepositAddress> DepositAddresses => Set<DepositAddress>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Xpub> Xpubs => Set<Xpub>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AccountMap.Create(modelBuilder);
        BitcoinBlockchainStatusMap.Create(modelBuilder);
        CryptoDepositMap.Create(modelBuilder);
        DepositAddressMap.Create(modelBuilder);
        TokenMap.Create(modelBuilder);
        UserMap.Create(modelBuilder);
        XpubMap.Create(modelBuilder);
    }
}
