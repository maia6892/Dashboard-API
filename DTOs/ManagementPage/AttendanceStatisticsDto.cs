using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.ManagementPage
{
    public class AttendanceStatisticsDto
    {
        public string DayOfWeek { get; set; }
        public int Leaves { get; set; }
        public int Attendance { get; set; }
    }
}