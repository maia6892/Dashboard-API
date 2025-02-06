using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.InvoicingPage
{
    public class WeeklyWalletTransactionsDto
    {
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public decimal Amount { get; set; }
    }
}