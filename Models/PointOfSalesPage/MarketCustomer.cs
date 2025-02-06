using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.PointOfSalesPage
{
    public class MarketCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MarketCustomerCity City { get; set; }
        public string Card { get; set; }
        public decimal Balance { get; set; }
        public Market Market { get; set; }
    }
}