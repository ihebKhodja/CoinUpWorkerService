using CoinUpWorkerService.Models;
namespace CoinUpWorkerService.Services
{
    public interface IDataCollectorService
    {
        Task<List<MarketData>> FetchMarketDataAsync();
    }


}