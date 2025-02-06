using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.TicketingPage
{
    public class TicketSalesAnalyticsDto
    {
        public string DateOrTime { get; set; }
        public decimal Amount { get; set; }
    }
}