using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.InvoicingPage
{
    public class QuickReviewDto
    {
        public int TotalInvoices { get; set; }
        public int TotalInvoicesPercentage { get; set; }
        public int PaidInvoices { get; set; }
        public int PaidInvoicesPercentage { get; set; }
        public int UnpaidInvoices { get; set; }
        public int UnpaidInvoicesPercentage { get; set; }
        public int InvoiceSent { get; set; }
        public int InvoiceSentPercentage { get; set; }
    }
}