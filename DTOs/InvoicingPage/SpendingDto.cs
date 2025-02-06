using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.InvoicingPage
{
    public class SpendingDto
    {
        public string Spending { get; set; }
        public decimal Amount { get; set; }
        public decimal Limit { get; set; }
        public int SpendingPercentage { get; set; }
    }
}