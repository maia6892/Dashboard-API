using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.CryptocurrencyPage;

namespace DashboardAPI.DTOs.CryptocurrencyPage
{
    public class TradeRequest
    {
        public decimal Amount { get; set; } 
        public decimal PricePerUnit { get; set; }
        public decimal FeesPercentage { get; set; }
        public string OrderTypeName { get; set; }
    }
}