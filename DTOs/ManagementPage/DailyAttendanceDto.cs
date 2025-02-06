using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.ManagementPage
{
    public class DailyAttendanceDto
    {
        public string Team { get; set; }
        public int TotalTeamMembers { get; set; }
        public int TeamMemebersAttended { get; set; }
        public int AttendancePercentage { get; set; }
    }
}