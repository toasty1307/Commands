﻿// <auto-generated />
using Commands.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Commands.Migrations
{
    [DbContext(typeof(GuildContext))]
    partial class GuildContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-rc.1.21452.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Commands.Data.GuildEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Commands")
                        .HasColumnType("text");

                    b.Property<string>("Groups")
                        .HasColumnType("text");

                    b.Property<string>("Prefix")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });
#pragma warning restore 612, 618
        }
    }
}
