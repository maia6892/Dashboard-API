using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.CryptocurrencyPage;
using DashboardAPI.Models.CryptocurrencyPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CryptocurrencyPageController : ControllerBase
    {
        private readonly TradeService _tradeService;
        private readonly DashboardDbContext _context;

        public CryptocurrencyPageController(DashboardDbContext context)
        {
            _tradeService = new TradeService(context);
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> QuickReview()
        {
            var desiredIds = new[] { "bitcoin", "ethereum", "litecoin", "ripple" };

            var url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd";
            var headers = new Dictionary<string, string> { { "accept", "application/json" } };
            var jsonResponse = await Helper.GetJsonResponseAsync(url, headers);

            var result = jsonResponse.RootElement
                .EnumerateArray()
                .Where(coin => desiredIds.Contains(coin.GetProperty("id").GetString()))
                .Select(coin => new
                {
                    Id = coin.GetProperty("id").GetString(),
                    CurrentPrice = coin.GetProperty("current_price").GetDecimal(),
                    PriceChangePercentage = coin.GetProperty("price_change_percentage_24h").GetDecimal(),
                    Image = coin.GetProperty("image").GetString()
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> MyPortfolio(string currencyName)
        {
            var url = $"https://min-api.cryptocompare.com/data/v2/histohour?fsym={currencyName}&tsym=USD&limit=23";
            var headers = new Dictionary<string, string>
            {
                { "x-api-key", "7d90023952598799db374a1a4e45a8c063a69c6337dcb1f139cc5ab29e802b7b" }
            };

            try
            {
                var jsonResponse = await Helper.GetJsonResponseAsync(url, headers);

                var res1 = jsonResponse.RootElement
                    .GetProperty("Data")
                    .GetProperty("Data")
                    .EnumerateArray()
                    .Select(dataPoint => dataPoint.GetProperty("high").GetDecimal())
                    .ToList();

                var url2 = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd";
                var headers2 = new Dictionary<string, string> { { "accept", "application/json" } };
                var jsonResponse2 = await Helper.GetJsonResponseAsync(url2, headers2);

                var res2 = jsonResponse2.RootElement
                    .EnumerateArray()
                    .Where(coin => coin.GetProperty("symbol").GetString() == currencyName)
                    .Select(coin => new
                    {
                        Id = coin.GetProperty("id").GetString(),
                        Symbol = coin.GetProperty("symbol").GetString(),
                        Name = coin.GetProperty("name").GetString(),
                        CurrentPrice = coin.GetProperty("current_price").GetDecimal(),
                        High = coin.GetProperty("high_24h").GetDecimal(),
                        Low = coin.GetProperty("low_24h").GetDecimal(),
                        PriceChangePercentage = coin.GetProperty("price_change_percentage_24h").GetDecimal(),
                        Volume = coin.GetProperty("total_volume").GetDecimal(),
                        VolumePercentage = coin.GetProperty("market_cap_change_percentage_24h").GetDecimal(),
                        Image = coin.GetProperty("image").GetString()
                    })
                    .ToList();

                var rate = (res2.FirstOrDefault()?.High - res2.FirstOrDefault()?.Low) / res2.FirstOrDefault()?.Low * 100 / 24;

                var result = new MyPortfolioDto()
                {
                    CurrencyShortName = currencyName.ToUpper(),
                    Price = res2.FirstOrDefault()?.CurrentPrice ?? 0,
                    PricePercentage = res2.FirstOrDefault()?.PriceChangePercentage ?? 0,
                    Rate = rate ?? 0,
                    Volume = res2.FirstOrDefault()?.Volume ?? 0,
                    VolumePercentage = res2.FirstOrDefault()?.VolumePercentage ?? 0,
                    PriceData = res1
                };

                return Ok(result);
            }
            catch (HttpRequestException e)
            {
                return BadRequest($"Request error: {e.Message}");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Data array not found in the response.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Unexpected error: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Statistics(string currencyName, int userId = 1)
        {
            var walletActivities = await _context.WalletActivities.Where(wa => wa.User.Id == userId).ToListAsync();
            var investments = await _context.Trades.Where(i => i.User.Id == userId && i.Currency.Name == currencyName).ToListAsync();

            var totalValue = walletActivities.Sum(wa => wa.Amount) + investments.Sum(i => i.TotalWithFees);
            var investmentTotalValue = _context.Trades.Where(i => i.User.Id == userId).Sum(i => i.TotalWithFees);

            var income = _context.WalletActivities.Where(wa => wa.User.Id == userId && wa.TransactionType.Operation == "Top Up").Sum(wa => wa.Amount) + _context.Trades.Where(i => i.User.Id == userId && i.Currency.Name == currencyName && i.OrderType.Type == "Sell").Sum(wa => wa.TotalWithFees);
            var spends = _context.WalletActivities.Where(wa => wa.User.Id == userId && wa.TransactionType.Operation == "Withdraw").Sum(wa => wa.Amount) + _context.Trades.Where(i => i.User.Id == userId && i.Currency.Name == currencyName && i.OrderType.Type == "Buy").Sum(wa => wa.TotalWithFees);
            var installments = _context.WalletActivities.Where(wa => wa.User.Id == userId && wa.IsInstallment).Sum(wa => wa.Amount);
            var invests = investments.Sum(i => i.TotalWithFees);

            var incomePercentage = Helper.GetPercentage(income, totalValue);
            var spendsPercentage = Helper.GetPercentage(spends, totalValue);
            var installmentsPercentage = Helper.GetPercentage(installments, totalValue);
            var investsPercentage = Helper.GetPercentage(invests, investmentTotalValue);

            var statistics = new StatisticsDto()
            {
                Income = income,
                IncomePercentage = incomePercentage,
                Spends = spends,
                SpendsPercentage = spendsPercentage,
                Installments = installments,
                InstallmentsPercentage = installmentsPercentage,
                Invests = invests,
                InvestsPercentage = investsPercentage,
            };
            return Ok(statistics);
        }



        [HttpGet]
        public async Task<IActionResult> SellOrder(string currencyName)
        {
            var sellOrder = await _context.TradeOrders.Where(t => t.Currency.Name == currencyName && t.OrderType.Type == "Sell").ToListAsync();
            var res = sellOrder.Select(so => new SellOrBuyOrderDto()
            {
                Price = so.Price,
                Amount = so.Amount,
                Total = so.Total
            }).ToList();

            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> BuyOrder(string currencyName)
        {
            var buyOrder = await _context.TradeOrders.Where(t => t.Currency.Name == currencyName && t.OrderType.Type == "Buy").ToListAsync();
            var res = buyOrder.Select(so => new SellOrBuyOrderDto()
            {
                Price = so.Price,
                Amount = so.Amount,
                Total = so.Total
            }).ToList();

            return Ok(res);
        }


        [HttpPost]
        public IActionResult QuickTrade([FromBody] TradeRequest request, string currencyName, int userId = 1)
        {
            var response = _tradeService.CalculateAndSaveTrade(request, currencyName, userId);
            return Ok(response);
        }


        [HttpGet]
        public async Task<IActionResult> OverviewBalance(string month, int year, int userId = 1)
        {
            try
            {
                if (!DateTime.TryParseExact(month, "MMMM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedMonth))
                {
                    return BadRequest("Invalid month name. Use full month name in English, e.g., 'November'.");
                }

                var startDate = new DateTime(year, parsedMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var walletActivities = await _context.WalletActivities
                    .Where(wa => wa.User.Id == userId && wa.Timestamp >= startDate && wa.Timestamp <= endDate)
                    .Include(wa => wa.TransactionType)
                    .Include(wa => wa.User)
                    .Include(wa => wa.Status)
                    .OrderBy(wa => wa.Timestamp)
                    .ToListAsync();

                var dailyBalances = new Dictionary<DateTime, decimal>();
                decimal currentBalance = 0;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dailyTransactions = walletActivities
                        .Where(wa => wa.Timestamp.Date == date.Date)
                        .ToList();

                    foreach (var transaction in dailyTransactions)
                    {
                        if (transaction.TransactionType.Operation == "Top Up")
                        {
                            currentBalance += transaction.Amount;
                        }
                        else if (transaction.TransactionType.Operation == "Withdrawal")
                        {
                            currentBalance -= transaction.Amount;
                        }
                    }

                    dailyBalances[date] = currentBalance;
                }

                var today = DateTime.UtcNow;
                var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
                var startOfLastWeek = startOfCurrentWeek.AddDays(-7);
                var endOfLastWeek = startOfCurrentWeek.AddDays(-1);

                var lastWeekActivities = await _context.WalletActivities
                    .Where(wa => wa.User.Id == userId && wa.Timestamp >= startOfLastWeek && wa.Timestamp <= endOfLastWeek)
                    .Include(wa => wa.TransactionType)
                    .Include(wa => wa.User)
                    .Include(wa => wa.Status)
                    .OrderBy(wa => wa.Timestamp)
                    .ToListAsync();

                decimal lastWeekBalance = 0;

                foreach (var transaction in lastWeekActivities)
                {
                    if (transaction.TransactionType?.Operation == "Top Up")
                    {
                        lastWeekBalance += transaction.Amount;
                    }
                    else if (transaction.TransactionType?.Operation == "Withdrawal")
                    {
                        lastWeekBalance -= transaction.Amount;
                    }
                }

                var currentWeekBalance = dailyBalances
                    .Where(db => db.Key >= startOfCurrentWeek && db.Key <= today)
                    .Sum(db => db.Value);

                decimal percentageDifference = Helper.GetPercentageDifference(lastWeekBalance, currentWeekBalance);

                var result = new
                {
                    LastWeekBalance = lastWeekBalance,
                    PercentageDifference = percentageDifference,
                    DailyBalances = dailyBalances.Select(db => new
                    {
                        Date = db.Key.ToString("yyyy-MM-dd"),
                        Balance = db.Value
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> WalletActivity(string timestamp, int userId = 1)
        {
            try
            {
                var today = DateTime.UtcNow;
                var monthly = today.Month;
                var yearly = today.Year;

                var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                var walletActivities = new List<WalletActivity>();

                switch (timestamp.ToLower())
                {
                    case "today":
                        walletActivities = await _context.WalletActivities
                            .Where(wa => wa.User != null && wa.User.Id == userId && wa.Timestamp.Date == today.Date)
                            .Include(wa => wa.TransactionType)
                            .Include(wa => wa.Status)
                            .OrderByDescending(wa => wa.Timestamp)
                            .ToListAsync();
                        break;
                    case "weekly":
                        walletActivities = await _context.WalletActivities
                            .Where(wa => wa.User != null && wa.User.Id == userId && wa.Timestamp.Date >= startOfCurrentWeek && wa.Timestamp.Date <= today.Date)
                            .Include(wa => wa.TransactionType)
                            .Include(wa => wa.Status)
                            .OrderByDescending(wa => wa.Timestamp)
                            .ToListAsync();
                        break;
                    case "monthly":
                        walletActivities = await _context.WalletActivities
                            .Where(wa => wa.User != null && wa.User.Id == userId && wa.Timestamp.Month == monthly && wa.Timestamp.Year == yearly)
                            .Include(wa => wa.TransactionType)
                            .Include(wa => wa.Status)
                            .OrderByDescending(wa => wa.Timestamp)
                            .ToListAsync();
                        break;
                    case "yearly":
                        walletActivities = await _context.WalletActivities
                            .Where(wa => wa.User != null && wa.User.Id == userId && wa.Timestamp.Year == yearly)
                            .Include(wa => wa.TransactionType)
                            .Include(wa => wa.Status)
                            .OrderByDescending(wa => wa.Timestamp)
                            .ToListAsync();
                        break;
                    default:
                        walletActivities = await _context.WalletActivities
                            .Where(wa => wa.User != null && wa.User.Id == userId)
                            .Include(wa => wa.TransactionType)
                            .Include(wa => wa.Status)
                            .OrderByDescending(wa => wa.Timestamp)
                            .ToListAsync();
                        break;
                }

                var result = walletActivities.Select(wa => new WalletActivityDto()
                {
                    Amount = wa.Amount,
                    TransactionType = wa.TransactionType?.Operation ?? "Unknown",
                    Status = wa.Status?.Status ?? "Unknown",
                    Timestamp = wa.Timestamp
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}