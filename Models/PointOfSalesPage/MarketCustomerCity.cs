using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.PointOfSalesPage
{
    public class MarketCustomerCity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MarketCustomerCountry Country { get; set; }
    }
}