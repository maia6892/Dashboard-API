using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.ManagementPage
{
    public class EmployeeDashboard
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Employee Employee { get; set; }
        public bool IsAttended { get; set; }
        public bool IsOnPersonalLeave { get; set; }
        public EmployeeCompany Company { get; set; }
    }
}