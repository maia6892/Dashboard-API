using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.TicketingPage
{
    public class CustomerStatisticsDto
    {
        public string CustomerAgeGroup { get; set; }
        public int CustomerPercentage { get; set; }
    }
}