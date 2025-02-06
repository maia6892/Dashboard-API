using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.BankingPage
{
    public class IncomeExpenseStatisticsDto
    {
        public string Month { get; set; }
        public string Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}