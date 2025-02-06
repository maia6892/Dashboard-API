using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.CryptocurrencyPage
{
    public class StatisticsDto
    {
        public decimal Income { get; set; }
        public int IncomePercentage { get; set; }
        public decimal Spends { get; set; }
        public int SpendsPercentage { get; set; }
        public decimal Installments { get; set; }
        public int InstallmentsPercentage { get; set; }
        public decimal Invests { get; set; }
        public int InvestsPercentage { get; set; }
    }
}