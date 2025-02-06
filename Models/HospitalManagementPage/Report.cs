using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.HospitalManagementPage
{
    public class Report
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }
        public Hospital Hospital { get; set; }
    }
}