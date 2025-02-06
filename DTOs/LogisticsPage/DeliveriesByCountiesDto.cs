using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class DeliveriesByCountiesDto
    {
        public string Country { get; set; }
        public int DeliveryPercentage { get; set; }
    }
}