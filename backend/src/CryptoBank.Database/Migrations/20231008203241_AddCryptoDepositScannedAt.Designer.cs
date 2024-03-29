﻿// <auto-generated />
using System;
using CryptoBank.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.Database.Migrations
{
    [DbContext(typeof(CryptoBankDbContext))]
    [Migration("20231008203241_AddCryptoDepositScannedAt")]
    partial class AddCryptoDepositScannedAt
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CryptoBank.Domain.Models.Account", b =>
                {
                    b.Property<string>("Number")
                        .HasColumnType("text")
                        .HasColumnName("number");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric")
                        .HasColumnName("amount");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency");

                    b.Property<DateTimeOffset>("OpenedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("opened_at");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Number");

                    b.HasIndex("UserId");

                    b.ToTable("accounts", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.BitcoinBlockchainStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<int>("LastProcessedBlockHeight")
                        .HasColumnType("integer")
                        .HasColumnName("last_processed_block_height");

                    b.HasKey("Id");

                    b.ToTable("bitcoin_blockchain_statuses", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.CryptoDeposit", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Id"));

                    b.Property<int>("AddressId")
                        .HasColumnType("integer")
                        .HasColumnName("address_id");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric")
                        .HasColumnName("amount");

                    b.Property<long>("Confirmations")
                        .HasColumnType("bigint")
                        .HasColumnName("confirmations");

                    b.Property<DateTimeOffset?>("ConfirmedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("confirmed_at");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency_code");

                    b.Property<DateTimeOffset?>("ScannedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scanned_at");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<string>("TxId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("tx_id");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.ToTable("crypto_deposits", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.DepositAddress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("CryptoAddress")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("crypto_address");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency_code");

                    b.Property<long>("DerivationIndex")
                        .HasColumnType("bigint")
                        .HasColumnName("derivation_index");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<int>("XpubId")
                        .HasColumnType("integer")
                        .HasColumnName("xpub_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.HasIndex("XpubId");

                    b.ToTable("deposit_addresses", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.Token", b =>
                {
                    b.Property<string>("RefreshToken")
                        .HasColumnType("text")
                        .HasColumnName("refresh_token");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTimeOffset>("ExpirationTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expiration_time");

                    b.Property<bool>("IsRevoked")
                        .HasColumnType("boolean")
                        .HasColumnName("is_revoked");

                    b.Property<string>("ReplacedById")
                        .HasColumnType("text")
                        .HasColumnName("replaced_by_id");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("RefreshToken");

                    b.HasIndex("ReplacedById")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("tokens", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<DateOnly?>("BirthDate")
                        .HasColumnType("date")
                        .HasColumnName("birth_date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password_hash");

                    b.Property<DateTimeOffset>("RegisteredAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("registered_at");

                    b.Property<int[]>("Roles")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasColumnName("roles");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.Xpub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency_code");

                    b.Property<long>("LastUsedDerivationIndex")
                        .HasColumnType("bigint")
                        .HasColumnName("last_used_derivation_index");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.ToTable("xpubs", (string)null);
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.Account", b =>
                {
                    b.HasOne("CryptoBank.Domain.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.CryptoDeposit", b =>
                {
                    b.HasOne("CryptoBank.Domain.Models.DepositAddress", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.DepositAddress", b =>
                {
                    b.HasOne("CryptoBank.Domain.Models.User", "User")
                        .WithOne()
                        .HasForeignKey("CryptoBank.Domain.Models.DepositAddress", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CryptoBank.Domain.Models.Xpub", "Xpub")
                        .WithMany()
                        .HasForeignKey("XpubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Xpub");
                });

            modelBuilder.Entity("CryptoBank.Domain.Models.Token", b =>
                {
                    b.HasOne("CryptoBank.Domain.Models.Token", "ReplacedBy")
                        .WithOne()
                        .HasForeignKey("CryptoBank.Domain.Models.Token", "ReplacedById");

                    b.HasOne("CryptoBank.Domain.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReplacedBy");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
