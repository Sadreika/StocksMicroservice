using Domain.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace StocksMicroservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => {
                services.AddHostedService<Worker>();
                services.AddDbContext<StocksDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("StocksDb")));
            }).Build().Run();
        }
    }
}