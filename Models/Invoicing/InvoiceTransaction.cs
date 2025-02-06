using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.Invoicing
{
    public class InvoiceTransaction
    {
        public int Id { get; set; }
        public InvoicingUser User { get; set; }
        public string Name { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public SpendingList SpendingList { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}