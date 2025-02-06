using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.ManagementPage
{
    public class LatestActivityDto
    {
        public string Name { get; set; }
        public string ActivityDetails { get; set; }
        public string Time { get; set; }
    }
}