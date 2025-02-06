using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.BankingPage;
using DashboardAPI.Models.BankingPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BankingPageController(DashboardDbContext context) : ControllerBase
    {
        private readonly DateTime today = DateTime.UtcNow.Date;
        private readonly int thisYear = DateTime.UtcNow.Year;
        private readonly int thisMonth = DateTime.UtcNow.Month;
        private readonly int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;

        [HttpGet]
        public async Task<IActionResult> GetCards(int userId = 1)
        {
            var cards = await context.BankCards
                .Where(card => card.BankUser.Id == userId)
                .ToListAsync();

            var transactions = await context.BankTransactions
                .Include(t => t.Card)
                .Where(t => t.User.Id == userId)
                .ToListAsync();

            var result = cards.Select(card => new GetCardsDto
            {
                CardNumber = card.CardNumber,
                CardBalance = transactions
                    .Where(t => t.Card.Id == card.Id)
                    .Sum(t => t.IsIncome ? t.Amount : -t.Amount)
            });

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> IncomeExpenseStatistics(int userId = 1)
        {
            var monthOfYear = Helper.GetMonthsOfYear(DateTime.UtcNow);

            var transactions = await context.BankTransactions
                .Include(t => t.Card)
                .Where(t => t.User.Id == userId)
                .ToListAsync();

            var income = transactions
                .Where(t => t.TransactionDate.Month == thisMonth && t.IsIncome)
                .Sum(t => t.Amount);
            var expense = transactions
                .Where(t => t.TransactionDate.Month == thisMonth && !t.IsIncome)
                .Sum(t => t.Amount);
            var incomeLastMonth = transactions
                .Where(t => t.TransactionDate.Month == lastMonth && t.IsIncome)
                .Sum(t => t.Amount);
            var expenseLastMonth = transactions
                .Where(t => t.TransactionDate.Month == lastMonth && !t.IsIncome)
                .Sum(t => t.Amount);

            var result = new
            {
                Income = income,
                IncomePercentage = Helper.GetPercentageDifference(incomeLastMonth, income),
                Expense = expenseLastMonth,
                ExpensePercentage = Helper.GetPercentageDifference(expenseLastMonth, expense),
                MonthOfYear = monthOfYear.Select(m => new IncomeExpenseStatisticsDto
                {
                    Month = m.Key,
                    Date = m.Value.ToString("MMM yyyy", new System.Globalization.CultureInfo("en-US")),
                    Income = transactions
                        .Where(t => t.TransactionDate.Year == thisYear && t.TransactionDate.Month == m.Value.Month && t.IsIncome)
                        .Sum(t => t.Amount),
                    Expense = transactions
                        .Where(t => t.TransactionDate.Year == thisYear && t.TransactionDate.Month == m.Value.Month && !t.IsIncome)
                        .Sum(t => t.Amount)
                }).ToList()
            };

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> SendPaymentGet(int userId = 1)
        {
            var contacts = context.BankUserContacts.Where(c => c.User.Id == userId).ToList();
            var response = new
            {
                Contacts = contacts.Select(c => new
                {
                    ContactName = context.BankUsers.Where(u => u.Id == c.Id).Select(u => u.Name),
                    ContactPicture = context.BankUsers.Where(u => u.Id == c.Id).Select(u => u.Picture)
                }),
                Category = context.BankTransactionCategories.Select(c => c.Name)
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> SendPaymentPost([FromBody] SendPaymentDto request, int userId = 1)
        {
            if (request == null || string.IsNullOrEmpty(request.Recipient))
            {
                return BadRequest("Recipient is required.");
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Transfer amount must be greater than zero.");
            }

            var balance = context.BankTransactions
                .Where(t => t.User.Id == userId)
                .Sum(t => t.IsIncome ? t.Amount : -t.Amount);

            if (request.Amount >= balance)
            {
                return BadRequest("Insufficient balance for this transfer.");
            }

            var transaction = new BankTransaction
            {
                Name = request.Recipient,
                User = await context.BankUsers.FirstOrDefaultAsync(u => u.Id == userId) ?? new BankUser(),
                Card = await context.BankCards.FirstOrDefaultAsync(c => c.CardNumber == context.BankUsers.FirstOrDefault(u => u.Id == userId).Cards.FirstOrDefault().CardNumber) ?? new BankCard(),
                Category = await context.BankTransactionCategories.FirstOrDefaultAsync(c => c.Name == request.Category) ?? new BankTransactionCategory(),
                IsIncome = false,
                Amount = request.Amount,
                Status = await context.BankTransactionStatuses.FirstOrDefaultAsync(s => s.Name == "Completed") ?? new BankTransactionStatus(),
                TransactionDate = DateTime.UtcNow
            };

            context.BankTransactions.Add(transaction);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> PreviousTransactions(string timestamp, int userId = 1)
        {
            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + 1);

            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfCurrentWeek = today.AddDays(-6);
            }

            var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfYear = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var previousTransactions = new List<BankTransaction>();

            switch (timestamp.ToLower())
            {
                case "today":
                    previousTransactions = await context.BankTransactions
                        .Where(i => i.User != null && i.User.Id == userId && i.TransactionDate.Date == today)
                        .Include(i => i.Category)
                        .Include(i => i.Card)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;

                case "weekly":
                    previousTransactions = await context.BankTransactions
                        .Where(i => i.User != null && i.User.Id == userId &&
                                    i.TransactionDate >= startOfCurrentWeek && i.TransactionDate.Date <= today)
                        .Include(i => i.Category)
                        .Include(i => i.Card)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;

                case "monthly":
                    previousTransactions = await context.BankTransactions
                        .Where(i => i.User != null && i.User.Id == userId &&
                                    i.TransactionDate >= startOfMonth && i.TransactionDate.Date <= today)
                        .Include(i => i.Category)
                        .Include(i => i.Card)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;

                case "yearly":
                    previousTransactions = await context.BankTransactions
                        .Where(i => i.User != null && i.User.Id == userId &&
                                    i.TransactionDate >= startOfYear && i.TransactionDate.Date <= today)
                        .Include(i => i.Category)
                        .Include(i => i.Card)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;

                default:
                    return BadRequest("Invalid timestamp parameter.");
            }

            var result = previousTransactions.Select(i => new PreviousTransactionsDto
            {
                Id = i.Id,
                Name = i.Name,
                Date = i.TransactionDate.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US")),
                Time = i.TransactionDate.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")),
                Category = i.Category.Name,
                Amount = i.Amount,
                Status = i.Status.Name,
                IsIncome = i.IsIncome
            });

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> PreviousTransactionsView(int transactionId)
        {
            var transaction = await context.BankTransactions
                .Where(i => i.Id == transactionId)
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Card)
                .Include(i => i.Status)
                .FirstOrDefaultAsync();

            var result = new PreviousTransactionsDto
            {
                Id = transaction?.Id ?? 0,
                Name = transaction?.Name ?? "Not found",
                Date = transaction?.TransactionDate.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US")) ?? "Not found",
                Time = transaction?.TransactionDate.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")) ?? "Not found",
                Category = transaction?.Category?.Name ?? "Not found",
                Amount = transaction?.Amount ?? 0,
                Status = transaction?.Status?.Name ?? "Not found",
                IsIncome = transaction?.IsIncome ?? false
            };

            return Ok(result);
        }


        [HttpPut]
        public async Task<IActionResult> PreviousTransactionsEdit(PreviousTransactionsDto transactionDto, int transactionId)
        {
            var findTransaction = await context.BankTransactions
                .Where(i => i.Id == transactionId)
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Card)
                .Include(i => i.Status)
                .FirstOrDefaultAsync();

            if (findTransaction == null)
            {
                return NotFound(new { message = "Transaction not found." });
            }

            if (findTransaction.Category == null)
            {
                return BadRequest(new { message = "Associated type not found for the transaction." });
            }
            if (findTransaction.Status == null)
            {
                return BadRequest(new { message = "Associated status not found for the transaction." });
            }

            if (!string.IsNullOrEmpty(transactionDto.Name))
            {
                if (findTransaction.Name == null)
                {
                    return BadRequest(new { message = "Transaction name information is missing for the transaction." });
                }
                findTransaction.Name = transactionDto.Name;
            }

            if (transactionDto.Date != null &&
                DateTime.TryParseExact(
                    transactionDto.Date,
                    "MMMM dd, yyyy",
                    new System.Globalization.CultureInfo("en-US"),
                    System.Globalization.DateTimeStyles.None,
                    out var parsedDate))
            {
                findTransaction.TransactionDate = DateTime.SpecifyKind(new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day), DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(transactionDto.Time) &&
                DateTime.TryParseExact(
                    transactionDto.Time,
                    "hh:mm tt",
                    new System.Globalization.CultureInfo("en-US"),
                    System.Globalization.DateTimeStyles.None,
                    out var parsedTime))
            {
                findTransaction.TransactionDate = findTransaction.TransactionDate.Date.Add(parsedTime.TimeOfDay);
            }

            if (transactionDto.Amount != default)
            {
                findTransaction.Amount = transactionDto.Amount;
            }

            if (transactionDto.Category != default)
            {
                findTransaction.Category.Name = transactionDto.Category;
            }

            if (transactionDto.Status != default)
            {
                findTransaction.Status.Name = transactionDto.Status;
            }

            try
            {
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the transaction.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SpendingCategories(int userId = 1)
        {
            var categories = await context.BankTransactionCategories
                .Take(4)
                .ToListAsync();

            var spendingCategories = categories.Select(c => new SpendingCategoriesDto
            {
                Category = c.Name,
                Amount = context.BankTransactions
                    .Where(t => t.User.Id == userId && t.Category.Id == c.Id && !t.IsIncome && t.TransactionDate.Month == DateTime.Now.Month)
                    .Sum(t => t.Amount),
                Limit = c.Limit
            });

            return Ok(spendingCategories);
        }

        [HttpGet]
        public async Task<IActionResult> MonthlyStatements(int userId = 1)
        {
            var categories = await context.BankTransactionCategories
                .Take(5)
                .ToListAsync();
            
            decimal totalSpends = 0;

            foreach (var category in categories)
            {
                totalSpends += context.BankTransactions
                    .Where(t => t.User.Id == userId && t.Category.Id == category.Id && !t.IsIncome && t.TransactionDate.Month == DateTime.Now.Month && t.Category == category)
                    .Sum(t => t.Amount);
            }

            var monthlyStatements = categories.Select(c => new MonthlyStatementsDto
            {
                Category = c.Name,
                Percentage = Helper.GetPercentage(context.BankTransactions
                    .Where(t => t.User.Id == userId && t.Category.Id == c.Id && !t.IsIncome && t.TransactionDate.Month == DateTime.Now.Month)
                    .Sum(t => t.Amount), totalSpends)
            });

            return Ok(monthlyStatements);
        }

        [HttpGet]
        public async Task<IActionResult> TransactionHistory(int userId = 1)
        {
            var transactions = await context.BankTransactions
                .Where(t => t.User.Id == userId && t.TransactionDate.Date == today)
                .Include(t => t.Category)
                .Include(t => t.Card)
                .Include(t => t.Status)
                .ToListAsync();

            var transactionHistory = transactions.Select(t => new PreviousTransactionsDto
            {
                Id = t?.Id ?? 0,
                Name = t?.Name ?? "Not found",
                Date = t?.TransactionDate.ToString("dd MMM yyyy", new System.Globalization.CultureInfo("en-US")) ?? "Not found",
                Time = t?.TransactionDate.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")) ?? "Not found",
                Category = t?.Category?.Name ?? "Not found",
                Amount = t?.Amount ?? 0,
                Status = t?.Status?.Name ?? "Not found",
                IsIncome = t?.IsIncome ?? false
            });

            return Ok(transactionHistory);
        }
    }
}