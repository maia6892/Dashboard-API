using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class ShipmentReviewDto
    {
        public int Canceled { get; set; }
        public int CanceledPercentage { get; set; }
        public int Delivered { get; set; }
        public int DeliveredPercentage { get; set; }
        public int Orders { get; set; }
        public int OrdersPercentage { get; set; }
        public int Pending { get; set; }
        public int PendingPercentage { get; set; }
        public int Revenue { get; set; }
        public int RevenuePercentage { get; set; }
        public int Refunded { get; set; }
        public int RefundedPercentage { get; set; }
    }
}