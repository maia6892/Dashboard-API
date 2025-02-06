using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.RestaurantPage
{
    public class TopSellingProductsDto
    {
        public string Name { get; set; }
        public int Stars { get; set; }
        public string Image { get; set; }
    }
}