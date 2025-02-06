using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.LogisticsPage;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class SalesByStoresLocationDto
    {
        public string Month { get; set; }
        public List<SalesByStoresLocationByMonthDto> SalesByStoresLocationByMonth { get; set; }
    }

    public class SalesByStoresLocationByMonthDto
    {
        public string SidesOfWorld { get; set; }
        public int TotalOrders { get; set; }
    }
}