using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class DailyAttendanceAndParticipationDto
    {
        public int AttendanceRate { get; set; }
        public int OnTimeArrival { get; set; }
        public int LateArrivals { get; set; }
        public int UnexcusedAbsences { get; set; }
        public int StudentsSick { get; set; }
    }
}