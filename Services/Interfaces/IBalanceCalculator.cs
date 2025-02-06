using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Services.Interfaces
{
    public interface IBalanceCalculator
    {
        decimal CalculateBalance(int userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}