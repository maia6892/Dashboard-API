using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class TodaysLiveOrdersDto
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
    }
}