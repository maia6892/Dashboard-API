using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class InvoicesDto
    {
        public int InvoiceId { get; set; }
        public string Customer { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }

    }
}