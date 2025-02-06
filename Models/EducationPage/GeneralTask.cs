using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.EducationPage;

namespace DashboardAPI.Models
{
    public class GeneralTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Subject Subject { get; set; }
        public Grade? Grade { get; set; }
        public bool HighPriority { get; set; }
        public TaskProgress Progress { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}