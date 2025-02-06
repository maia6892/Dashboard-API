using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage
{
    public class QuickReviewDto
    {
        public decimal Revenue { get; set; }
        public decimal RevenuePercentage { get; set; }
        public decimal Costs { get; set; }
        public decimal CostsPercentage { get; set; }
        public decimal Profits { get; set; }
        public decimal ProfitsPercentage { get; set; }
        public decimal Shipments { get; set; }
        public decimal ShipmentsPercentage { get; set; }
    }
}