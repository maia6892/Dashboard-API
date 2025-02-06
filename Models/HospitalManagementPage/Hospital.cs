using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.HospitalManagementPage
{
    public class Hospital
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Doctor> Doctors { get; set; }
        public ICollection<Patient> Patients { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<HospitalActivity> HospitalActivities { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}