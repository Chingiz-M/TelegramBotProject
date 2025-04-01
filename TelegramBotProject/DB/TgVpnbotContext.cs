using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TelegramBotProject.Entities;

namespace TelegramBotProject.DB;

public partial class TgVpnbotContext : DbContext
{
    public TgVpnbotContext()
    {
    }

    public TgVpnbotContext(DbContextOptions<TgVpnbotContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserDB> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = StartUp.GetTokenfromConfig("DBConnection");
        //optionsBuilder.UseSqlite(connection);
        optionsBuilder.UseNpgsql(connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDB>().ToTable("users");
    }
}
