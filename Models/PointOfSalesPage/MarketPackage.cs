using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.PointOfSalesPage
{
    public class MarketPackage
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public MarketProduct Product { get; set; }
        public int Quantity { get; set; }
        public MarketPackageStatus Status { get; set; }
        public MarketCustomer Customer { get; set; }
        public DateTime CreatedAt { get; set; }
        public SocialPlatform SocialPlatform { get; set; }
        public Market Market { get; set; }
    }
}