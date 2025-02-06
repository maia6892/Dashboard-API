using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.RestaurantPage
{
    public class DishOrderDish
    {
        public int Id { get; set; }
        public int DishOrderId { get; set; }
        public DishOrder DishOrder { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int Quantity { get; set; }
    }
}