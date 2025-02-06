using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.PointOfSalesPage
{
    public class ProductSalesDto
    {
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string Status { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public int Sales { get; set; }
        public decimal Earnings { get; set; }
    }
}