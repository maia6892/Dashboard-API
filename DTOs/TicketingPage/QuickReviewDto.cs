using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.TicketingPage
{
    public class QuickReviewDto
    {
        public decimal Sales { get; set; }
        public int Customer { get; set; }
        public int  Growth { get; set; }
    }
}