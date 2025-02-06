using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.CryptocurrencyPage
{
    public class TradeOrder
    {
        public int Id { get; set; }
        public OrderType OrderType { get; set; }
        public Currency Currency { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
    }
}

