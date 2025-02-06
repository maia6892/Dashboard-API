using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage;

public class OnTimeArrivalDto
{
    public int OnTime { get; set; }
    public int OntimeDifference { get; set; }
    public List<OnTimeArrivalWeekDto> Week { get; set; }
}
public class OnTimeArrivalWeekDto
{
    public string DayOfWeek { get; set; }
    public int OnTimeAttendance { get; set; }
}
