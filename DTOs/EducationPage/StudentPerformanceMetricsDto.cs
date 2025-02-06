using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models;
using Microsoft.VisualBasic;

namespace DashboardAPI.DTOs.EducationPage
{
    public class StudentPerformanceMetricsDto
    {
        public string Subject { get; set; }
        public int NumberOfA { get; set; }
        public int NumberOfB { get; set; }
        public int NumberOfC { get; set; }
        public int NumberOfD { get; set; }
    }
}