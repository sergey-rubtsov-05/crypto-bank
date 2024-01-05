using CryptoBank.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.Database.ModelMaps;

public static class BitcoinBlockchainStatusMap
{
    public static void Create(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BitcoinBlockchainStatus>();
        entity.ToTable("bitcoin_blockchain_statuses");
        entity.HasKey(status => status.Id);
        entity.Property(status => status.Id).HasColumnName("id").IsRequired().UseIdentityAlwaysColumn();
        entity.Property(status => status.LastProcessedBlockHeight).HasColumnName("last_processed_block_height")
            .IsRequired();
    }
}
