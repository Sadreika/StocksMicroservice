namespace Repositories.Interfaces
{
    public interface IStockRepo
    {
        Task<bool> CreateStock(string stockData);
    }
}
