using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.LogisticsPage
{
    public class CostService
    {
        public int Id { get; set; }
        public int CostId { get; set; }
        public decimal CostAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public Cost Costs { get; set; }
    }
}