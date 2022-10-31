﻿using Domain.DbContexts;
using Domain.Entities;
using Newtonsoft.Json.Linq;

namespace StocksMicroservice
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly StocksDbContext _context;

        public Worker(ILogger<Worker> logger, StocksDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var stockShortName = "TSLA"; //<= replace
                var stockData = await GetStock(stockShortName);
                if (stockData != null)
                {
                    var status = await CreateStock(stockData); //<= replace
                }

                await Task.Delay(5000, stoppingToken);
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

        private async Task<bool> CreateStock(string stockData)
        {
            try
            {
                var jData = JObject.Parse(stockData);
                var yieldParseResult = decimal.TryParse(jData["summaryDetail"]["yield"].ToString(), out decimal yield);
                var regularMarketPriceParseResult = decimal.TryParse(jData["price"]["regularMarketPrice"]["fmt"].ToString(), out decimal regularMarketPrice);

                _context.Stocks.Add(new Stock()
                {
                    Name = jData["price"]["longName"].ToString(),
                    Price = regularMarketPriceParseResult ? regularMarketPrice : 0.0m,
                    Currency = jData["price"]["currency"].ToString(),
                    Symbol = jData["quoteType"]["symbol"].ToString(),
                    Yield = yieldParseResult ? yield : 0.0m,
                    MarketCap = jData["summaryDetail"]["marketCap"]["fmt"].ToString()
                });

                return await _context.SaveChangesAsync() >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Could not save stock to database because of exception: {ex.Message}", DateTimeOffset.Now);

                return false;
            }
        }
    }
}