using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.HospitalManagementPage
{
    public class PatientOverviewDto
    {
        public string Date { get; set; }
        public string? FullDate { get; set; }
        public int Children { get; set; }
        public int Adults { get; set; }
        public int Elderly { get; set; }
    }
}