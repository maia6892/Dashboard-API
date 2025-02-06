using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.PointOfSalesPage
{
    public class MarketProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public MarketProductStatus Status { get; set; }
        public MarketCategory Category { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public Market Market { get; set; }
    }
}