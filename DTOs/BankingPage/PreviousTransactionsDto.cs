using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.BankingPage
{
    public class PreviousTransactionsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public bool IsIncome { get; set; }
    }
}