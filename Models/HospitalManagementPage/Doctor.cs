using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.HospitalManagementPage
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public Field Field { get; set; }
        public DoctorStatus Status { get; set; }
        public Hospital Hospital { get; set; }
    }
}