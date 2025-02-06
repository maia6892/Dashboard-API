using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.EducationPage;

namespace DashboardAPI.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime AdmissionDate { get; set; }
        public ICollection<GeneralTask>? Tasks { get; set; }
        public ICollection<SchoolDashboard> SchoolDashboards { get; set; }
    }
}