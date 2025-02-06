using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.PointOfSalesPage
{
    public class SalesByCategoryDto
    {
        public string CategoryName { get; set; }
        public int SalePercentage { get; set; }
        public int CategoryProducts { get; set; }
    }
}