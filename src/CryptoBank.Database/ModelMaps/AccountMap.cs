using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.ModelMaps;

public static class AccountMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var accountEntity = modelBuilder.Entity<Account>();
        accountEntity.ToTable("accounts");
        accountEntity.HasKey(account => account.Number);
        accountEntity.Property(account => account.Number).HasColumnName("number").IsRequired();
        accountEntity.Property(account => account.Currency).HasColumnName("currency").IsRequired();
        accountEntity.Property(account => account.Amount).HasColumnName("amount").IsRequired();
        accountEntity.Property(account => account.OpenedAt).HasColumnName("opened_at").IsRequired();
        accountEntity.Property(account => account.UserId).HasColumnName("user_id").IsRequired();
        accountEntity.HasOne(account => account.User).WithMany().HasForeignKey(account => account.UserId);
    }
}
