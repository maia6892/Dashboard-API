using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.BankingPage
{
    public class BankTransactionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Limit { get; set; }
    }
}