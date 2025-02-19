using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.RestaurantPage;
using DashboardAPI.Models.RestaurantPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RestaurantPageController(DashboardDbContext context) : ControllerBase
    {
        private readonly DateTime today = DateTime.UtcNow.Date;
        private readonly int thisMonth = DateTime.UtcNow.Month;
        private readonly int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;
        private readonly int year = DateTime.UtcNow.Year;

        [HttpGet]
        public async Task<IActionResult> QuickRewiewTop(int restaurantId = 1)
        {
            var income = await context.DishOrderDishes
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId && dod.DishOrder.OrderDate.Month == thisMonth)
                .Select(dod => dod.Dish.Price * dod.Quantity)
                .SumAsync();

            var incomeLastMonth = await context.DishOrderDishes
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId && dod.DishOrder.OrderDate.Month == lastMonth)
                .Select(dod => dod.Dish.Price * dod.Quantity)
                .SumAsync();

            var expense = await context.DishOrderDishes
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId && dod.DishOrder.OrderDate.Month == thisMonth)
                .Select(dod => dod.Dish.CookingCost * dod.Quantity)
                .SumAsync();

            var expenseLastMonth = await context.DishOrderDishes
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId && dod.DishOrder.OrderDate.Month == lastMonth)
                .Select(dod => dod.Dish.CookingCost * dod.Quantity)
                .SumAsync();

            var quickRewiew = new QuickReviewTopDto()
            {
                Income = income,
                IncomePercentage = Helper.GetPercentageDifference(incomeLastMonth, income),
                Expense = expense,
                ExpensePercentage = Helper.GetPercentageDifference(expenseLastMonth, expense),
                TotalIncome = income - expense
            };
            return Ok(quickRewiew);
        }

        [HttpGet]
        public async Task<IActionResult> QuickRewiewBottom(int restaurantId = 1)
        {
            var quickRewiew = new QuickReviewBottomDto()
            {
                TotalOrdersCompleted = await context.DishOrders.Where(d => d.OrderDate.Month == thisMonth && d.Restaurant.Id == restaurantId && d.Status.Status == "Completed").CountAsync(),
                TotalOrdersDelivered = await context.DishOrders.Where(d => d.OrderDate.Month == thisMonth && d.Restaurant.Id == restaurantId && d.Status.Status == "Delivered").CountAsync(),
                TotalOrdersCanceled = await context.DishOrders.Where(d => d.OrderDate.Month == thisMonth && d.Restaurant.Id == restaurantId && d.Status.Status == "Canceled").CountAsync(),
                TotalOrdersPending = await context.DishOrders.Where(d => d.OrderDate.Month == thisMonth && d.Restaurant.Id == restaurantId && d.Status.Status == "On Process").CountAsync()
            };
            return Ok(quickRewiew);
        }

        [HttpGet]
        public async Task<IActionResult> CurrentOrderSummary(string timestamp, int restaurantId = 1)
        {
            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfCurrentWeek = today.AddDays(-6);
            }
            startOfCurrentWeek = DateTime.SpecifyKind(startOfCurrentWeek, DateTimeKind.Utc);

            var currentOrderSummary = new List<DishOrder>();

            switch (timestamp.ToLower())
            {
                case "today":
                    currentOrderSummary = await context.DishOrders
                        .Where(d => d.OrderDate >= today && d.OrderDate < today.AddDays(1) && d.Restaurant.Id == restaurantId)
                        .Include(d => d.DishOrderDishes)
                        .Include(d => d.Visitor)
                        .Include(d => d.Restaurant)
                        .Include(d => d.Status)
                        .ToListAsync();
                    break;

                case "weekly":
                    currentOrderSummary = await context.DishOrders
                        .Where(d => d.OrderDate >= startOfCurrentWeek && d.OrderDate < today.AddDays(1) && d.Restaurant.Id == restaurantId)
                        .Include(d => d.DishOrderDishes)
                        .Include(d => d.Visitor)
                        .Include(d => d.Restaurant)
                        .Include(d => d.Status)
                        .ToListAsync();
                    break;

                case "monthly":
                    var startOfMonth = DateTime.SpecifyKind(new DateTime(year, thisMonth, 1), DateTimeKind.Utc);
                    var startOfNextMonth = startOfMonth.AddMonths(1);

                    currentOrderSummary = await context.DishOrders
                        .Where(d => d.OrderDate >= startOfMonth && d.OrderDate < startOfNextMonth && d.Restaurant.Id == restaurantId)
                        .Include(d => d.DishOrderDishes)
                        .Include(d => d.Visitor)
                        .Include(d => d.Restaurant)
                        .Include(d => d.Status)
                        .ToListAsync();
                    break;

                case "yearly":
                    var startOfYear = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);
                    var startOfNextYear = startOfYear.AddYears(1);

                    currentOrderSummary = await context.DishOrders
                        .Where(d => d.OrderDate >= startOfYear && d.OrderDate < startOfNextYear && d.Restaurant.Id == restaurantId)
                        .Include(d => d.DishOrderDishes)
                        .Include(d => d.Visitor)
                        .Include(d => d.Restaurant)
                        .Include(d => d.Status)
                        .ToListAsync();
                    break;

                default:
                    return BadRequest("Not a valid timestamp.");
            }

            var delivered = currentOrderSummary.Count(d => d.Status.Status == "Delivered");
            var onProcess = currentOrderSummary.Count(d => d.Status.Status == "On Process");
            var newOrders = currentOrderSummary.Count(d => d.Status.Status == "New orders");

            var totalOrders = delivered + onProcess + newOrders;

            var result = new CurrentOrderSummaryDto
            {
                Delivered = delivered,
                OnProcess = onProcess,
                NewOrders = newOrders,
                DeliveredPercentage = 100 - Helper.GetPercentage(onProcess, totalOrders) - Helper.GetPercentage(newOrders, totalOrders),
                OnProcessPercentage = Helper.GetPercentage(onProcess, totalOrders),
                NewOrdersPercentage = Helper.GetPercentage(newOrders, totalOrders)
            };

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> TodaysLiveOrders(int restaurantId = 1)
        {
            var todaysLiveOrders = await context.DishOrderDishes
                .Include(dod => dod.DishOrder)
                .Include(dod => dod.Dish)
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId
                            && dod.DishOrder.OrderDate >= today
                            && dod.DishOrder.OrderDate < today.AddDays(1)
                            && dod.DishOrder.Status.Status == "New orders")
                .ToListAsync();

            var result = todaysLiveOrders.Select(dod => new TodaysLiveOrdersDto
            {
                Name = "Order #" + dod.Id,
                Amount = dod.Dish.Price * dod.Quantity,
                Date = dod.DishOrder.OrderDate.ToString("hh:mm tt")
            }).ToList();

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> TopSellingProducts(int restaurantId = 1)
        {
            var dishes = await context.Dishes.ToListAsync();

            var dishOrderDishes = await context.DishOrderDishes
                .Include(dod => dod.DishOrder)
                .Include(dod => dod.Dish)
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId
                            && dod.DishOrder.OrderDate.Month == thisMonth)
                .ToListAsync();

            var result = dishes.Select(tsp => new TopSellingProductsDto
            {
                Name = tsp.Name,
                Stars = (dishOrderDishes.Where(d => d.Dish.Id == tsp.Id && d.DishOrder.Stars != null).Count() > 0
                    ? dishOrderDishes.Where(d => d.Dish.Id == tsp.Id).Sum(d => d.DishOrder.Stars) / dishOrderDishes.Where(d => d.Dish.Id == tsp.Id && d.DishOrder.Stars != null).Count()
                    : 0) ?? 0,
                Image = tsp.PhotoUrl
            })
            .OrderByDescending(tsp => tsp.Stars)
            .ToList();

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> TrendingItems(string timestamp, int restaurantId = 1)
        {
            if (string.IsNullOrEmpty(timestamp) || !DateTime.TryParseExact(timestamp, "MMMM yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var selectedDate))
            {
                return BadRequest("Invalid date format. Please provide a date in the format 'MMMM yyyy', e.g., 'November 2024'.");
            }

            var startOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var dishes = await context.Dishes.ToListAsync();

            var dishOrderDishes = await context.DishOrderDishes
                .Include(dod => dod.DishOrder)
                .Include(dod => dod.Dish)
                .Where(dod => dod.DishOrder.Restaurant.Id == restaurantId
                            && dod.DishOrder.OrderDate >= startOfMonth
                            && dod.DishOrder.OrderDate < endOfMonth)
                .ToListAsync();

            var trendingItems = dishes.Select(ti => new TrendingItemsDto
            {
                Name = ti.Name,
                Price = ti.Price,
                Image = ti.PhotoUrl,
                Sales = dishOrderDishes.Where(d => d.Dish.Id == ti.Id).Sum(d => d.Quantity),
                SalesPercentage = Helper.GetPercentage(dishOrderDishes.Where(d => d.Dish.Id == ti.Id).Sum(d => d.Quantity), dishOrderDishes.Sum(d => d.Quantity))
            })
            .OrderByDescending(item => item.Sales)
            .ToList();

            for (int i = 0; i < trendingItems.Count; i++)
            {
                trendingItems[i].Rank = i + 1;
            }

            return Ok(trendingItems);
        }

        [HttpGet]
        public IActionResult DailyOrderReport(int restaurantId = 1)
        {
            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfCurrentWeek = today.AddDays(-6);
            }
            startOfCurrentWeek = DateTime.SpecifyKind(startOfCurrentWeek, DateTimeKind.Utc);

            var week = Helper.GetWeekDays(today, includeWeekends: true);

            var dailyOrderReport = week.Select(w => new DailyOrderReportDto
            {
                Day = w.Key,
                Orders = context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate >= w.Value.Date && d.OrderDate < w.Value.AddDays(1)).Count(),
                Date = w.Value.ToString("MMM d\\t\\h, yyyy", new System.Globalization.CultureInfo("en-US"))
            });

            return Ok(dailyOrderReport);
        }

        [HttpGet]
        public async Task<IActionResult> DailyOrderReportDownload(int restaurantId = 1)
        {
            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfCurrentWeek = today.AddDays(-6);
            }
            startOfCurrentWeek = DateTime.SpecifyKind(startOfCurrentWeek, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(startOfCurrentWeek.AddDays(6), DateTimeKind.Utc);

            var orders = await context.DishOrders
                .Where(d => d.Restaurant != null
                            && d.Restaurant.Id == restaurantId
                            && d.OrderDate >= startOfCurrentWeek
                            && d.OrderDate <= end)
                .Include(d => d.Restaurant)
                .Include(d => d.DishOrderDishes)
                    .ThenInclude(dod => dod.Dish)
                .ToListAsync();

            var reportLines = new List<string>
            {
                "OrderId,DishName,Quantity,OrderDate"
            };

            foreach (var order in orders)
            {
                foreach (var dishOrder in order.DishOrderDishes ?? Enumerable.Empty<DishOrderDish>())
                {
                    var dishName = dishOrder.Dish?.Name ?? "Unknown";
                    var line = $"{order.Id},{dishName},{dishOrder.Quantity},{order.OrderDate:yyyy-MM-dd HH:mm:ss}";
                    reportLines.Add(line);
                }
            }

            var csvContent = string.Join("\n", reportLines);
            var fileName = $"WeeklyOrderReport_{startOfCurrentWeek:yyyyMMdd}_{end:yyyyMMdd}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerReviews(int year, int restaurantId = 1)
        {

            var searchYear = new DateTime(year, 1, 1);

            var monthOfYear = Helper.GetMonthsOfYear(searchYear);

            var positiveThisMonth = await context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate.Date.Month == thisMonth && d.Stars >= 3).CountAsync();
            var positiveLastMonth = await context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate.Date.Month == lastMonth && d.Stars >= 3).CountAsync();

            var negativeThisMonth = await context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate.Date.Month == thisMonth && d.Stars < 3).CountAsync();
            var negativeLastMonth = await context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate.Date.Month == lastMonth && d.Stars < 3).CountAsync();

            var customerReviews = new
            {
                PositiveReviews = positiveThisMonth,
                PositiveReviewsPercentage = Helper.GetPercentage(positiveLastMonth, positiveThisMonth),
                NegativeReviews = negativeThisMonth,
                NegativeReviewsPercentage = Helper.GetPercentage(negativeLastMonth, negativeThisMonth),
                monthOfYear = monthOfYear.Select(m => new CustomerReviewsDto
                {
                    Month = m.Key,
                    Date = m.Value.ToString("MMM yyyy", new System.Globalization.CultureInfo("en-US")),
                    Positive = context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate.Date.Month == m.Value.Month && d.Stars >= 3).Count(),
                    Negative = context.DishOrders.Where(d => d.Restaurant.Id == restaurantId && d.OrderDate.Date.Month == m.Value.Month && d.Stars < 3).Count()
                }).ToList()
            };

            return Ok(customerReviews);
        }


        [HttpGet]
        public async Task<IActionResult> CustomerReviewsDownload(int year, int restaurantId = 1)
        {
            var searchYear = new DateTime(year, 1, 1);
            var monthsOfYear = Helper.GetMonthsOfYear(searchYear);
            var csvLines = new List<string>
            {
                "Month,PositiveReviews,NegativeReviews"
            };

            foreach (var month in monthsOfYear)
            {
                var positiveReviews = await context.DishOrders
                    .Where(d => d.Restaurant.Id == restaurantId
                                && d.OrderDate.Year == year
                                && d.OrderDate.Month == month.Value.Month
                                && d.Stars >= 3)
                    .CountAsync();

                var negativeReviews = await context.DishOrders
                    .Where(d => d.Restaurant.Id == restaurantId
                                && d.OrderDate.Year == year
                                && d.OrderDate.Month == month.Value.Month
                                && d.Stars < 3)
                    .CountAsync();

                var line = $"{month.Value.ToString("MMM yyyy", new System.Globalization.CultureInfo("en-US"))}," +
                            $"{positiveReviews},{negativeReviews}";

                csvLines.Add(line);
            }

            var csvContent = string.Join("\n", csvLines);

            var fileName = $"CustomerReviews_{year}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }

        
    }
}