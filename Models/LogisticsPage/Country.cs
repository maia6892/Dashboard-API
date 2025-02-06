using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.LogisticsPage
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SideOfWorld SideOfWorld { get; set; }
    }
}