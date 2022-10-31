using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.DbContexts
{
    public class StocksDbContext : DbContext
    {
        public StocksDbContext(DbContextOptions<StocksDbContext> options) : base(options)
        {
        }

        public DbSet<Stock> Stocks { get; set; }
    }
}

