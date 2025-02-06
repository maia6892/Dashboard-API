using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.HospitalManagementPage
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public DateTime BirthDate { get; set; }
        public Hospital Hospital { get; set; }
    }
}