using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.CryptocurrencyPage;
using DashboardAPI.DTOs.InvoicingPage;
using DashboardAPI.Models.Invoicing;
using DashboardAPI.Services;
using DashboardAPI.Services.Interfaces;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class InvoicingPageController(DashboardDbContext context, IBalanceCalculator balanceCalculator) : ControllerBase
    {
        private int thisMonth = DateTime.UtcNow.Month;
        private int lastMonth = DateTime.UtcNow.AddMonths(-1).Month;
        [HttpGet]
        public async Task<IActionResult> QuickReview(int userId = 1)
        {
            var invoices = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == thisMonth).CountAsync();
            var paidInvoices = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == thisMonth && i.Status.Status == "Completed" && i.Type.Type == "Transfer Out").CountAsync();
            var unpaidInvoices =await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == thisMonth && i.Status.Status == "Canceled" && i.Type.Type == "Transfer Out").CountAsync();
            var invoiceSent =await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == thisMonth && i.Type.Type == "Transfer Out").CountAsync();

            var invoicesLastMonth =await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == lastMonth).CountAsync();
            var paidInvoicesLastMonth =await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == lastMonth && i.Status.Status == "Completed" && i.Type.Type == "Transfer Out").CountAsync();
            var unpaidInvoicesLastMonth =await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == lastMonth && i.Status.Status == "Canceled" && i.Type.Type == "Transfer Out").CountAsync();
            var invoiceSentLastMonth =await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == lastMonth && i.Type.Type == "Transfer Out").CountAsync();

            var quickReview = new QuickReviewDto()
            {
                TotalInvoices = invoices,
                TotalInvoicesPercentage = Helper.GetPercentageDifference(invoicesLastMonth, invoices),
                PaidInvoices = paidInvoices,
                PaidInvoicesPercentage = Helper.GetPercentageDifference(paidInvoicesLastMonth, paidInvoices),
                UnpaidInvoices = unpaidInvoices,
                UnpaidInvoicesPercentage = Helper.GetPercentageDifference(unpaidInvoicesLastMonth, unpaidInvoices),
                InvoiceSent = invoiceSent,
                InvoiceSentPercentage = Helper.GetPercentageDifference(invoiceSentLastMonth, invoiceSent)
            };

            return Ok(quickReview);
        }

        [HttpGet]
        public IActionResult SpendingAndSpendingLists(int userId = 1)
        {
            var spending = context.SpendingListLimits.Select(s => new SpendingDto()
            {
                Spending = s.SpendingList.Name,
                Amount = context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == thisMonth && i.Type.Type == "Transfer Out" && i.SpendingList.Name == s.SpendingList.Name).Sum(i => i.Amount),
                Limit = s.Limit,
                SpendingPercentage = Helper.GetPercentage(context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == thisMonth && i.Type.Type == "Transfer Out" && i.SpendingList.Name == s.SpendingList.Name).Sum(i => i.Amount), s.Limit)
            });

            return Ok(spending);
        }


        [HttpGet]
        public IActionResult CalculateBalance(int userId = 1)
        {

            var balance = balanceCalculator.CalculateBalance(userId);

            return Ok(balance);
        }

        [HttpPost]
        public async Task<IActionResult> QuickTransfer([FromBody] QuickTransferDto transferDto, int userId = 1)
        {
            if (transferDto == null || string.IsNullOrEmpty(transferDto.Recipient))
            {
                return BadRequest("Recipient is required.");
            }

            if (transferDto.Amount <= 0)
            {
                return BadRequest("Transfer amount must be greater than zero.");
            }
            var balance = balanceCalculator.CalculateBalance(userId);
            if (transferDto.Amount >= balance)
            {
                return BadRequest("Insufficient balance for this transfer.");
            }

            var transaction = new InvoiceTransaction
            {
                User = await context.InvoicingUsers.FirstOrDefaultAsync(u => u.Id == userId) ?? new InvoicingUser(),
                Name = transferDto.Recipient,
                Type = await context.TransactionTypes.FirstOrDefaultAsync(t => t.Type == "Transfer Out") ?? new TransactionType(),
                Amount = transferDto.Amount,
                Status = await context.TransactionStatuses.FirstOrDefaultAsync(s => s.Status == "Completed") ?? new TransactionStatus(),
                SpendingList = await context.SpendingLists.FirstOrDefaultAsync(s => s.Name == "Hobbies") ?? new SpendingList(),
                TransactionDate = DateTime.UtcNow
            };

            context.InvoiceTransactions.Add(transaction);
            await context.SaveChangesAsync();

            balance = balanceCalculator.CalculateBalance(userId);

            return Ok();
        }

        [HttpGet]
        public IActionResult TransactionOverview(int userId = 1)
        {
            var year = Helper.GetMonthsOfYear(DateTime.UtcNow);

            var transactionOverview = year.Select(m => new TransactionOverviewDto()
            {
                Month = m.Key,
                Income = context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == m.Value.Month && (i.Type.Type == "Transfer In" || i.Type.Type == "Cashback")).Sum(i => i.Amount),
                Expences = context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Month == m.Value.Month && i.Type.Type == "Transfer Out").Sum(i => i.Amount)
            }).ToList();

            return Ok(transactionOverview);
        }

        [HttpGet]
        public IActionResult WeeklyWalletTransactions(string month, int year, int userId = 1)
        {

            if (!DateTime.TryParseExact(month, "MMMM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedMonth))
            {
                return BadRequest("Invalid month name. Use full month name in English, e.g., 'November'.");
            }

            var startDate = new DateTime(year, parsedMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var thisWeekStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var thisWeekEnd = startDate.AddMonths(1).AddDays(-1);

            var weeklyTransactions = balanceCalculator.CalculateBalance(userId, startDate, endDate);

            var week = Helper.GetWeekDays(parsedMonth, includeWeekends: true);
            var thisWeek = Helper.GetWeekDays(thisWeekStart, includeWeekends: true);

            var thisWeekTransactions = balanceCalculator.CalculateBalance(userId, thisWeekStart, thisWeekEnd);

            var res = new
            {
                TransactionsAmount = weeklyTransactions,
                TransactionsAmountPercentage = Helper.GetPercentageDifference(weeklyTransactions, thisWeekTransactions),
                week = week.Select(w => new WeeklyWalletTransactionsDto()
                {
                    Date = w.Value,
                    Day = w.Key,
                    Amount = context.InvoiceTransactions.Where(i => i.User.Id == userId && i.TransactionDate.Year == year && i.TransactionDate.Month == parsedMonth.Month && i.TransactionDate.Day == w.Value.Day).Sum(i => i.Amount),
                }).ToList()
            };

            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> PreviousTransactions(string timestamp, int userId = 1)
        {
            var today = DateTime.UtcNow;
            var monthly = today.Month;
            var yearly = today.Year;

            var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            var previousTransactions = new List<InvoiceTransaction>();

            switch (timestamp.ToLower())
            {
                case "today":
                    previousTransactions = await context.InvoiceTransactions
                        .Where(i => i.User != null && i.User.Id == userId && i.TransactionDate.Date == today.Date)
                        .Include(i => i.Type)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();

                    break;
                case "weekly":
                    previousTransactions = await context.InvoiceTransactions
                        .Where(i => i.User != null && i.User.Id == userId && i.TransactionDate.Date >= startOfCurrentWeek && i.TransactionDate.Date <= today.Date)
                        .Include(i => i.Type)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;
                case "monthly":
                    previousTransactions = await context.InvoiceTransactions
                        .Where(i => i.User != null && i.User.Id == userId && i.TransactionDate.Month == monthly)
                        .Include(i => i.Type)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;
                case "yearly":
                    previousTransactions = await context.InvoiceTransactions
                        .Where(i => i.User != null && i.User.Id == userId && i.TransactionDate.Year == yearly)
                        .Include(i => i.Type)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;
                default:
                    previousTransactions = await context.InvoiceTransactions
                        .Where(i => i.User != null && i.User.Id == userId)
                        .Include(i => i.Type)
                        .Include(i => i.Status)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToListAsync();
                    break;
            }
            var result = previousTransactions.Select(pt => new PreviousTransactionsDto()
            {
                Id = pt.Id,
                TransactionName = pt.Name,
                Date = pt.TransactionDate,
                TransactionType = pt.Type.Type,
                Amount = pt.Amount,
                Status = pt.Status.Status
            }).ToList();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> PreviousTransactionsView(int transactionId, int userId = 1)
        {
            var transaction = new PreviousTransactionsDto()
            {
                Id = transactionId,
                TransactionName = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.Id == transactionId).Select(i => i.Name).FirstOrDefaultAsync() ?? "Unknown",
                Date = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.Id == transactionId).Select(i => i.TransactionDate).FirstOrDefaultAsync(),
                TransactionType = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.Id == transactionId).Select(i => i.Type.Type).FirstOrDefaultAsync() ?? "Unknown",
                Amount = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.Id == transactionId).Select(i => i.Amount).FirstOrDefaultAsync(),
                Status = await context.InvoiceTransactions.Where(i => i.User.Id == userId && i.Id == transactionId).Select(i => i.Status.Status).FirstOrDefaultAsync() ?? "Unknown"
            };
            return Ok(transaction);
        }

        [HttpPut]
        public async Task<IActionResult> PreviousTransactionsEdit([FromQuery] PreviousTransactionsDto transaction, int transactionId, int userId = 1)
        {
            var findTransaction = await context.InvoiceTransactions
            .Include(i => i.User)
            .Include(i => i.Type)
            .Include(i => i.Status)
            .FirstOrDefaultAsync(i => i.User.Id == userId && i.Id == transactionId);

            if (findTransaction == null)
            {
                return NotFound(new { message = "Transaction not found." });
            }

            if (findTransaction.Type == null)
            {
                return BadRequest(new { message = "Associated type not found for the transaction." });
            }
            if (findTransaction.Status == null)
            {
                return BadRequest(new { message = "Associated status not found for the transaction." });
            }

            if (!string.IsNullOrEmpty(transaction.TransactionName))
            {
                if (findTransaction.Name == null)
                {
                    return BadRequest(new { message = "Transaction name information is missing for the transaction." });
                }
                findTransaction.Name = transaction.TransactionName;
            }

            if (transaction.Date != default && transaction.Date != findTransaction.TransactionDate)
            {
                findTransaction.TransactionDate = transaction.Date;
            }

            if (transaction.Amount != default)
            {
                findTransaction.Amount = transaction.Amount;
            }
            
            if (transaction.TransactionType != default)
            {
                findTransaction.Type.Type = transaction.TransactionType;
            }
            
            if (transaction.Status != default)
            {
                findTransaction.Status.Status = transaction.Status;
            }

            try
            {
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the invoice.", error = ex.Message });
            }
        }
    }
}