using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.BankingPage
{
    public class SendPaymentDto
    {
        public string Recipient { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }
}