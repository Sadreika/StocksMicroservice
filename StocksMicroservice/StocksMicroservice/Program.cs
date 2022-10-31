namespace StocksMicroservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => {
                services.AddHostedService<Worker>();
            }).Build().Run();
        }
    }
}