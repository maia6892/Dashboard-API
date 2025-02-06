using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.Services.Interfaces;

namespace DashboardAPI.Services
{
    public class BalanceCalculator(DashboardDbContext context) : IBalanceCalculator
    {
        public decimal CalculateBalance(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var incomeTypes = new[] { "Cashback", "Transfer In" };
            var expenseTypes = new[] { "Transfer Out" };

            var query = context.InvoiceTransactions
                .Where(t => t.User.Id == userId && t.Status.Status == "Completed");

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }
            
            var totalIncome = query
                .Where(t => incomeTypes.Contains(t.Type.Type))
                .Sum(t => t.Amount);

            var totalExpense = query
                .Where(t => expenseTypes.Contains(t.Type.Type))
                .Sum(t => t.Amount);

            return totalIncome - totalExpense;
        }
    }
}