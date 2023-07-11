using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.ModelMaps;

public static class XpubMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Xpub>();

        entity.ToTable("xpubs");

        entity.HasKey(xpub => xpub.Id);

        entity.Property(xpub => xpub.Id).HasColumnName("id").IsRequired().UseIdentityAlwaysColumn();
        entity.Property(xpub => xpub.CurrencyCode).HasColumnName("currency_code").IsRequired();
        entity.Property(xpub => xpub.Value).HasColumnName("value").IsRequired();
        entity.Property(xpub => xpub.LastUsedDerivationIndex).HasColumnName("last_used_derivation_index").IsRequired();
    }
}
