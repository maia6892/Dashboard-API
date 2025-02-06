using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.ManagementPage
{
    public class EmployeeActivity
    {
        public int Id { get; set; }
        public Employee Employee { get; set; }
        public string ActivityDescription { get; set; }
        public DateTime Time { get; set; }
        public EmployeeCompany Company { get; set; }
    }
}