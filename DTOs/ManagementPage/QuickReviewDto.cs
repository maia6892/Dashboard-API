using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.ManagementPage
{
    public class QuickReviewDto
    {
        public int TotalEmployees { get; set; }
        public int TotalEmployeesPercentage { get; set; }
        public int TotalAttendance { get; set; }
        public int TotalAttendancePercentage { get; set; }
        public int PersonalLeave { get; set; }
        public int PersonalLeavePercentage { get; set; }
        public int Absence { get; set; } 
        public int AbsencePercentage { get; set; } 
    }
}