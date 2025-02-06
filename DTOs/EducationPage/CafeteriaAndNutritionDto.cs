using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.EducationPage
{
    public class CafeteriaAndNutritionDto
    {
        public double SchoolLunch { get; set; }
        public double LunchFromHome { get; set; }
        public double SkippingLunch { get; set; }
    }
}