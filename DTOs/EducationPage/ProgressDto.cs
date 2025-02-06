using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class ProgressDto
    {
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
    }
}