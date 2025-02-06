using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.LogisticsPage;
using DashboardAPI.DTOs.PointOfSalesPage;
using DashboardAPI.Models.PointOfSalesPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PointOfSalesPageController(DashboardDbContext context) : ControllerBase
    {
        private readonly DateTime today = DateTime.UtcNow.Date;
        private readonly int thisYear = DateTime.UtcNow.Year;
        private readonly int lastYear = DateTime.UtcNow.AddYears(-1).Year;
        private readonly int thisMonth = DateTime.UtcNow.Month;
        private readonly int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;

        [HttpGet]
        public IActionResult SalesOvewerviewByFormula(int marketId = 1)
        {
            var monthOfYear = Helper.GetMonthsOfYear(new DateTime(thisYear, 1, 1));
            var salesOverview = monthOfYear.Select(m => new
            {
                Month = m.Key,
                Sales = context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == m.Value.Month).Sum(o => o.Product.Price * o.Quantity),
                GrossMargin = context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == m.Value.Month).Sum(o => o.Product.Price * o.Quantity) * 80 / 100,
                NetMargin = context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == m.Value.Month).Sum(o => o.Product.Price * o.Quantity) - context.MarketPackages.Where(o => o.CreatedAt.Month == m.Value.Month).Sum(o => o.Product.Price * o.Quantity) * 80 / 100
            });
            return Ok(salesOverview);
        }

        [HttpGet]
        public async Task<IActionResult> PopularSearch(int marketId = 1)
        {
            var categories = await context.MarketCategories.ToListAsync();
            var customer = await context.MarketCustomers.Where(o => o.Market.Id == marketId).FirstOrDefaultAsync();
            var result = new
            {
                Customer = new
                {
                    CustomerName = customer?.Name,
                    Balance = Math.Round(customer.Balance, 2),
                    CardNumber = customer.Card
                },
                Categories = categories.Select(c => new
                {
                    CategoryName = c.Name,
                    SalePercentage = Helper.GetPercentage(context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Category.Id == c.Id).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId).Count()),
                })
            };

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> SalesByCategory(string timestamp, int marketId = 1)
        {
            var startOfLastWeek = today.AddDays(-((int)today.DayOfWeek + 6) % 7 - 7);
            var endOfLastWeek = startOfLastWeek.AddDays(6);

            var categories = await context.MarketCategories.ToListAsync();
            var salesByCategory = new List<SalesByCategoryDto>();

            switch (timestamp.ToLower())
            {
                case "today":
                    salesByCategory = categories.Select(c => new SalesByCategoryDto
                    {
                        CategoryName = c.Name,
                        SalePercentage = Helper.GetPercentage(context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Category.Id == c.Id && o.CreatedAt.Date == today).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Date == today).Count()),
                        CategoryProducts = context.MarketProducts.Where(o => o.Market.Id == marketId && o.Category.Id == c.Id).Count()
                    }).ToList();
                    break;
                case "last week":
                    salesByCategory = categories.Select(c => new SalesByCategoryDto
                    {
                        CategoryName = c.Name,
                        SalePercentage = Helper.GetPercentage(context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Category.Id == c.Id && o.CreatedAt.Date >= startOfLastWeek && o.CreatedAt.Date <= endOfLastWeek).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Date >= startOfLastWeek && o.CreatedAt.Date <= endOfLastWeek).Count()),
                        CategoryProducts = context.MarketProducts.Where(o => o.Market.Id == marketId && o.Category.Id == c.Id).Count()
                    }).ToList();
                    break;
                case "last month":
                    salesByCategory = categories.Select(c => new SalesByCategoryDto
                    {
                        CategoryName = c.Name,
                        SalePercentage = Helper.GetPercentage(context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Category.Id == c.Id && o.CreatedAt.Month == thisMonth).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == thisMonth).Count()),
                        CategoryProducts = context.MarketProducts.Where(o => o.Market.Id == marketId && o.Category.Id == c.Id).Count()
                    }).ToList();
                    break;
                case "last year":
                    salesByCategory = categories.Select(c => new SalesByCategoryDto
                    {
                        CategoryName = c.Name,
                        SalePercentage = Helper.GetPercentage(context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Category.Id == c.Id && o.CreatedAt.Year == lastYear).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Year == lastYear).Count()),
                        CategoryProducts = context.MarketProducts.Where(o => o.Market.Id == marketId && o.Category.Id == c.Id).Count()
                    }).ToList();
                    break;

                default:
                    return BadRequest();
            }

            return Ok(salesByCategory);
        }

        [HttpGet]
        public async Task<IActionResult> Transactions(DateTime date, int marketId = 1)
        {
            var transactions = await context.MarketPackages
                .Where(o => o.Market.Id == marketId && o.CreatedAt.Date == date.Date)
                .Include(o => o.Product)
                .Include(o => o.Customer)
                    .ThenInclude(c => c.City)
                    .ThenInclude(ct => ct.Country)
                .Include(o => o.Status)
                .Include(o => o.SocialPlatform)
                .ToListAsync();

            var result = transactions.Select(t => new TransactionsDto
            {
                OrderId = t.Id,
                ProductName = t.Product.Name,
                ProductImageUrl = t.Product.Image,
                Quantity = t.Quantity,
                Total = t.Product.Price * t.Quantity,
                Status = t.Status.Name,
                Customer = t.Customer.Name,
                City = t.Customer.City.Name,
                Country = t.Customer.City.Country.Name
            });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int marketId = 1)
        {
            var totalOrders = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Date <= today).CountAsync();

            var totalOrdersThisMonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == thisMonth).CountAsync();
            var totalOrdersLastMonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == lastMonth).CountAsync();

            var totalSalesThisMonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == thisMonth).SumAsync(o => o.Product.Price * o.Quantity);
            var totalSalesLastMonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == lastMonth).SumAsync(o => o.Product.Price * o.Quantity);

            var growthThisMonth = Helper.GetPercentage(totalOrdersThisMonth, totalOrders);
            var growthLastMonth = Helper.GetPercentage(totalOrdersThisMonth, totalOrders);

            var quickView = new
            {
                TotalOrders = totalOrdersThisMonth,
                TotalOrdersPercentage = Helper.GetPercentageDifference(totalOrdersLastMonth, totalOrdersThisMonth),
                TotalSales = totalSalesThisMonth,
                TotalSalesPercentage = Helper.GetPercentageDifference(totalSalesLastMonth, totalSalesThisMonth),
                MonthlyGrowth = growthThisMonth,
                MonthlyGrowthPercentage = growthThisMonth - growthLastMonth
            };

            return Ok(quickView);
        }

        [HttpGet]
        public async Task<IActionResult> TotalOrdersByPlatform(int marketId = 1)
        {
            var platforms = await context.SocialPlatforms.ToListAsync();
            var monthOfYear = Helper.GetMonthsOfYear(new DateTime(thisYear, 1, 1));

            var result = monthOfYear.Select(m => new
            {
                totalOrdersByPlatform = platforms.Select(p => new
                {
                    Month = m.Key,
                    OrdersPersentage = Helper.GetPercentage(context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == m.Value.Month && o.SocialPlatform.Id == p.Id).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == m.Value.Month).Count()),
                })
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> TotalOrdersByPlatformAdd([FromBody] SocialPlatform newPlatform)
        {
            if (newPlatform == null || string.IsNullOrEmpty(newPlatform.Name))
            {
                return BadRequest("Invalid platform data.");
            }

            var platform = new SocialPlatform
            {
                Name = newPlatform.Name
            };

            await context.SocialPlatforms.AddAsync(platform);
            await context.SaveChangesAsync();

            return Ok(new { Message = "Platform added successfully", Platform = platform });
        }

        [HttpGet]
        public async Task<IActionResult> ProductSales(string month, int year, int marketId = 1)
        {
            var monthNumber = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;

            var products = await context.MarketProducts
                .Where(o => o.Market.Id == marketId)
                .Include(o => o.Status)
                .ToListAsync();

            var result = products.Select(p => new ProductSalesDto
            {
                ProductName = p.Name,
                ProductImageUrl = p.Image,
                Status = p.Status.Name,
                Stock = p.Stock,
                Price = p.Price,
                Sales = context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Id == p.Id && o.CreatedAt.Month == monthNumber && o.CreatedAt.Year == year).Sum(o => o.Quantity),
                Earnings = p.Price * context.MarketPackages.Where(o => o.Market.Id == marketId && o.Product.Id == p.Id && o.CreatedAt.Month == monthNumber && o.CreatedAt.Year == year).Sum(o => o.Quantity)
            });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Visitors(string timestamp, int marketId = 1)
        {
            var week = Helper.GetThisWeekDays(today, includeWeekends: true);
            var months = Helper.GetMonthsOfYear(new DateTime(thisYear, 1, 1));
            var years = Helper.GetYearsInDatabase(context.MarketPackages.Select(o => o.CreatedAt).ToList());
            
            var newVisitorsThisMont = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == thisMonth).Select(o => o.Customer).Distinct().CountAsync();
            var newVisitorsLastmonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == lastMonth).Select(o => o.Customer).Distinct().CountAsync();

            var returningThisMonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == thisMonth).Select(o => o.Customer).CountAsync();
            var returningLastMonth = await context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Month == lastMonth).Select(o => o.Customer).CountAsync();

            var newAndReturningVisitors = new 
            {
                NewVisitors = newVisitorsThisMont,
                NewVisitorsPercentage = Helper.GetPercentageDifference(newVisitorsLastmonth, newVisitorsThisMont),
                ReturningVisitors = returningThisMonth,
                ReturningVisitorsPercentage = Helper.GetPercentageDifference(returningLastMonth, returningThisMonth)
            };

            var totalVisitors = new List<VisitorsDto>();
            
            switch (timestamp.ToLower())
            {
                case "weekly":
                    totalVisitors = week.Select(w => new VisitorsDto
                    {
                        Date = w.Key,
                        TotalVisitors = context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Date == w.Value).Select(o => o.Customer).Distinct().Count()
                    }).ToList();
                    break;
                case "monthly":
                    totalVisitors = months.Select(w => new VisitorsDto
                    {
                        Date = w.Key,
                        TotalVisitors = context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Date == w.Value).Select(o => o.Customer).Distinct().Count()
                    }).ToList();

                    break;
                case "yearly":
                    totalVisitors = months.Select(w => new VisitorsDto
                    {
                        Date = w.Key,
                        TotalVisitors = context.MarketPackages.Where(o => o.Market.Id == marketId && o.CreatedAt.Date == w.Value).Select(o => o.Customer).Distinct().Count()
                    }).ToList();
                    break;
                default:
                    return BadRequest();
            }

            var result = new
            {
                NewAndReturningVisitors = newAndReturningVisitors,
                TotalVisitors = totalVisitors
            };

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> SalesActivity(string month, int marketId = 1)
        {
            var monthNumber = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;

            var statuses = await context.MarketPackageStatuses.ToListAsync();
            var result = statuses.Select(s => new
            {
                Status = s.Name,
                Amount = context.MarketPackages.Where(o => o.Market.Id == marketId && o.Status.Id == s.Id && o.CreatedAt.Month == monthNumber && o.CreatedAt.Year == thisYear).Count(),
                Percentage = Helper.GetPercentageDifference(context.MarketPackages.Where(o => o.Market.Id == marketId && o.Status.Id == s.Id && o.CreatedAt.Month == monthNumber && o.CreatedAt.Year == thisYear).Count(), context.MarketPackages.Where(o => o.Market.Id == marketId && o.Status.Id == s.Id && o.CreatedAt.Month == thisMonth && o.CreatedAt.Year == thisYear).Count())
            });
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> RecentActivity(int marketId = 1)
        {
            var activities = await context.MarketActivities
                .Where(a => a.Market.Id == marketId && a.Date.Date < today.Date)
                .OrderByDescending(a => a.Date)
                .Take(100)
                .ToListAsync();

            return Ok(activities);
        }
    }
}