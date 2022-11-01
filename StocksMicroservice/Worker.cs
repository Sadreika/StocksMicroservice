using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.Interfaces;
using System.Text;

namespace StocksMicroservice
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IStockRepo _stockRepo;

        public Worker(ILogger<Worker> logger, IStockRepo stockRepo)
        {
            _logger = logger;
            _stockRepo = stockRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started {0}", DateTime.Now);

            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "StockQueue",
                                       durable: false,
                                       exclusive: false,
                                       autoDelete: false,
                                       arguments: null);

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("Received {0}", message);

                        if (message != null)
                        {
                            var stockData = await GetStock(message);
                            if (stockData != null)
                            {
                                await _stockRepo.CreateStock(stockData);
                            }

                            message = null;
                        }
                    };

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        channel.BasicConsume(queue: "StockQueue",
                                       autoAck: true,
                                       consumer: consumer);

                        await Task.Delay(5000, stoppingToken);
                    }

                    if (stoppingToken.IsCancellationRequested)
                    {
                        channel.BasicConsume(queue: "StockQueue",
                                       autoAck: true,
                                       consumer: consumer);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Worker failed because of exception: {0}", ex.Message);
            }
        }

        private async Task<string?> GetStock(string stockShortName)
        {
            using var response = await new HttpClient().SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://yh-finance.p.rapidapi.com/stock/v2/get-financials?symbol={stockShortName}&region=US"),
                Headers = {
                    { "X-RapidAPI-Key", "cd60461fcbmsh0af02a35e8344e4p1a8fd6jsn1f6b11d17b93" },
                    { "X-RapidAPI-Host", "yh-finance.p.rapidapi.com" }
                }
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Could not get {stockShortName} stock", DateTimeOffset.Now);

                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
