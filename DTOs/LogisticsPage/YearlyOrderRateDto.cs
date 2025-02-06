using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage;

public class YearlyOrderRateDto
{
    public string Month { get; set; }
    public int MonthOrderRate { get; set; }
    public List<WeeklyOrderRateDto> WeeklyRates { get; set; }
}

public class WeeklyOrderRateDto
{
    public string WeekStart { get; set; }
    public int WeekOrderRate { get; set; }
}
