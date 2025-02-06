using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.LogisticsPage;
public class CarryingCostsDto
{
    public decimal Costs { get; set; }
    public decimal CostsPercentage { get; set; }
    public List<CarryingCostsWeekDto> WeeklyCosts { get; set; }
}

public class CarryingCostsWeekDto
{
    public string DayOfWeek { get; set; }
    public decimal Cost { get; set; }
}
