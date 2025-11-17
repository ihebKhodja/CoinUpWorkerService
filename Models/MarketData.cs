using System.ComponentModel.DataAnnotations;

namespace CoinUpWorkerService.Models
{
    public class MarketData
    {
        [Key]
        public int Id { get; set; }
        public string AssetId { get; set; } = string.Empty; // Ex: "bitcoin"
        public string Name { get; set; } = string.Empty;    // Ex: "Bitcoin"
        public string Symbol { get; set; } = string.Empty;  // Ex: "BTC"
        public decimal PriceUsd { get; set; }
        public decimal VolumeUsd24Hr { get; set; }
        public decimal MarketCapUsd { get; set; }
        public decimal ChangePercent24Hr { get; set; }
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    }

}
