using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.ModelMaps;

public static class DepositAddressMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DepositAddress>();
        entity.ToTable("deposit_addresses");
        entity.HasKey(address => address.Id);
        entity.Property(address => address.Id).HasColumnName("id").IsRequired().UseIdentityAlwaysColumn();
        entity.Property(address => address.CurrencyCode).HasColumnName("currency_code").IsRequired();
        entity.Property(address => address.UserId).HasColumnName("user_id").IsRequired();
        entity.HasOne(address => address.User).WithOne().HasForeignKey<DepositAddress>(address => address.UserId);
        entity.Property(address => address.XpubId).HasColumnName("xpub_id").IsRequired();
        entity.HasOne(address => address.Xpub).WithMany().HasForeignKey(address => address.XpubId);
        entity.Property(address => address.DerivationIndex).HasColumnName("derivation_index").IsRequired();
        entity.Property(address => address.CryptoAddress).HasColumnName("crypto_address").IsRequired();
    }
}
