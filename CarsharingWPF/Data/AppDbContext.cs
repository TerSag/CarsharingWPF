using Microsoft.EntityFrameworkCore;
using CarsharingWPF.Models;

namespace CarsharingWPF.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        public bool WasCreated { get; }

        public AppDbContext()
        {
            WasCreated = Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=carsharing.db");
        }
    }
}
