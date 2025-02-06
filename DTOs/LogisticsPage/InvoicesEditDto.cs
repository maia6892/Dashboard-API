using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class InvoicesEditDto
    {
        public string Customer { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Amount { get; set; }
    }
}