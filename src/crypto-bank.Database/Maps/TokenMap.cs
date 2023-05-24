using crypto_bank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database.Maps;

public class TokenMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var tokenEntityBuilder = modelBuilder.Entity<Token>();
        tokenEntityBuilder.ToTable("tokens");
        tokenEntityBuilder.Property(token => token.Id).HasColumnName("id").IsRequired();
        tokenEntityBuilder.Property(token => token.AccessToken).HasColumnName("access_token").IsRequired();
        tokenEntityBuilder.Property(token => token.RefreshToken).HasColumnName("refresh_token").IsRequired();

        tokenEntityBuilder.HasIndex(token => token.Id).IsUnique();
    }
}
