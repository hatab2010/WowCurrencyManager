using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.v2.Data;

namespace Data
{
    public class MobileContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Purse> Purses { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Order> Orders { get; set; }

        public MobileContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Mobile.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Channel>()
                .Property(e => e.ChannelRole)
                .HasConversion(
                    v => v.ToString(),
                    v => (ChannelRole)Enum.Parse(typeof(ChannelRole), v));
            
            modelBuilder
                .Entity<Channel>()
                .Property(e => e.GameVersion)
                .HasConversion(
                    v => v.ToString(),
                    v => (GameVersion)Enum.Parse(typeof(GameVersion), v));

            modelBuilder
                .Entity<Channel>()
                .Property(e => e.Fraction)
                .HasConversion(
                    v => v.ToString(),
                    v => (Fraction)Enum.Parse(typeof(Fraction), v));

            modelBuilder
                .Entity<Channel>()
                .Property(e => e.WorldPart)
                .HasConversion(
                    v => v.ToString(),
                    v => (WorldPart)Enum.Parse(typeof(WorldPart), v));
        }
    }
}
