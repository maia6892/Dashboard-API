using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class ActivityLogsDto
    {
        public string DayOfWeek { get; set; }
        public int DailyAttendance { get; set; }
        public int ExtraCurricular { get; set; }
        public int Support { get; set; }
        
    }
}