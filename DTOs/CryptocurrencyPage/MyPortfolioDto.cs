using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.CryptocurrencyPage
{
    public class MyPortfolioDto
    {
        public string CurrencyShortName { get; set; }
        public decimal Price { get; set; }
        public decimal PricePercentage { get; set; }
        public decimal Rate { get; set; }
        public decimal Volume { get; set; }
        public decimal VolumePercentage { get; set; }
        public List<decimal> PriceData { get; set; }
        

    }
}