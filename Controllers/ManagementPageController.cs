using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.ManagementPage;
using DashboardAPI.Models.ManagementPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ManagementPageController(DashboardDbContext context) : ControllerBase
    {
        private readonly DateTime today = DateTime.UtcNow.Date;
        private readonly int thisYear = DateTime.UtcNow.Year;
        private readonly int lastYear = DateTime.UtcNow.AddYears(-1).Year;
        private readonly int thisMonth = DateTime.UtcNow.Month;
        private readonly int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;

        [HttpGet]
        public async Task<IActionResult> QuickReview(int companyId = 1)
        {
            var thisMonthStart = new DateTime(today.Year, today.Month, 1).ToUniversalTime();
            var nextMonthStart = thisMonthStart.AddMonths(1).ToUniversalTime();
            var lastMonthStart = thisMonthStart.AddMonths(-1).ToUniversalTime();
            var lastMonthEnd = thisMonthStart.ToUniversalTime();

            var newEmployeesCount = await context.Employees
                .Where(e => e.HireDate >= thisMonthStart && e.HireDate < nextMonthStart && e.Company.Id == companyId)
                .CountAsync();

            var existingEmployeesCount = await context.Employees
                .Where(e => e.HireDate < thisMonthStart && e.Company.Id == companyId)
                .CountAsync();

            var newEmployeesLastMonthCount = await context.Employees
                .Where(e => e.HireDate >= lastMonthStart && e.HireDate < lastMonthEnd && e.Company.Id == companyId)
                .CountAsync();

            var existingEmployeesLastMonthCount = await context.Employees
                .Where(e => e.HireDate < lastMonthStart && e.Company.Id == companyId)
                .CountAsync();

            var attendance = await context.EmployeeDashboards
                .Where(e => e.Date.Month == thisMonth && e.Date.Year == thisYear && e.IsAttended && e.Company.Id == companyId)
                .CountAsync();
            var attendanceLastMonth = await context.EmployeeDashboards
                .Where(e => e.Date.Month == lastMonth && e.Date.Year == lastYear && e.IsAttended && e.Company.Id == companyId)
                .CountAsync();

            var personalLeave = await context.EmployeeDashboards
                .Where(e => e.Date.Month == thisMonth && e.Date.Year == thisYear && !e.IsAttended && e.IsOnPersonalLeave && e.Company.Id == companyId)
                .CountAsync();
            var personalLeaveLastMonth = await context.EmployeeDashboards
                .Where(e => e.Date.Month == lastMonth && e.Date.Year == lastYear && !e.IsAttended && e.IsOnPersonalLeave && e.Company.Id == companyId)
                .CountAsync();

            var absence = await context.EmployeeDashboards
                .Where(e => e.Date.Month == thisMonth && e.Date.Year == thisYear && !e.IsAttended && !e.IsOnPersonalLeave && e.Company.Id == companyId)
                .CountAsync();
            var absenceLastMonth = await context.EmployeeDashboards
                .Where(e => e.Date.Month == lastMonth && e.Date.Year == lastYear && !e.IsAttended && !e.IsOnPersonalLeave && e.Company.Id == companyId)
                .CountAsync();

            var quickReview = new QuickReviewDto()
            {
                TotalEmployees = newEmployeesCount + existingEmployeesCount,
                TotalEmployeesPercentage = Helper.GetPercentageDifference(existingEmployeesLastMonthCount + newEmployeesLastMonthCount, existingEmployeesCount + newEmployeesCount),
                TotalAttendance = attendance,
                TotalAttendancePercentage = Helper.GetPercentageDifference(attendanceLastMonth, attendance),
                PersonalLeave = personalLeave,
                PersonalLeavePercentage = Helper.GetPercentageDifference(personalLeaveLastMonth, personalLeave),
                Absence = absence,
                AbsencePercentage = Helper.GetPercentageDifference(absenceLastMonth, absence)
            };

            return Ok(quickReview);
        }


        [HttpGet]
        public async Task<IActionResult> AttendanceStatistics(int companyId = 1)
        {
            var attendance = await context.EmployeeDashboards
                .Where(e => e.Date.Month == thisMonth && e.IsAttended && e.Company.Id == companyId)
                .CountAsync();
            var attendanceLastMonth = await context.EmployeeDashboards
                .Where(e => e.Date.Month == lastMonth && e.IsAttended && e.Company.Id == companyId)
                .CountAsync();

            var personalLeave = await context.EmployeeDashboards
                .Where(e => e.Date.Month == thisMonth && !e.IsAttended && e.IsOnPersonalLeave && e.Company.Id == companyId)
                .CountAsync();
            var personalLeaveLastMonth = await context.EmployeeDashboards
                .Where(e => e.Date.Month == lastMonth && !e.IsAttended && e.IsOnPersonalLeave && e.Company.Id == companyId)
                .CountAsync();

            var daysOfWeek = Helper.GetThisWeekDays(today, includeWeekends: true).ToList();
            var result = new
            {
                TotalAttendance = attendance,
                TotalAttendancePercentage = Helper.GetPercentageDifference(attendanceLastMonth, attendance),
                TotalPersonalLeave = personalLeave,
                TotalPersonalLeavePercentage = Helper.GetPercentageDifference(personalLeaveLastMonth, personalLeave),
                Week = daysOfWeek.Select(d => new AttendanceStatisticsDto
                {
                    DayOfWeek = d.Key,
                    Attendance = context.EmployeeDashboards
                        .Where(e => e.Date.Date == d.Value.Date && e.IsAttended && e.Company.Id == companyId)
                        .Count(),
                    Leaves = context.EmployeeDashboards
                        .Where(e => e.Date.Date == d.Value.Date && !e.IsAttended && e.IsOnPersonalLeave && e.Company.Id == companyId)
                        .Count()
                })
            };
            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> UpcomingEvents(int companyId = 1)
        {
            var events = await context.ManagementEventsCalendars.Where(e => e.EventDate.Date >= today && e.EventDate.Date < today.AddDays(5) && e.Company.Id == companyId).ToListAsync();
            var week = Helper.GetWeekStartingToday(today);

            var upcomingEvents = week.Select(d => new
            {
                DayOfWeek = d.Key,
                Events = events.Where(e => e.EventDate.Date == d.Value.Date).Select(e => new UpcomingEventsDto
                {
                    Time = e.StartTime.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")),
                    Event = e.EventDescription
                })
            });

            return Ok(upcomingEvents);
        }


        [HttpGet]
        public async Task<IActionResult> DailyAttendance(int companyId = 1)
        {
            var teams = await context.EmployeeTeams.Where(t => t.Company.Id == companyId).ToListAsync();
            var dailyAttendance = teams.Select(t => new DailyAttendanceDto
            {
                Team = t.Name,
                TotalTeamMembers = context.Employees.Where(e => e.Team.Id == t.Id && e.Company.Id == companyId).Count(),
                TeamMemebersAttended = context.EmployeeDashboards.Where(e => e.Date.Date == today.Date && e.IsAttended && e.Employee.Team.Id == t.Id && e.Company.Id == companyId).Count(),
                AttendancePercentage = Helper.GetPercentage(context.EmployeeDashboards.Where(e => e.Date.Date == today.Date && e.IsAttended && e.Employee.Team.Id == t.Id && e.Company.Id == companyId).Count(), context.Employees.Where(e => e.Team.Id == t.Id && e.Company.Id == companyId).Count())
            });
            return Ok(dailyAttendance);
        }


        [HttpGet]
        public async Task<IActionResult> EmployeesDataPosition(int companyId = 1)
        {
            var position = await context.EmployeePositions.Where(p => p.Company.Id == companyId).Select(p => p.Name).ToListAsync();
            return Ok(position);
        }

        [HttpGet]
        public IActionResult EmployeesDataStatus(int companyId = 1)
        {
            var status = new List<string> { "Active", "Leave" };
            return Ok(status);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeesData(string? name, string? position, string? status, int companyId = 1)
        {
            var query = context.Employees
                .Where(e => e.Company.Id == companyId)
                .Include(e => e.Position)
                .Include(e => e.Team)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = FilterEmployeesByStatus(query, status, companyId, position, name);
            }

            var employees = await query.ToListAsync();

            var result = employees.Select(e => new EmployeesDataDto
            {
                EmployeeName = e.Name,
                Email = e.Email,
                Position = e.Position.Name,
                Team = e.Team.Name,
                AttendanceRate = CalculateAttendanceRate(e.Id, companyId),
                Status = DetermineStatus(e.Id, companyId)
            }).ToList();

            return Ok(result);
        }

        

        [HttpGet]
        public async Task<IActionResult> ComingEvents(DateTime date, int companyId = 1)
        {
            var events = await context.ManagementEventsCalendars.Where(e => e.EventDate.Date == date && e.Company.Id == companyId).OrderByDescending(e => e.StartTime).ToListAsync();
            var result = events.Select(e => new ComingEventsDto
            {
                EventDescription = e.EventDescription,
                EventDate = e.EventDate.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US")),
                StartTime = e.StartTime.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")),
                EndTime = e.EndTime.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US"))
            });
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeAttendanceRate(int year, int companyId = 1)
        {
            var attended = await context.EmployeeDashboards.Where(e => e.Company.Id == companyId && e.Date.Year == year && e.IsAttended).CountAsync();
            var totalemployees = await context.EmployeeDashboards.Where(e => e.Company.Id == companyId && e.Date.Year == year).CountAsync();

            return Ok((double)attended * 100 / totalemployees);
        }


        [HttpGet]
        public async Task<IActionResult> LatestActivity(int companyId = 1)
        {
            var latestActivity = await context.EmployeeActivities
            .Where(a => a.Company.Id == companyId && a.Time.Date == today)
            .Include(a => a.Employee)
            .OrderByDescending(a => a.Time)
            .ToListAsync();

            var result = latestActivity.Select(a => new LatestActivityDto
            {
                Name = a.Employee.Name,
                Time = a.Time.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")),
                ActivityDetails = a.ActivityDescription
            });

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> HiringAnalytics(int year, int companyId = 1)
        {
            var findYear = new DateTime(year, 1, 1).ToUniversalTime();

            var monthsOfYear = Helper.GetMonthsOfYear(findYear);

            var hiringAnalytics = new
            {
                TotalApplicants = await context.Applicants.Where(a => a.Company.Id == companyId && a.Date.Year == year).SumAsync(a => a.TotalApplicants),
                TotalApplicantsPercentage = Helper.GetPercentageDifference( await context.Applicants.Where(a => a.Company.Id == companyId && a.Date.Year == lastYear).SumAsync(a => a.TotalApplicants), await context.Applicants.Where(a => a.Company.Id == companyId && a.Date.Year == thisYear).SumAsync(a => a.TotalApplicants)),
                Analytics = monthsOfYear.Select(async m => new HiringAnalyticsDto
                {
                    Month = m.Key,
                    Count = await context.Applicants.Where(a => a.Company.Id == companyId && a.Date.Year == year && a.Date.Month == m.Value.Month).Select(a => a.TotalApplicants).SumAsync()
                })
            };
            return Ok(hiringAnalytics);
        }

        // ******************************   Helper Functions    *************************
        
        private int CalculateAttendanceRate(int employeeId, int companyId)
        {
            var lastMonth = DateTime.Today.AddMonths(-1).Month;

            var attendedDays = context.EmployeeDashboards
                .Count(ed => ed.Date.Month == lastMonth && ed.IsAttended && ed.Employee.Id == employeeId && ed.Company.Id == companyId);

            var totalDays = context.EmployeeDashboards
                .Count(ed => ed.Date.Month == lastMonth && ed.Employee.Id == employeeId && ed.Company.Id == companyId);

            return Helper.GetPercentage(attendedDays, totalDays);
        }

        private string DetermineStatus(int employeeId, int companyId)
        {
            var isActive = context.EmployeeDashboards
                .Any(ed => ed.Date.Date == today && ed.Employee.Id == employeeId && ed.Company.Id == companyId &&
                    (ed.IsAttended || (!ed.IsAttended && !ed.IsOnPersonalLeave)));

            return isActive ? "Active" : "Leave";
        }

        private IQueryable<Employee> FilterEmployeesByStatus(IQueryable<Employee> query, string status, int companyId, string? position, string? name)
        {
            if (status == "Active")
            {
                query = query.Where(e =>
                    context.EmployeeDashboards.Any(ed =>
                        ed.Employee.Id == e.Id &&
                        ed.Date.Date == today &&
                        (ed.IsAttended || (!ed.IsAttended && !ed.IsOnPersonalLeave)) &&
                        ed.Company.Id == companyId));
            }
            else
            {
                query = query.Where(e =>
                    context.EmployeeDashboards.Any(ed =>
                        ed.Employee.Id == e.Id &&
                        ed.Date.Date == today &&
                        !ed.IsAttended &&
                        ed.IsOnPersonalLeave &&
                        ed.Company.Id == companyId));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => EF.Functions.Like(e.Name, $"%{name}%"));
            }

            if (!string.IsNullOrEmpty(position))
            {
                query = query.Where(e => e.Position != null && e.Position.Name == position);
            }

            return query;
        }
    }
}
