using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.CryptocurrencyPage
{

    public class Trade
    {
        public int Id { get; set; }
        public CryptoUser User { get; set; }
        public string CurrencyName { get; set; }
        public Currency Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalWithoutFees { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal TotalWithFees { get; set; } 
        public string OrderTypeName { get; set; } 
        public OrderType OrderType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}