using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.EducationPage;

namespace DashboardAPI.Models;

public class SchoolDashboard
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int StudentId { get; set; }
    public Student Students { get; set; }
    public bool Attended { get; set; }
    public AbsenceStatus? AbsenceStatuses { get; set; }
    public LunchChoice? LunchChoices { get; set; }
    public ArrivalStatus? ArrivalStatuses { get; set; }
    public bool ExtraCurricular { get; set; }
    public bool Support { get; set; }
}


