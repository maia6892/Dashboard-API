using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.BankingPage
{
    public class BankTransaction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BankUser User { get; set; }
        public BankCard Card { get; set; }
        public BankTransactionCategory Category { get; set; }
        public bool IsIncome { get; set; }
        public decimal Amount { get; set; }
        public BankTransactionStatus Status { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}