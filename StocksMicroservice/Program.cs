using Domain.DbContexts;

namespace StocksMicroservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => {
                services.AddHostedService<Worker>();
                //services.AddDbContext<StocksDbContext>(options =>
                //    options.UseSqlServer(Configuration.GetConnectionString("StocksDb"), b => b.MigrationsAssembly("Migrations")));
            }).Build().Run();
        }
    }
}