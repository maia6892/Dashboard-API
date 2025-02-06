using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class DeliveryStatusDto
    {
        public int Delivered { get; set; }
        public int DeliveredPercentage { get; set; }
        public int OnProgress { get; set; }
        public int OnProgressPercentage { get; set; }
        public List<DeliveryStatusMonthsDto> DeliveryStatusMonths { get; set; }
    }

    public class DeliveryStatusMonthsDto
    {
        public string Month { get; set; }
        public int DeliveredThisMonth { get; set; }
        public int OnProgressThisMonth { get; set; }
    }
}