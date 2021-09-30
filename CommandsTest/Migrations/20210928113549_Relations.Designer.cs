﻿// <auto-generated />
using CommandsTest.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CommandsTest.Migrations
{
    [DbContext(typeof(GuildContext))]
    [Migration("20210928113549_Relations")]
    partial class Relations
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-rc.1.21452.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CommandsTest.Data.BlacklistEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Blacklist");
                });

            modelBuilder.Entity("CommandsTest.Data.CommandEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<decimal?>("BlacklistEntityId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Name");

                    b.HasIndex("BlacklistEntityId");

                    b.ToTable("CommandEntity");
                });

            modelBuilder.Entity("CommandsTest.Data.GroupEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<decimal?>("BlacklistEntityId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Name");

                    b.HasIndex("BlacklistEntityId");

                    b.ToTable("GroupEntity");
                });

            modelBuilder.Entity("CommandsTest.Data.GuildEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("CommandsTest.Data.BlacklistEntity", b =>
                {
                    b.HasOne("CommandsTest.Data.GuildEntity", "GuildEntity")
                        .WithMany("Blacklist")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildEntity");
                });

            modelBuilder.Entity("CommandsTest.Data.CommandEntity", b =>
                {
                    b.HasOne("CommandsTest.Data.BlacklistEntity", null)
                        .WithMany("Commands")
                        .HasForeignKey("BlacklistEntityId");
                });

            modelBuilder.Entity("CommandsTest.Data.GroupEntity", b =>
                {
                    b.HasOne("CommandsTest.Data.BlacklistEntity", null)
                        .WithMany("Groups")
                        .HasForeignKey("BlacklistEntityId");
                });

            modelBuilder.Entity("CommandsTest.Data.BlacklistEntity", b =>
                {
                    b.Navigation("Commands");

                    b.Navigation("Groups");
                });

            modelBuilder.Entity("CommandsTest.Data.GuildEntity", b =>
                {
                    b.Navigation("Blacklist");
                });
#pragma warning restore 612, 618
        }
    }
}
