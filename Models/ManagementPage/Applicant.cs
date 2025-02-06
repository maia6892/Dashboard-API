using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.ManagementPage
{
    public class Applicant
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalApplicants { get; set; }
        public EmployeeCompany Company { get; set; }
    }
}