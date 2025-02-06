using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.HospitalManagementPage
{
    public class CalendarDto
    {
        public string Appointment { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}