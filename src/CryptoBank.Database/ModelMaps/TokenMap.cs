using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.ModelMaps;

public static class TokenMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Token>();
        entity.ToTable("tokens");
        entity.HasKey(token => token.RefreshToken);
        entity.Property(token => token.RefreshToken).HasColumnName("refresh_token").IsRequired();
        entity.Property(token => token.UserId).HasColumnName("user_id").IsRequired();
        entity.HasOne(token => token.User).WithMany().HasForeignKey(token => token.UserId);
        entity.Property(token => token.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(token => token.ExpirationTime).HasColumnName("expiration_time").IsRequired();
        entity.Property(token => token.IsRevoked).HasColumnName("is_revoked").IsRequired();
        entity.Property(token => token.ReplacedById).HasColumnName("replaced_by_id");
        entity.HasOne(token => token.ReplacedBy).WithOne().HasForeignKey<Token>(token => token.ReplacedById);
    }
}
