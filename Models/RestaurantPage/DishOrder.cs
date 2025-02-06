using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.RestaurantPage
{
    public class DishOrder
    {
        public int Id { get; set; }
        public RestaurantVisitor Visitor { get; set; }
        public Restaurant Restaurant { get; set; }
        public DishOrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public int? Stars { get; set; }
        public ICollection<DishOrderDish> DishOrderDishes { get; set; }
    }
}