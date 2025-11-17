using CoinUpWorkerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinUpWorkerService.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<MarketData> MarketData { get; set; }

    }
}
