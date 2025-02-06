using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class SubjectsReviewDto
    {
        public string Subject { get; set; }
        public int CompletedTasksPercentage { get; set; }
    }
}