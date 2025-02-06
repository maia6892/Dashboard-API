using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.ManagementPage
{
    public class EmployeeCompany
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public ICollection<EmployeePosition> EmployeePositions { get; set; }
        public ICollection<EmployeeTeam> EmployeeTeams { get; set; }
        public ICollection<EmployeeDashboard> EmployeeDashboards { get; set; }
        public ICollection<EmployeeActivity> EmployeeActivities { get; set; }
        public ICollection<ManagementEventsCalendar> ManagementEventsCalendars { get; set; }
        public ICollection<Applicant> Applicants { get; set; }
    }
}