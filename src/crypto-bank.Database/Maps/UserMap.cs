using crypto_bank.Domain;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database.Maps;

public class UserMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var userEntityBuilder = modelBuilder.Entity<User>();
        userEntityBuilder.ToTable("users");
        userEntityBuilder.Property(user => user.Id).HasColumnName("id").IsRequired().UseIdentityAlwaysColumn();
        userEntityBuilder.Property(user => user.Email).HasColumnName("email");
        userEntityBuilder.Property(user => user.Password).HasColumnName("password");
    }
}
