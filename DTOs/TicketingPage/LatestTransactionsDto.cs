using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.TicketingPage
{
    public class LatestTransactionsDto
    {
        public string CustomerName { get; set; }
        public string PurchaseDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}