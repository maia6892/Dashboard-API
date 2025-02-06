using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.HospitalManagementPage;
using DashboardAPI.Models.HospitalManagementPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HospitalManagementController(DashboardDbContext context) : ControllerBase
    {
        private readonly DateTime today = DateTime.UtcNow.Date;
        private readonly int thisYear = DateTime.UtcNow.Year;
        private readonly int lastYear = DateTime.UtcNow.AddYears(-1).Year;
        private readonly int thisMonth = DateTime.UtcNow.Month;
        private readonly int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;

        [HttpGet]
        public async Task<IActionResult> QuickView(int hospitalId = 1)
        {
            var patientsThisMonth = context.Appointments.Where(a => a.Date.Month == thisMonth && a.Hospital.Id == hospitalId).Select(a => a.Patient).Distinct().Count();
            var patientsLastMonth = context.Appointments.Where(a => a.Date.Month == lastMonth && a.Hospital.Id == hospitalId).Select(a => a.Patient).Distinct().Count();

            var appointmentsThisMonth = context.Appointments.Where(a => a.Date.Month == thisMonth && a.Hospital.Id == hospitalId).Count();
            var appointmentsLastMonth = context.Appointments.Where(a => a.Date.Month == lastMonth && a.Hospital.Id == hospitalId).Count();

            var quickView = new
            {
                Visitors = patientsThisMonth * 2,
                VisitorsPercentage = Helper.GetPercentageDifference(patientsLastMonth * 2, patientsThisMonth * 2),
                Patients = patientsThisMonth,
                PatientsPercentage = Helper.GetPercentageDifference(patientsLastMonth, patientsThisMonth),
                Appointments = appointmentsThisMonth,
                AppointmentsPercentage = Helper.GetPercentageDifference(appointmentsLastMonth, appointmentsThisMonth),
                Bedroom = patientsThisMonth / 10,
                BedroomPercentage = Helper.GetPercentageDifference(patientsLastMonth / 10, patientsThisMonth / 10),
            };
            return Ok(quickView);
        }

        [HttpGet]
        public async Task<IActionResult> PatientOverview(string timestamp, int hospitalId = 1)
        {
            var hoursOfDay = Helper.GetHoursOfToday(today);
            var week = Helper.GetThisWeekDays(today, includeWeekends: true);
            var months = Helper.GetMonthsOfYear(new DateTime(thisYear, 1, 1));
            var years = Helper.GetYearsInDatabase(context.Appointments.Where(a => a.Hospital.Id == hospitalId).Select(a => a.Date).ToList());

            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            var lastMonthDateTime = today.AddMonths(-1);

            var appointment = context.Appointments
            .Where(a => a.Hospital.Id == hospitalId)
            .Include(a => a.Patient)
            .ToList();

            var childrenThisMonth = appointment.Count(p => p.Date.Month == thisMonth && Helper.CalculateAge(p.Patient.BirthDate) < 18);
            var adultsThisMonth = appointment.Count(p => p.Date.Month == thisMonth && Helper.CalculateAge(p.Patient.BirthDate) >= 18 && Helper.CalculateAge(p.Patient.BirthDate) < 65);
            var elderlyThisMonth = appointment.Count(p => p.Date.Month == thisMonth && Helper.CalculateAge(p.Patient.BirthDate) >= 65);

            var childrenLastMonth = appointment.Count(p => p.Date.Month == lastMonth && Helper.CalculateAge(p.Patient.BirthDate) < 18);
            var adultsLastMonth = appointment.Count(p => p.Date.Month == lastMonth && Helper.CalculateAge(p.Patient.BirthDate) >= 18 && Helper.CalculateAge(p.Patient.BirthDate) < 65);
            var elderlyLastMonth = appointment.Count(p => p.Date.Month == lastMonth && Helper.CalculateAge(p.Patient.BirthDate) >= 65);

            var patientsByAge = new
            {
                Children = childrenThisMonth,
                ChildrenPercentage = Helper.GetPercentageDifference(childrenLastMonth, childrenThisMonth),
                Adults = adultsThisMonth,
                AdultsPercentage = Helper.GetPercentageDifference(adultsLastMonth, adultsThisMonth),
                Elderly = elderlyThisMonth,
                ElderlyPercentage = Helper.GetPercentageDifference(elderlyLastMonth, elderlyThisMonth)
            };

            var patientOverview = new List<PatientOverviewDto>();

            switch (timestamp.ToLower())
            {
                case "today":
                    patientOverview = hoursOfDay.Select(h => new PatientOverviewDto()
                    {
                        Date = h.Key,
                        Children = appointment.Count(p => p.Date.Hour == h.Value.Hour && p.Date.Date == today && Helper.CalculateAge(p.Patient.BirthDate) < 18),
                        Adults = appointment.Count(p => p.Date.Hour == h.Value.Hour && p.Date.Date == today && Helper.CalculateAge(p.Patient.BirthDate) >= 18 && Helper.CalculateAge(p.Patient.BirthDate) < 65),
                        Elderly = appointment.Count(p => p.Date.Hour == h.Value.Hour && p.Date.Date == today && Helper.CalculateAge(p.Patient.BirthDate) >= 65)
                    }).ToList();
                    break;
                case "weekly":
                    patientOverview = week.Select(h => new PatientOverviewDto()
                    {
                        Date = h.Key,
                        FullDate = h.Value.ToString("d MMM yyyy", new System.Globalization.CultureInfo("en-US")),
                        Children = appointment.Count(p => p.Date.Date == h.Value.Date && Helper.CalculateAge(p.Patient.BirthDate, h.Value) < 18),
                        Adults = appointment.Count(p => p.Date.Date == h.Value.Date && Helper.CalculateAge(p.Patient.BirthDate, h.Value) >= 18 && Helper.CalculateAge(p.Patient.BirthDate, h.Value) < 65),
                        Elderly = appointment.Count(p => p.Date.Date == h.Value.Date && Helper.CalculateAge(p.Patient.BirthDate, h.Value) >= 65)
                    }).ToList();
                    break;
                case "monthly":
                    patientOverview = months.Select(h => new PatientOverviewDto()
                    {
                        Date = h.Key,
                        Children = appointment.Count(p => p.Date.Month == h.Value.Month && Helper.CalculateAge(p.Patient.BirthDate, h.Value) < 18),
                        Adults = appointment.Count(p => p.Date.Month == h.Value.Month && Helper.CalculateAge(p.Patient.BirthDate, h.Value) >= 18 && Helper.CalculateAge(p.Patient.BirthDate, h.Value) < 65),
                        Elderly = appointment.Count(p => p.Date.Month == h.Value.Month && Helper.CalculateAge(p.Patient.BirthDate, h.Value) >= 65)
                    }).ToList();
                    break;
                case "yearly":
                    patientOverview = years.Select(h => new PatientOverviewDto()
                    {
                        Date = h.Key,
                        Children = appointment.Count(p => p.Date.Year == h.Value.StartDate.Year && Helper.CalculateAge(p.Patient.BirthDate, h.Value.EndDate) < 18),
                        Adults = appointment.Count(p => p.Date.Year == h.Value.StartDate.Year && Helper.CalculateAge(p.Patient.BirthDate, h.Value.EndDate) >= 18 && Helper.CalculateAge(p.Patient.BirthDate, h.Value.EndDate) < 65),
                        Elderly = appointment.Count(p => p.Date.Year == h.Value.StartDate.Year && Helper.CalculateAge(p.Patient.BirthDate, h.Value.EndDate) >= 65)
                    }).ToList();
                    break;
                default:
                    return BadRequest();
            }

            return Ok(new
            {
                PatientsByAge = patientsByAge,
                patientsOvewview = patientOverview
            });
        }

        [HttpGet]
        public async Task<IActionResult> Calendar(int patientId = 1, int hospitalId = 1)
        {
            var months = Helper.GetMonthsOfYear(new DateTime(DateTime.Now.Year, 1, 1));

            var patientAppointments = await context.Appointments
                .Where(a => a.Patient.Id == patientId && a.Hospital.Id == hospitalId)
                .Include(a => a.Treatment)
                .Include(a => a.Doctor)
                    .ThenInclude(a => a.Field)
                .ToListAsync();

            var calendar = months.Select(m => new
            {
                Month = m.Key,
                Days = Helper.GetAllDaysOfMonth(m.Value.Year, m.Value.Month)
                    .Select(day => new
                    {
                        Day = day.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        Appointments = patientAppointments
                            .Where(a => a.Date.Date == day.Date)
                            .Select(a => new CalendarDto
                            {
                                Appointment = a.Treatment.Name,
                                StartTime = a.Date.ToString("hh:00 tt", System.Globalization.CultureInfo.InvariantCulture),
                                EndTime = a.Date.AddHours(1).ToString("hh:00 tt", System.Globalization.CultureInfo.InvariantCulture),
                            })
                            .ToList()
                    }).ToList()
            }).ToList();

            return Ok(calendar);
        }

        [HttpGet]
        public async Task<IActionResult> DoctorsSchedule(int hospitalId = 1)
        {
            var doctors = await context.Doctors
                .Where(d => d.Hospital.Id == hospitalId)
                .Include(d => d.Status)
                .Include(d => d.Field)
                .ToListAsync();
            var schedule = doctors.Select(d => new DoctorsScheduleDto
            {
                Name = d.Name,
                Fild = d.Field.Name,
                ProfilePhoto = d.Photo,
                Status = d.Status.Name,
            });
            var quickView = new
            {
                Available = doctors.Count(d => d.Status.Name == "Available"),
                NotAvailable = doctors.Count(d => d.Status.Name == "Unavailable"),
                Leave = doctors.Count(d => d.Status.Name == "Leave"),
            };

            return Ok(new
            {
                QuickView = quickView,
                Schedule = schedule
            });
        }

        [HttpGet]
        public async Task<IActionResult> Reports(int hospitalId = 1)
        {
            var reports = await context.Reports
                .Where(r => r.Hospital.Id == hospitalId)
                .Take(20)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            var result = reports.Select(r => new
            {
                Name = r.Description,
                Date = r.Date.ToString("hh:00 tt", System.Globalization.CultureInfo.InvariantCulture),
            });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> ReportById(int reportId, int hospitalId = 1)
        {
            var findReport = await context.Reports
                .Where(r => r.Hospital.Id == hospitalId && r.Id == reportId)
                .FirstOrDefaultAsync();

            return Ok(findReport);
        }

        [HttpGet]
        public async Task<IActionResult> RecentActivity(int hospitalId = 1)
        {
            var activity = await context.HospitalActivities
                .Where(a => a.Hospital.Id == hospitalId && a.Date > DateTime.UtcNow.AddDays(-7) && a.Date.Date <= today)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var result = activity.Select(a => new
            {
                Name = a.Patient.Name,
                PhotoUrl = a.Patient.Photo,
                Activity = a.Description,
                Date = (DateTime.UtcNow - a.Date).TotalHours >= 24
                    ? a.Date.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)
                    : (DateTime.UtcNow - a.Date).TotalHours >= 1
                        ? $"{(int)(DateTime.UtcNow - a.Date).TotalHours} hours ago"
                        : $"{(int)(DateTime.UtcNow - a.Date).TotalMinutes} minutes ago"
            });

            return Ok(result);
        }

        [HttpGet]
        public IActionResult Revenue(int year, int hospitalId = 1)
        {
            var months = Helper.GetMonthsOfYear(new DateTime(year, 1, 1));

            var incomeThisMonth = context.Appointments.Where(a => a.Date.Month == thisMonth && a.Date.Year == thisYear && a.Hospital.Id == hospitalId).Sum(a => a.Treatment.Price);
            var incomeLastMonth = context.Appointments.Where(a => a.Date.Month == lastMonth && a.Date.Year == thisYear && a.Hospital.Id == hospitalId).Sum(a => a.Treatment.Price);

            var expenseThisMonth = context.Reports.Where(a => a.Date.Month == thisMonth && a.Date.Year == thisYear && a.Hospital.Id == hospitalId).Sum(a => a.Cost);
            var expenseLastMonth = context.Reports.Where(a => a.Date.Month == lastMonth && a.Date.Year == thisYear && a.Hospital.Id == hospitalId).Sum(a => a.Cost);

            var result = new
            {
                RevenueTthisMonth = incomeThisMonth - expenseThisMonth,
                RevenuePercentage = Helper.GetPercentageDifference(incomeLastMonth - expenseLastMonth, incomeThisMonth - expenseThisMonth),
                Income = incomeThisMonth,
                IncomePercentage = Helper.GetPercentageDifference(incomeLastMonth, incomeThisMonth),
                Expense = expenseThisMonth,
                ExpensePercentage = Helper.GetPercentageDifference(expenseLastMonth, expenseThisMonth),
                Revenue = months.Select(m => new
                {
                    Month = m.Key,
                    Income = context.Appointments.Where(a => a.Date.Month == m.Value.Month && a.Date.Year == year && a.Hospital.Id == hospitalId).Sum(a => a.Treatment.Price),
                    Expense = context.Reports.Where(a => a.Date.Month == m.Value.Month && a.Date.Year == year && a.Hospital.Id == hospitalId).Sum(a => a.Cost),
                }).ToList()
            };

            return Ok(result);
        }

        // [HttpGet]
        // public async Task<IActionResult> PatientOverviewByDepartments(string week, string month, int year, int hospitalId = 1)
        // {
        //     var months = Helper.GetMonthsOfYear(new DateTime(thisYear, 1, 1));
        //     var fields = await context.Fields.ToListAsync();



        //     return Ok();
        // }

        [HttpGet]
        public async Task<IActionResult> PatientOverviewByDepartments(string week, string month, int year, int hospitalId = 1)
        {
            var months = Helper.GetMonthsOfYear(new DateTime(year, 1, 1));

            var selectedMonth = months.FirstOrDefault(m => m.Key.Equals(month, StringComparison.OrdinalIgnoreCase)).Value;
            if (selectedMonth == default)
                return BadRequest("Invalid month provided.");

            var startOfWeek = Helper.GetStartOfWeek(selectedMonth, week).ToUniversalTime();
            var endOfWeek = startOfWeek.AddDays(6).ToUniversalTime();

            var appointments = await context.Appointments
                .Where(a => a.Hospital.Id == hospitalId &&
                            a.Date.ToUniversalTime().Date >= startOfWeek.Date &&
                            a.Date.ToUniversalTime().Date <= endOfWeek.Date)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Field)
                .ToListAsync();

            var groupedData = appointments
                .GroupBy(a => new { a.Date.DayOfWeek, a.Doctor.Field.Name })
                .Select(g => new
                {
                    Day = g.Key.DayOfWeek.ToString(),
                    Department = g.Key.Name,
                    Patients = g.Count()
                })
                .ToList();

            var weekDaysOrder = new List<string>
                { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            var departments = groupedData
                .GroupBy(g => g.Department)
                .Select(d => new
                {
                    Department = d.Key,
                    Data = d.OrderBy(x => weekDaysOrder.IndexOf(x.Day)).ToList()
                })
                .ToList();

            var result = new
            {
                Week = week,
                Month = month,
                Year = year,
                Departments = departments
            };

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> PatientAppointmentStatuses()
        {
            var res = await context.AppointmentStatuses.Select(a => a.Name).ToListAsync();
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> PatientAppointment(string? searchWord, DateTime? date, string? status, int hospitalId = 1)
        {
            date ??= today;

            var query = context.Appointments
                .Where(a => a.Hospital.Id == hospitalId && a.Date.Date == date.Value.Date)
                .Include(a => a.Patient)
                .Include(a => a.Status)
                .Include(a => a.Treatment)
                .Include(a => a.Doctor)
                    .ThenInclude(a => a.Field)
                .OrderByDescending(a => a.Date)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchWord))
            {
                query = query.Where(e => e.Patient.Name.Contains(searchWord) || e.Doctor.Name.Contains(searchWord) || e.Treatment.Name.Contains(searchWord));
            }

            if (date.HasValue)
            {
                query = query.Where(e => e.Date.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(e => e.Status != null && e.Status.Name == status);
            }

            var result = query.Select(a => new PatientAppointmentDto
            {
                PatientName = a.Patient.Name,
                Age = Helper.CalculateAge(a.Patient.BirthDate, DateTime.UtcNow),
                Time = a.Date.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")),
                DoctorName = a.Doctor.Name,
                Treatment = a.Treatment.Name,
                Status = a.Status.Name,
                Date = a.Date
            });
            return Ok(result);
        }
    }
}