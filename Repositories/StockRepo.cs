using Domain.DbContexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Repositories.Interfaces;

namespace Repositories
{
    public class StockRepo : IStockRepo
    {
        private readonly ILogger<StockRepo> _logger;
        private readonly StocksDbContext _context;

        public StockRepo(ILogger<StockRepo> logger, StocksDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<bool> CreateStock(string stockData)
        {
            try
            {
                var jData = JObject.Parse(stockData);
                var yieldParseResult = decimal.TryParse(jData["summaryDetail"]["yield"].ToString(), out decimal yield);
                var regularMarketPriceParseResult = decimal.TryParse(jData["price"]["regularMarketPrice"]["fmt"].ToString(), out decimal regularMarketPrice);

                var newStock = new Stock()
                {
                    Name = jData["price"]["longName"].ToString(),
                    Price = regularMarketPriceParseResult ? regularMarketPrice : 0.0m,
                    Currency = jData["price"]["currency"].ToString(),
                    Symbol = jData["quoteType"]["symbol"].ToString(),
                    Yield = yieldParseResult ? yield : 0.0m,
                    MarketCap = jData["summaryDetail"]["marketCap"]["fmt"].ToString()
                };

                var stock = GetStock(newStock.Name);
                if (stock == null)
                {
                    _context.Stocks.Add(newStock);
                }
                else
                {
                    stock.Price = newStock.Price;
                    stock.Currency = newStock.Currency;
                    stock.Symbol = newStock.Symbol;
                    stock.Yield = newStock.Yield;
                    stock.MarketCap = newStock.MarketCap;
                }

                return await _context.SaveChangesAsync() >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Could not save stock to database because of exception: {ex.Message}", DateTimeOffset.Now);

                return false;
            }
        }

        public Stock? GetStock(string stockName)
        {
            return _context.Stocks.FirstOrDefault(x => x.Name == stockName);
        }
    }
}