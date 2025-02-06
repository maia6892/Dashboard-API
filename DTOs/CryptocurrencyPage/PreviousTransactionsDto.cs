using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.CryptocurrencyPage
{
    public class PreviousTransactionsDto
    {
        public int Id { get; set; }
        public string TransactionName { get; set; }
        public DateTime Date { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}