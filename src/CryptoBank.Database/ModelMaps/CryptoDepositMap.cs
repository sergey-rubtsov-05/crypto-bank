using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.ModelMaps;

public static class CryptoDepositMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CryptoDeposit>();
        entity.ToTable("crypto_deposits");
        entity.HasKey(deposit => deposit.Id);
        entity.Property(deposit => deposit.Id).HasColumnName("id").IsRequired().UseIdentityAlwaysColumn();
        entity.Property(deposit => deposit.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(deposit => deposit.AddressId).HasColumnName("address_id").IsRequired();
        entity.HasOne(deposit => deposit.Address).WithMany().HasForeignKey(deposit => deposit.AddressId);
        entity.Property(deposit => deposit.Amount).HasColumnName("amount").IsRequired();
        entity.Property(deposit => deposit.CurrencyCode).HasColumnName("currency_code").IsRequired();
        entity.Property(deposit => deposit.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(deposit => deposit.TxId).HasColumnName("tx_id").IsRequired();
        entity.Property(deposit => deposit.Confirmations).HasColumnName("confirmations").IsRequired();
        entity.Property(deposit => deposit.Status).HasColumnName("status").IsRequired();
    }
}
