using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.ManagementPage
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public EmployeePosition Position { get; set; }
        public EmployeeTeam Team { get; set; }
        public EmployeeCompany Company { get; set; }
        public DateTime HireDate { get; set; }
    }
}