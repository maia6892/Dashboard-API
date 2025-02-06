using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.CryptocurrencyPage
{
    public class SellOrBuyOrderDto
    {
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
    }
}