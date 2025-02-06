using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class CustomerReviewsDto
    {
        public string Month { get; set; }
        public string Date { get; set; }
        public int Positive { get; set; }
        public int Negative { get; set; }
    }
}