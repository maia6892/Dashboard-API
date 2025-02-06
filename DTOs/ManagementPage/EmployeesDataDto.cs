using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.ManagementPage
{
    public class EmployeesDataDto
    {
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public int AttendanceRate { get; set; }
        public string Status { get; set; }
    }
}