using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.RestaurantPage
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PhotoUrl { get; set; }
        public string Category { get; set; }
        public decimal CookingCost { get; set; }
        public ICollection<DishOrderDish> DishOrderDishes { get; set; }
    }
}