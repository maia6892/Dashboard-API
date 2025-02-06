using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.HospitalManagementPage
{
    public class PatientAppointmentDto
    {
        public string PatientName { get; set; }
        public int Age { get; set; }
        public string Time { get; set; }
        public string DoctorName { get; set; }
        public string Treatment { get; set; }
        public string Status { get; set; }
        public string MoreInfo { get; set; }
        public DateTime Date { get; set; }
    }
}