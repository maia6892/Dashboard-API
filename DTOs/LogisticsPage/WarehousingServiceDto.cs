using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class WarehousingServiceDto
    {
        public string ServiceName { get; set; }
        public decimal ServicePayment { get; set; }
        public decimal ServicePaymentPercentage { get; set; }
    }
}