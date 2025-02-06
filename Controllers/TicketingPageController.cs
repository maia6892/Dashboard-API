using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.TicketingPage;
using DashboardAPI.Models.TicketingPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TicketingPageController(DashboardDbContext context) : ControllerBase
    {
        private readonly DateTime today = DateTime.UtcNow.Date;
        private readonly int thisYear = DateTime.UtcNow.Year;
        private readonly int lastYear = DateTime.UtcNow.AddYears(-1).Year;
        private readonly int thisMonth = DateTime.UtcNow.Month;
        private readonly int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;

        [HttpGet]
        public async Task<IActionResult> TicketSalesAnalytics(string timestamp, int companyId = 1)
        {
            var tickets = new List<TicketPurchase>();
            var result = new List<TicketSalesAnalyticsDto>();

            var hours = Helper.GetHoursOfToday(today);
            var weekOfMonth = Helper.GetWeeksOfMonth(new DateTime(thisYear, thisMonth, 1));
            var monthOfYear = Helper.GetMonthsOfYear(new DateTime(thisYear, 1, 1));
            var allDates = context.TicketPurchases.Select(tp => tp.PurchaseDate).ToList();
            var years = Helper.GetYearsInDatabase(allDates);

            switch (timestamp.ToLower())
            {
                case "today":
                    tickets = await context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Date == today).ToListAsync();
                    result = hours.Select(h => new TicketSalesAnalyticsDto()
                    {
                        DateOrTime = h.Key,
                        Amount = tickets.Where(o => o.PurchaseDate.Hour == h.Value.Hour).Sum(o => o.Amount)
                    }).ToList();
                    break;
                case "weekly":
                    tickets = await context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Month == thisMonth).ToListAsync();
                    result = weekOfMonth.Select(w => new TicketSalesAnalyticsDto()
                    {
                        DateOrTime = w.Key,
                        Amount = tickets.Where(o => o.PurchaseDate.Date >= w.Value.StartDate && o.PurchaseDate.Date <= w.Value.EndDate).Sum(o => o.Amount)
                    }).ToList();
                    break;
                case "monthly":
                    tickets = await context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Year == thisYear).ToListAsync();
                    result = monthOfYear.Select(m => new TicketSalesAnalyticsDto()
                    {
                        DateOrTime = m.Key,
                        Amount = tickets.Where(o => o.PurchaseDate.Month == m.Value.Month).Sum(o => o.Amount)
                    }).ToList();
                    break;
                case "yearly":
                    tickets = await context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId).ToListAsync();
                    result = years.Select(y => new TicketSalesAnalyticsDto()
                    {
                        DateOrTime = y.Key,
                        Amount = tickets.Where(o => o.PurchaseDate.Year == y.Value.StartDate.Year).Sum(o => o.Amount)
                    }).ToList();
                    break;
            };

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> TicketSelling(string timestamp, int companyId = 1)
        {
            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            var tickets = await context.Tickets
                .Where(o => o.Company.Id == companyId)
                .Include(o => o.Category)
                .ToListAsync();

            var ticketsPuchused = await context.TicketPurchases
                .Where(o => o.Customer.TravelCompany.Id == companyId)
                .Include(o => o.Customer)
                .Include(o => o.Ticket)
                .Include(o => o.Status)
                .ToListAsync();

            var ticketList = new List<Ticket>();
            var ticketPurchase = new List<TicketPurchase>();

            switch (timestamp)
            {
                case "Today":
                    ticketPurchase = ticketsPuchused.Where(o => o.PurchaseDate.Date == today).ToList();
                    ticketList = tickets.Where(o => o.PublishDate.Date == today).ToList();
                    break;
                case "This Week":
                    ticketPurchase = ticketsPuchused.Where(o => o.PurchaseDate.Date >= startOfCurrentWeek && o.PurchaseDate.Date <= today).ToList();
                    ticketList = tickets.Where(o => o.PublishDate.Date >= startOfCurrentWeek && o.PublishDate.Date <= today).ToList();
                    break;
                case "This Month":
                    ticketPurchase = ticketsPuchused.Where(o => o.PurchaseDate.Month == thisMonth).ToList();
                    ticketList = tickets.Where(o => o.PublishDate.Month == thisMonth).ToList();
                    break;
                case "This Year":
                    ticketPurchase = ticketsPuchused.Where(o => o.PurchaseDate.Year == thisYear).ToList();
                    ticketList = tickets.Where(o => o.PublishDate.Year == thisYear).ToList();
                    break;
                default:
                    return BadRequest();
            }

            var ticketSelling = new
            {
                TicketSold = ticketPurchase.Count(),
                TicketSoldPercentage = Helper.GetPercentage(ticketPurchase.Count(), ticketList.Count()),
                AvailableTickets = ticketList.Count() - ticketPurchase.Count(),
                AvailableTicketsPercentage = Helper.GetPercentage(ticketList.Count() - ticketPurchase.Count(), ticketList.Count()),
                PendingPayment = ticketPurchase.Count(o => o.Status.Name == "Pending"),
                PendingPaymentPercentage = Helper.GetPercentage(ticketPurchase.Count(o => o.Status.Name == "Pending"), ticketPurchase.Count()),
            };

            return Ok(ticketSelling);
        }

        [HttpGet]
        public async Task<IActionResult> QuickReview(int companyId = 1)
        {
            var sales = await context.TicketPurchases.Where(o => o.PurchaseDate.Month == thisMonth && o.Customer.TravelCompany.Id == companyId).CountAsync();
            var salesLastMonth = await context.TicketPurchases.Where(o => o.PurchaseDate.Month == lastMonth && o.Customer.TravelCompany.Id == companyId).CountAsync();
            var customer = await context.TravelCompanyCustomers.Where(o => o.TravelCompany.Id == companyId).CountAsync();

            var quickReview = new QuickReviewDto()
            {
                Sales = sales,
                Customer = customer,
                Growth = Helper.GetPercentageDifference(salesLastMonth, sales)
            };

            return Ok(quickReview);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerStatistics(int companyId = 1)
        {
            var totalCustomers = await context.TravelCompanyCustomers
                .Where(o => o.TravelCompany.Id == companyId)
                .CountAsync();

            var customers = await context.TravelCompanyCustomers
                .Where(o => o.TravelCompany.Id == companyId)
                .ToListAsync();

            var adult = customers.Count(o => Helper.CalculateAge(o.BirthDate) >= 30 && Helper.CalculateAge(o.BirthDate) <= 45);
            var young = customers.Count(o => Helper.CalculateAge(o.BirthDate) >= 17 && Helper.CalculateAge(o.BirthDate) < 30);
            var teenager = customers.Count(o => Helper.CalculateAge(o.BirthDate) >= 11 && Helper.CalculateAge(o.BirthDate) < 17);
            var kid = customers.Count(o => Helper.CalculateAge(o.BirthDate) >= 6 && Helper.CalculateAge(o.BirthDate) < 11);

            var customerAgeGroups = new Dictionary<string, int>()
            {
                {"Adult", adult},
                {"Young", young},
                {"Teenager", teenager},
                {"Kid", kid},
            };

            var customerStatistics = customerAgeGroups.Select(c => new CustomerStatisticsDto()
            {
                CustomerAgeGroup = c.Key,
                CustomerPercentage = Helper.GetPercentage(c.Value, totalCustomers)
            }).ToList();

            return Ok(customerStatistics);
        }


        [HttpGet]
        public async Task<IActionResult> LatestTransactions(string? customer, string? date, string? status, int companyId = 1)
        {
            var query = context.TicketPurchases
                .Where(o => o.Customer.TravelCompany.Id == companyId)
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .AsQueryable();

            if (!string.IsNullOrEmpty(customer))
            {
                query = query.Where(e => e.Customer.Name.Contains(customer));
            }


            if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(date, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US"), DateTimeStyles.None, out var parsedDate))
            {
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                DateTime nextDay = parsedDate.AddDays(1);
                nextDay = DateTime.SpecifyKind(nextDay, DateTimeKind.Utc);

                query = query.Where(e => e.PurchaseDate >= parsedDate && e.PurchaseDate < nextDay);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(e => e.Status != null && e.Status.Name == status);
            }

            var result = await query
                .OrderByDescending(c => c.PurchaseDate)
                .Take(1000)
                .Select(c => new LatestTransactionsDto
                {
                    CustomerName = c.Customer.Name,
                    PurchaseDate = c.PurchaseDate.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("en-US")),
                    Amount = c.Amount,
                    Status = c.Status.Name
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> DailySalesGrowth(int companyId = 1)
        {
            var sales = await context.TicketPurchases.Where(o => o.PurchaseDate.Date == today && o.Customer.TravelCompany.Id == companyId).CountAsync();
            return Ok(sales);
        }

        [HttpGet]
        public async Task<IActionResult> TicketAvailability(int companyId = 1)
        {
            var ticketSold = await context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Month == thisMonth).CountAsync();
            var tickets = await context.Tickets.Where(o => o.Company.Id == companyId && o.PublishDate.Month == thisMonth).CountAsync();
            var ticketAvailability = new
            {
                TicketSold = ticketSold,
                TicketSoldPercentage = Helper.GetPercentage(ticketSold, tickets),
                AvailableTickets = tickets - ticketSold,
                AvailableTicketsPercentage = Helper.GetPercentage(tickets - ticketSold, tickets)
            };
            return Ok(ticketAvailability);
        }

        [HttpGet]
        public async Task<IActionResult> SalesSummary(int companyId = 1)
        {
            var ticketCategories = await context.TicketCategories.ToListAsync();
            var salesSummary = new
            {
                TicketSold = context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Month == thisMonth).Count(),
                Categories = ticketCategories.Select(c => new
                {
                    Category = c.Name,
                    Percentage = Helper.GetPercentage(context.TicketPurchases.Where(o => o.Ticket.Category.Id == c.Id && o.PurchaseDate.Month == thisMonth).Count(), context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Month == thisMonth).Count())
                })
            };
            return Ok(salesSummary);
        }


        [HttpGet]
        public IActionResult SalesComparison(int companyId = 1)
        {
            var week = Helper.GetTwoWeekStartingToday(today);
            var ticketSold = context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Month == thisMonth).Count();
            var ticketSoldLastMonth = context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Month == lastMonth).Count();

            var salesComparison = new
            {
                TicketSold = ticketSold,
                TicketSoldLastMonth = Helper.GetPercentageDifference(ticketSoldLastMonth, ticketSold),
                Week = week.Select(w => new
                {
                    Day = w.Key,
                    Sold = context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Date == w.Value.Date).Count(),
                    Available = context.Tickets.Where(o => o.Company.Id == companyId && o.PublishDate.Date == w.Value.Date).Count() - context.TicketPurchases.Where(o => o.Customer.TravelCompany.Id == companyId && o.PurchaseDate.Date == w.Value.Date).Count()
                })
            };

            return Ok(salesComparison);
        }
    }
}