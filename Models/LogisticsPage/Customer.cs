using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.LogisticsPage
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}