using crypto_bank.Domain;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database;

public class CryptoBankDb : DbContext
{
    public CryptoBankDb(DbContextOptions<CryptoBankDb> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}
