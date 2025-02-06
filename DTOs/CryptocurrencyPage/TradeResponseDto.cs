using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.CryptocurrencyPage;

namespace DashboardAPI.DTOs.CryptocurrencyPage
{
    public class TradeResponse
    {
        public string CurrencyName { get; set; }
        public decimal Amount { get; set; } 
        public decimal PricePerUnit { get; set; } 
        public decimal TotalWithoutFees { get; set; }
        public decimal FeesAmount { get; set; } 
        public decimal TotalWithFees { get; set; }
        public string OrderType { get; set; } 
    }

}