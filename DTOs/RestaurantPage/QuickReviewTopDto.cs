using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class QuickReviewTopDto
    {
        public decimal Income { get; set; }
        public int IncomePercentage { get; set; }
        public decimal Expense { get; set; }
        public int ExpensePercentage { get; set; }
        public decimal TotalIncome { get; set; }
    }
}