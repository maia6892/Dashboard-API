using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class QuickReviewBottomDto
    {
        public int TotalOrdersCompleted { get; set; }
        public int TotalOrdersDelivered { get; set; }
        public int TotalOrdersCanceled { get; set; }
        public int TotalOrdersPending { get; set; }
    }
}