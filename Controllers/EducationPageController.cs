using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.EducationPage;
using DashboardAPI.Models;
using DashboardAPI.Models.EducationPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class EducationPageController(DashboardDbContext context) : ControllerBase
{
    private static DateTime today = DateTime.UtcNow.Date;
    private static DateTime yesterday = today.AddDays(-1);
    private static int thisMonth = DateTime.UtcNow.Date.Month;
    private static int lastMonth = thisMonth - 1;

    [HttpGet]
    public async Task<IActionResult> QuickReview()
    {
        var totalStudents = await context.Students.CountAsync();
        var studentsThisMonth = await context.Students.Where(s => s.AdmissionDate.Month == thisMonth).CountAsync();
        var studentsLastMonth = await context.Students.Where(s => s.AdmissionDate.Month == lastMonth).CountAsync();

        var dailyAttendance = await context.SchoolDashboards
                    .Where(s => s.Date.Date == today && s.Attended)
                    .CountAsync();
        var absence = await context.SchoolDashboards
                .Where(s => s.Attended == false && s.Date.Date == today)
                .CountAsync();
        var lateArrival = await context.SchoolDashboards
                .Where(s => s.ArrivalStatuses.Status == "Late" && s.Date.Date == today)
                .CountAsync();

        var dailyAttendanceYesterday = await context.SchoolDashboards
                    .Where(s => s.Date.Date == yesterday && s.Attended)
                    .CountAsync();

        var absencePercentageToday = await context.SchoolDashboards
                .Where(s => s.Attended == false && s.Date.Date == today)
                .CountAsync();
        var absencePercentageYesterday = await context.SchoolDashboards
                .Where(s => s.Attended == false && s.Date.Date == yesterday)
                .CountAsync();

        var lateArrivalPercentageToday = await context.SchoolDashboards
                .Where(s => s.ArrivalStatuses.Status == "Late" && s.Date.Date == today)
                .CountAsync();
        var lateArrivalPercentageYesterday = await context.SchoolDashboards
                .Where(s => s.ArrivalStatuses.Status == "Late" && s.Date.Date == yesterday)
                .CountAsync();

        var quickReview = new QuickReviewDto
        {
            TotalStudents = totalStudents,
            DailyAttendance = Helper.GetPercentage(dailyAttendance, totalStudents),
            Absence = absence,
            LateArrival = lateArrival,
            TotalStudentsPercentage = Convert.ToInt32(Helper.GetPercentageDifference(studentsLastMonth, studentsThisMonth)),
            DailyAttendancePercentage = Convert.ToInt32(Helper.GetPercentageDifference(dailyAttendanceYesterday, dailyAttendance)),
            AbsencePercentage = Convert.ToInt32(Helper.GetPercentageDifference(absencePercentageYesterday, absencePercentageToday)),
            LateArrivalPercentage = Convert.ToInt32(Helper.GetPercentageDifference(lateArrivalPercentageYesterday, lateArrivalPercentageToday))
        };

        return Ok(quickReview);
    }
    
    [HttpGet]
    public async Task<IActionResult> WeeklyTarget()
    {
        var weeklyTarget = new WeeklyTargetDto()
        {
            TotalTasks = await context.Tasks.CountAsync(),
            TotalTasksPercentage = await context.Tasks.CountAsync(),
            CompletedTasks = await context.Tasks.Where(t => t.Progress.Progress == "Completed").CountAsync(),
            CompletedTasksPercentage = context.Tasks.Where(t => t.Progress.Progress == "Completed").Count() * 100 / context.Tasks.Count(),
            InProgressTasks = await context.Tasks.Where(t => t.Progress.Progress == "In Progress").CountAsync(),
            InProgressTasksPercentage = context.Tasks.Where(t => t.Progress.Progress == "In Progress").Count() * 100 / context.Tasks.Count(),
            PendingTasks = await context.Tasks.Where(t => t.Progress.Progress == "Pending").CountAsync(),
            PendingTasksPercentage = context.Tasks.Where(t => t.Progress.Progress == "Pending").Count() * 100 / context.Tasks.Count()
        };

        return Ok(weeklyTarget);
    }

    [HttpGet]
    public async Task<IActionResult> SubjectsProgress()
    {
        var subjects = await context.Tasks
            .Include(t => t.Progress)
            .Include(t => t.Subject)
            .Take(4)
            .ToListAsync();

        var subjectsProgress = subjects.Select(s => new SubjectsReviewDto
        {
            Subject = s.Subject.Name,
            CompletedTasksPercentage = context.Tasks.Where(t => t.Subject.Name == s.Subject.Name && t.Progress.Progress == "Completed").Count() * 100 / context.Tasks.Where(t => t.Subject.Name == s.Subject.Name).Count(),

        });
        return Ok(subjectsProgress);
    }

    [HttpGet]
    public async Task<IActionResult> Progress()
    {
        var progress = new ProgressDto()
        {
            CompletedTasks = await context.Tasks.Where(t => t.Progress.Progress == "Completed").CountAsync(),
            InProgressTasks = await context.Tasks.Where(t => t.Progress.Progress == "In Progress").CountAsync()
        };
        return Ok(progress);
    }

    [HttpGet]
    public async Task<IActionResult> TaskLists()
    {
        var tasks = await context.Tasks
            .Include(t => t.Progress)
            .Include(t => t.Subject)
            .Where(t => t.Progress.Progress == "In Progress" && t.HighPriority == false)
            .OrderBy(t => t.Id)
            .ToListAsync();

        if (tasks == null || !tasks.Any())
        {
            return NotFound("No tasks in progress were found.");
        }

        var tasksList = tasks.Select(t => new TasksDto
        {
            Id = t.Id,
            Title = t.Title,
            Subject = t.Subject.Name
        }).ToList();

        return Ok(tasksList);
    }

    [HttpPut]
    public async Task<IActionResult> TaskListsUpdate(TasksDto taskDto, int id)
    {
        var task = await context.Tasks
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (!string.IsNullOrEmpty(taskDto.Title))
        {
            task.Title = taskDto.Title;
        }

        if (!string.IsNullOrEmpty(taskDto.Subject))
        {
            task.Subject.Name = taskDto.Subject;
        }

        try
        {
            await context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the task.", error = ex.Message });
        }
    }


    [HttpPut]
    public async Task<IActionResult> TaskListsComplete(int id)
    {
        var findTask = await context.Tasks.Include(t => t.Progress).FirstOrDefaultAsync(t => t.Id == id);

        var newProgress = await context.TaskProgresses.FirstOrDefaultAsync(p => p.Id == 3);

        findTask.Progress = newProgress;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> StudentPerformanceMetrics()
    {
        var student = await context.Students.FirstOrDefaultAsync();

        var subjects = await context.Tasks.Include(t => t.Students.Where(s => s.Id == student.Id)).Include(t => t.Subject).Distinct().Take(5).Distinct().ToListAsync();

        var studentPerformanceMetrics = subjects.Select(s => new StudentPerformanceMetricsDto
        {
            Subject = s.Subject.Name,
            NumberOfA = context.Tasks.Where(t => t.Subject.Name == s.Subject.Name && t.Grade.Letter == "A").Count(),
            NumberOfB = context.Tasks.Where(t => t.Subject.Name == s.Subject.Name && t.Grade.Letter == "B").Count(),
            NumberOfC = context.Tasks.Where(t => t.Subject.Name == s.Subject.Name && t.Grade.Letter == "C").Count(),
            NumberOfD = context.Tasks.Where(t => t.Subject.Name == s.Subject.Name && t.Grade.Letter == "D").Count(),
        }).ToList();

        return Ok(studentPerformanceMetrics);
    }

    [HttpGet]
    public IActionResult OnTimeArrival()
    {
        

        var totalStudents = GetTotalStudents(today);

        var week = Helper.GetWeekDays(today, includeWeekends: false);

        var onTimeArrivalWeek = week.Select(d => new OnTimeArrivalWeekDto
        {
            DayOfWeek = d.Key,
            OnTimeAttendance = context.SchoolDashboards
                .Count(s => s.Date.Date == d.Value.Date && s.ArrivalStatuses.Status == "On-Time")
        }).ToList();

        var onTimeArrival = new OnTimeArrivalDto
        {
            OnTime = CalculateAttendancePercentage(startOfWeek: week["Monday"], "On-Time", totalStudents),
            OntimeDifference = CalculateAttendanceDifference(week["Monday"], totalStudents),
            Week = onTimeArrivalWeek
        };

        return Ok(onTimeArrival);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityLogs()
    {
        var week = Helper.GetWeekDays(today, includeWeekends: true);

        var totalStudents = await context.Students.CountAsync();

        var activityLogs = week.Select(d => new ActivityLogsDto
        {
            DayOfWeek = d.Value.ToString(),
            DailyAttendance = CalculateActivityPercentage(d.Value, "Attended", totalStudents),
            ExtraCurricular = CalculateActivityPercentage(d.Value, "ExtraCurricular", totalStudents),
            Support = CalculateActivityPercentage(d.Value, "Support", totalStudents)
        }).ToList();

        return Ok(activityLogs);
    }

    

    [HttpGet]
    public async Task<IActionResult> CafeteriaAndNutrition()
    {
        var attendedStudents = await context.SchoolDashboards
            .Where(s => s.Date.Date == today && s.Attended == true)
            .CountAsync();

        if (attendedStudents == 0)
        {
            var cafeteriaAndNutrition = new CafeteriaAndNutritionDto()
            {
                SchoolLunch = 0,
                LunchFromHome = 0,
                SkippingLunch = 0,
            };
            return Ok(cafeteriaAndNutrition);
        }

        var schoolLunch = await context.SchoolDashboards
            .Where(s => s.Date.Date == today && s.LunchChoices.Choice == "School Lunch")
            .CountAsync();

        var lunchFromHome = await context.SchoolDashboards
            .Where(s => s.Date.Date == today && s.LunchChoices.Choice == "Lunch from Home")
            .CountAsync();

        var cafeteriaAndNutritionResult = new CafeteriaAndNutritionDto()
        {
            SchoolLunch = Helper.GetPercentage(schoolLunch, attendedStudents),
            LunchFromHome = Helper.GetPercentage(lunchFromHome, attendedStudents),
            SkippingLunch = 100- Helper.GetPercentage(schoolLunch + lunchFromHome, attendedStudents),
        };

        return Ok(cafeteriaAndNutritionResult);
    }

    [HttpGet]
    public async Task<IActionResult> DailyAttendanceAndParticipation()
    {
        var totalStudents = await context.Students.CountAsync();

        var attendanceRate = await context.SchoolDashboards.Where(s => s.Date.Date == today && s.Attended).CountAsync();
        var onTimeArrival = await context.SchoolDashboards.Where(s => s.Date.Date == today && s.ArrivalStatuses.Status == "On-Time").CountAsync();
        var lateArrivals = await context.SchoolDashboards.Where(s => s.Date.Date == today && s.ArrivalStatuses.Status == "Late").CountAsync();
        var unexcusedAbsences = await context.SchoolDashboards.Where(s => s.Date.Date == today && s.Attended == false && s.AbsenceStatuses.Status == "Unexcused").CountAsync();
        var studentsSick = await context.SchoolDashboards.Where(s => s.Date.Date == today && s.Attended == false && s.AbsenceStatuses.Status == "Sick").CountAsync();

        var dailyAttendanceAndParticipation = new DailyAttendanceAndParticipationDto()
        {
            AttendanceRate = Helper.GetPercentage(attendanceRate, totalStudents),
            OnTimeArrival = Helper.GetPercentage(onTimeArrival, totalStudents),
            LateArrivals = Helper.GetPercentage(lateArrivals, totalStudents),
            UnexcusedAbsences = Helper.GetPercentage(unexcusedAbsences, totalStudents),
            StudentsSick = Helper.GetPercentage(studentsSick, totalStudents),
        };

        return Ok(dailyAttendanceAndParticipation);
    }

    [HttpGet]
    public async Task<IActionResult> PriorityTasks()
    {
        var tasks = await context.Tasks
            .Include(t => t.Progress)
            .Include(t => t.Subject)
            .Where(t => t.Progress.Progress == "In Progress" && t.HighPriority == true)
            .Take(6)
            .ToListAsync();

        if (tasks == null || !tasks.Any())
        {
            return NotFound("No high-priority tasks in progress were found.");
        }

        var priorityTasks = tasks.Select(t => new TasksDto
        {
            Title = t.Title,
            Subject = t.Subject.Name
        }).ToList();

        return Ok(priorityTasks);
    }
    
    
    // ------------------- HELPING METHODS -------------------

    
    private int GetTotalStudents(DateTime currentDate)
    {
        return context.Students.Count(s => s.AdmissionDate.Month <= currentDate.Month);
    }

    private int CalculateAttendancePercentage(DateTime startOfWeek, string status, int totalStudents)
    {
        return context.SchoolDashboards
            .Count(s => s.Date.Date == startOfWeek.Date && s.ArrivalStatuses.Status == status)
            * 100 / totalStudents;
    }

    private int CalculateAttendanceDifference(DateTime startOfWeek, int totalStudents)
    {
        var currentWeekOnTime = context.SchoolDashboards
            .Count(s => s.Date.Date == startOfWeek.Date && s.ArrivalStatuses.Status == "On-Time");

        var previousWeekOnTime = context.SchoolDashboards
            .Count(s => s.Date.Date == startOfWeek.AddDays(-7).Date && s.ArrivalStatuses.Status == "On-Time");

        return (previousWeekOnTime - currentWeekOnTime) * 100 / totalStudents;
    }

    private int CalculateActivityPercentage(DateTime date, string activityType, int totalStudents)
    {
        return activityType switch
        {
            "Attended" => context.SchoolDashboards.Count(s => s.Date.Date == date.Date && s.Attended) * 100 / totalStudents,
            "ExtraCurricular" => context.SchoolDashboards.Count(s => s.Date.Date == date.Date && s.ExtraCurricular) * 100 / totalStudents,
            "Support" => context.SchoolDashboards.Count(s => s.Date.Date == date.Date && s.Support) * 100 / totalStudents,
            _ => 0
        };
    }

    // --------------------------------------------------------------------------------


}


