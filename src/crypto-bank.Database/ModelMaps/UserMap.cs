using crypto_bank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database.ModelMaps;

public static class UserMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var userEntityBuilder = modelBuilder.Entity<User>();
        userEntityBuilder.ToTable("users");
        userEntityBuilder.HasKey(user => user.Id);
        userEntityBuilder.Property(user => user.Id).HasColumnName("id").IsRequired().UseIdentityAlwaysColumn();
        userEntityBuilder.Property(user => user.Email).HasColumnName("email").IsRequired();
        userEntityBuilder.Property(user => user.PasswordHash).HasColumnName("password_hash").IsRequired();
        userEntityBuilder.Property(user => user.BirthDate).HasColumnName("birth_date");
        userEntityBuilder.Property(user => user.RegisteredAt).HasColumnName("registered_at").IsRequired();
        userEntityBuilder.Property(user => user.Roles).HasColumnName("roles").IsRequired();

        userEntityBuilder.HasIndex(user => user.Email).IsUnique();
    }
}
