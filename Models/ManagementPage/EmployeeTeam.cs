using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.ManagementPage
{
    public class EmployeeTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public EmployeeCompany Company { get; set; }
    }
}