﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using crypto_bank.Database;

#nullable disable

namespace crypto_bank.Database.Migrations
{
    [DbContext(typeof(CryptoBankDbContext))]
    partial class CryptoBankDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("crypto_bank.Domain.Models.Account", b =>
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

            modelBuilder.Entity("crypto_bank.Domain.Models.User", b =>
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

            modelBuilder.Entity("crypto_bank.Domain.Models.Account", b =>
                {
                    b.HasOne("crypto_bank.Domain.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
