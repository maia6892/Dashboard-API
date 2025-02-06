using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.InvoicingPage
{
    public class TransactionOverviewDto
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expences { get; set; }
    }
}