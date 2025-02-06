using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.InvoicingPage
{
    public class QuickTransferDto
    {
        public string Recipient { get; set; }
        public decimal Amount { get; set; }
    }
}