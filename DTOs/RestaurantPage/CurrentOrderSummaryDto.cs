using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class CurrentOrderSummaryDto
    {
        public int Delivered { get; set; }
        public int DeliveredPercentage { get; set; }
        public int OnProcess { get; set; }
        public int OnProcessPercentage { get; set; }
        public int NewOrders { get; set; }
        public int NewOrdersPercentage { get; set; }
    }
}