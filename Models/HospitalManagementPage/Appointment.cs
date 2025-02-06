using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.HospitalManagementPage
{
    public class Appointment
    {
        public int Id { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public AppointmentStatus Status { get; set; }
        public Treatment Treatment { get; set; }
        public DateTime Date { get; set; }
        public Hospital Hospital { get; set; }
    }
}