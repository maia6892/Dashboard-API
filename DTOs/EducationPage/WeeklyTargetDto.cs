using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class WeeklyTargetDto
    {
        public int TotalTasks { get; set; }
        public double TotalTasksPercentage { get; set; }
        public int CompletedTasks { get; set; }
        public double CompletedTasksPercentage { get; set; }
        public int InProgressTasks { get; set; }
        public double InProgressTasksPercentage { get; set; }
        public int PendingTasks { get; set; }
        public double PendingTasksPercentage { get; set; }

    }
}