using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.LogisticsPage
{
    public class FeeService
    {
        public int Id { get; set; }
        public int FeeId { get; set; }
        public decimal FeePercentage { get; set; }
        public DateTime PaymentDate { get; set; }
        public Fee Fees { get; set; }
    }
}