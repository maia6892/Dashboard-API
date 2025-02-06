using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.PointOfSalesPage
{
    public class Market
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<MarketCustomer> Customers { get; set; }
        public ICollection<MarketProduct> Products { get; set; }
        public ICollection<MarketActivity> Activities { get; set; }
        public ICollection<MarketPackage> Packages { get; set; }
    }
}