using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class DailyOrderReportDto
    {
        public string Day { get; set; }
        public int Orders { get; set; }
        public string Date { get; set; }
    }
}