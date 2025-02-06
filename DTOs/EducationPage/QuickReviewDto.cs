using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class QuickReviewDto
    {
        public int TotalStudents { get; set; }
        public int TotalStudentsPercentage { get; set; }
        public decimal DailyAttendance { get; set; }
        public int DailyAttendancePercentage { get; set; }
        public int Absence { get; set; }
        public int AbsencePercentage { get; set; }
        public int LateArrival { get; set; }
        public int LateArrivalPercentage { get; set; }
    }
}