using crypto_bank.Domain.Features.Accounts.Models;
using Microsoft.EntityFrameworkCore;

namespace crypto_bank.Database.Features.Accounts;

public static class AccountMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var accountEntity = modelBuilder.Entity<Account>();
        accountEntity.ToTable("accounts");
        accountEntity.HasKey(account => account.Number);
        accountEntity.Property(account => account.Number).HasColumnName("number").IsRequired()
            .UseIdentityAlwaysColumn();
        accountEntity.Property(account => account.Currency).HasColumnName("currency").IsRequired();
        accountEntity.Property(account => account.Amount).HasColumnName("amount");
        accountEntity.Property(account => account.OpenedAt).HasColumnName("opened_at").IsRequired();
        accountEntity.Property(account => account.UserId).HasColumnName("user_id").IsRequired();
        accountEntity.HasOne(account => account.User).WithMany().HasForeignKey(account => account.UserId);
    }
}
