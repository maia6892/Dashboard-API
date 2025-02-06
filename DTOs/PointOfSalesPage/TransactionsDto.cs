using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.PointOfSalesPage
{
    public class TransactionsDto
    {
        public int OrderId { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string Customer { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}