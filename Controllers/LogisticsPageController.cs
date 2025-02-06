using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.LogisticsPage;
using DashboardAPI.Models.LogisticsPage;
using DashboardAPI.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace DashboardAPI.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class LogisticsPageController(DashboardDbContext context) : ControllerBase
{

    private static DateTime today = DateTime.UtcNow.Date;
    private static DateTime yesterday = today.AddDays(-1);
    private static int thisMonth = DateTime.UtcNow.Month;
    private static int lastMonth = thisMonth - 1;
    private static int year = today.Year;
    private static int lastYear = year - 1;

    [HttpGet]
    public async Task<IActionResult> QuickReview()
    {
        var revenue = Math.Truncate(context.Orders.Where(o => o.OrderDate.Month == thisMonth).Sum(o => o.Amount));
        var revenueLastMonth = Math.Truncate(context.Orders.Where(o => o.OrderDate.Month == lastMonth).Sum(o => o.Amount));
        var costs = await context.CostServices.Where(w => w.PaymentDate.Year == DateTime.UtcNow.Year).SumAsync(w => w.CostAmount);
        var costsLastMonth = await context.CostServices.Where(w => w.PaymentDate.Year == DateTime.UtcNow.Year - 1).SumAsync(w => w.CostAmount);
        var profits = revenue - costs;
        var profitsLastMonth = revenueLastMonth - costsLastMonth;
        var shipments = await context.Orders.Where(o => o.OrderDate.Month == thisMonth).CountAsync();
        var shipmentsLastMonth = context.Orders.Where(o => o.OrderDate.Month == lastMonth).Count();

        var quickReview = new QuickReviewDto()
        {
            Revenue = revenue,
            Costs = costs,
            Profits = profits,
            Shipments = shipments,
            RevenuePercentage = Helper.GetPercentageDifference(revenueLastMonth, revenue),
            CostsPercentage = Helper.GetPercentageDifference(costsLastMonth, costs),
            ProfitsPercentage = Helper.GetPercentageDifference(profitsLastMonth, profits),
            ShipmentsPercentage = Helper.GetPercentageDifference(shipmentsLastMonth, shipments)
        };
        return Ok(quickReview);
    }

    [HttpGet]
    public IActionResult YearlyOrderRate(int searchYear)
    {
        if (searchYear <= 0)
        {
            return BadRequest("The searchYear parameter must be a valid positive number.");
        }

        var startOfYear = new DateTime(searchYear, 1, 1);
        var endOfYear = new DateTime(searchYear, 12, 31);

        var year = Helper.GetMonthsOfYear(startOfYear);

        var yearlyOrderRate = year.Select(m => new YearlyOrderRateDto()
        {
            Month = m.Key,
            MonthOrderRate = context.Orders.Count(o => o.OrderDate.Month == m.Value.Month),
            WeeklyRates = Helper.GetWeeksOfMonth(m.Value).Select(week => new WeeklyOrderRateDto
            {
                WeekStart = week.Key,
                WeekOrderRate = context.Orders.Count(o => o.OrderDate >= week.Value.StartDate && o.OrderDate <= week.Value.EndDate)
            }).ToList()
        }).ToList();


        return Ok(yearlyOrderRate);
    }

    [HttpGet]
    public async Task<IActionResult> CarryingCosts()
    {
        var week = Helper.GetWeekDays(today, includeWeekends: true);

        var costs = await context.Orders.Where(o => o.OrderDate.Month == today.Month).SumAsync(o => o.Amount);
        var costsLastMonth = await context.Orders.Where(o => o.OrderDate.Month == today.Month - 1).SumAsync(o => o.Amount);

        var carryingCosts = new CarryingCostsDto
        {
            Costs = costs,
            CostsPercentage = Helper.GetPercentageDifference(costsLastMonth, costs),
            WeeklyCosts = week.Select(d => new CarryingCostsWeekDto
            {
                DayOfWeek = d.Key,
                Cost = context.Orders.Where(o => o.OrderDate.Date == d.Value.Date).Sum(o => o.Amount)
            }).ToList()

        };
        return Ok(carryingCosts);
    }

    [HttpGet]
    public async Task<IActionResult> DeliveryStatus()
    {
        var year = Helper.GetMonthsOfYear(today);

        var delivered = await context.Shipments.Where(s => s.DeliveryStatus.Status == "Delivered").Select(s => s.Orders).Where(o => o.OrderDate.Month == thisMonth).CountAsync();
        var deliveredLastMonth = await context.Shipments.Where(s => s.DeliveryStatus.Status == "Delivered").Select(s => s.Orders).Where(o => o.OrderDate.Month == thisMonth - 1).CountAsync();

        var onProgress = await context.Shipments.Where(s => s.DeliveryStatus.Status == "On Progress").Select(s => s.Orders).Where(o => o.OrderDate.Month == thisMonth).CountAsync();
        var onProgressLastMonth = await context.Shipments.Where(s => s.DeliveryStatus.Status == "On Progress").Select(s => s.Orders).Where(o => o.OrderDate.Month == thisMonth - 1).CountAsync();

        var deliveryStatus = new DeliveryStatusDto()
        {
            Delivered = delivered,
            DeliveredPercentage = Convert.ToInt32(Helper.GetPercentageDifference(deliveredLastMonth, delivered)),
            OnProgress = onProgress,
            OnProgressPercentage = Convert.ToInt32(Helper.GetPercentageDifference(onProgressLastMonth, onProgress)),
            DeliveryStatusMonths = year.Select(m => new DeliveryStatusMonthsDto()
            {
                Month = m.Key,
                DeliveredThisMonth = context.Shipments.Where(s => s.DeliveryStatus.Status == "Delivered").Select(s => s.Orders).Where(o => o.OrderDate.Month == m.Value.Month).Count(),
                OnProgressThisMonth = context.Shipments.Where(s => s.DeliveryStatus.Status == "On Progress").Select(s => s.Orders).Where(o => o.OrderDate.Month == m.Value.Month).Count()
            }).ToList()
        };

        return Ok(deliveryStatus);
    }

    [HttpGet]
    public IActionResult SalesByStoresLocation()
    {
        var year = Helper.GetMonthsOfYear(today);

        var sidesOfWorld = context.SidesOfWorld.ToList();

        var salesByStoresLocation = year.Select(m => new SalesByStoresLocationDto()
        {
            Month = m.Key,
            SalesByStoresLocationByMonth = sidesOfWorld.Select(s => new SalesByStoresLocationByMonthDto()
            {
                SidesOfWorld = s.Side,
                TotalOrders = context.Orders.Where(o => o.OrderDate.Month == m.Value.Month && o.Country.SideOfWorld.Id == s.Id).Count()
            }).ToList()
        }).ToList();

        return Ok(salesByStoresLocation);
    }

    [HttpGet]
    public async Task<IActionResult> DeliveriesByCounties()
    {
        var topCountries = await context.Orders
            .Where(o => o.Shipments != null)
            .GroupBy(o => o.Country)
            .Select(group => new
            {
                Country = group.Key,
                ShipmentCount = group.Sum(o => o.Shipments.Count)
            })
            .OrderByDescending(x => x.ShipmentCount)
            .Take(5)
            .ToListAsync();

        var totalCountries = context.Countries.Distinct().Count();


        var deliveriesByCounties = topCountries.Select(x => new DeliveriesByCountiesDto()
        {
            Country = x.Country.Name,
            DeliveryPercentage = Helper.GetPercentage(x.ShipmentCount, totalCountries)
        }).ToList();


        return Ok(deliveriesByCounties);
    }

    [HttpGet]
    public async Task<IActionResult> ShipmentReview()
    {
        var canceled = await context.Orders.Where(o => o.OrderStatus.Status == "Canceled" && o.OrderDate.Date == today).CountAsync();
        var canceledYesterday = await context.Orders.Where(o => o.OrderStatus.Status == "Canceled" && o.OrderDate.Date == yesterday).CountAsync();
        var delivered = await context.Shipments.Where(s => s.DeliveryStatus.Status == "Delivered" && s.Orders.OrderDate.Date == today).CountAsync();
        var deliveredYesterday = await context.Shipments.Where(s => s.DeliveryStatus.Status == "Delivered" && s.Orders.OrderDate.Date == yesterday).CountAsync();
        var orders = await context.Orders.Where(o => o.OrderDate.Date == today).CountAsync();
        var ordersYesterday = await context.Orders.Where(o => o.OrderDate.Date == yesterday).CountAsync();
        var refunded = await context.Orders.Where(o => o.OrderDate.Date == today && o.OrderStatus.Status == "Refunded").SumAsync(o => o.Amount);
        var refundedYesterday = await context.Orders.Where(o => o.OrderDate.Date == yesterday && o.OrderStatus.Status == "Refunded").SumAsync(o => o.Amount);
        var pending = await context.Orders.Where(o => o.OrderDate.Date == today && o.OrderStatus.Status == "Pending").CountAsync();
        var pendingYesterday = await context.Orders.Where(o => o.OrderDate.Date == yesterday && o.OrderStatus.Status == "Pending").CountAsync();
        var revenue = await context.Orders.Where(o => o.OrderDate.Date == today).SumAsync(o => o.Amount);
        var revenueYesterday = await context.Orders.Where(o => o.OrderDate.Date == yesterday).SumAsync(o => o.Amount);

        var shipmentReview = new ShipmentReviewDto()
        {
            Canceled = canceled,
            CanceledPercentage = Convert.ToInt32(Helper.GetPercentageDifference(canceledYesterday, canceled)),
            Delivered = delivered,
            DeliveredPercentage = Convert.ToInt32(Helper.GetPercentageDifference(deliveredYesterday, delivered)),
            Orders = orders,
            OrdersPercentage = Convert.ToInt32(Helper.GetPercentageDifference(ordersYesterday, orders)),
            Pending = Convert.ToInt32(pending),
            PendingPercentage = Convert.ToInt32(Helper.GetPercentageDifference(pendingYesterday, pending)),
            Revenue = Convert.ToInt32(revenue),
            RevenuePercentage = Convert.ToInt32(Helper.GetPercentageDifference(revenueYesterday, revenue)),
            Refunded = Convert.ToInt32(refunded),
            RefundedPercentage = Convert.ToInt32(Helper.GetPercentageDifference(refundedYesterday, refunded))
        };

        return Ok(shipmentReview);
    }

    [HttpGet]
    public async Task<IActionResult> WarehousingService()
    {
        var costServices = await context.CostServices
            .Include(cs => cs.Costs)
            .ToListAsync();

        var groupedCostServices = costServices
            .GroupBy(cs => cs.CostId)
            .Select(group => new
            {
                CostId = group.Key,
                CurrentYearValue = group
                    .Where(cs => cs.PaymentDate.Year == year)
                    .OrderByDescending(cs => cs.PaymentDate)
                    .Select(cs => cs.CostAmount)
                    .FirstOrDefault(),
                LastYearValue = group
                    .Where(cs => cs.PaymentDate.Year == lastYear)
                    .OrderByDescending(cs => cs.PaymentDate)
                    .Select(cs => cs.CostAmount)
                    .FirstOrDefault(),
                Name = group.FirstOrDefault()?.Costs?.Name ?? "Unknown"
            })
            .ToList();

        var feeServices = await context.FeeServices
            .Include(fs => fs.Fees)
            .ToListAsync();

        var groupedFeeServices = feeServices
            .GroupBy(fs => fs.FeeId)
            .Select(group => new
            {
                FeeId = group.Key,
                CurrentYearValue = group
                    .Where(fs => fs.PaymentDate.Year == year)
                    .OrderByDescending(fs => fs.PaymentDate)
                    .Select(fs => fs.FeePercentage)
                    .FirstOrDefault(),
                LastYearValue = group
                    .Where(fs => fs.PaymentDate.Year == lastYear)
                    .OrderByDescending(fs => fs.PaymentDate)
                    .Select(fs => fs.FeePercentage)
                    .FirstOrDefault(),
                Name = group.FirstOrDefault()?.Fees?.Name ?? "Unknown"
            })
            .ToList();

        var warehousingServices = new
        {
            Cost = groupedCostServices.Select(gcs => new WarehousingServiceDto()
            {
                ServiceName = gcs.Name,
                ServicePayment = gcs.CurrentYearValue,
                ServicePaymentPercentage = Helper.GetPercentageDifference(gcs.LastYearValue, gcs.CurrentYearValue)
            }).ToList(),
            Fee = groupedFeeServices.Select(gfs => new WarehousingServiceDto()
            {
                ServiceName = gfs.Name,
                ServicePayment = gfs.CurrentYearValue,
                ServicePaymentPercentage = Helper.GetPercentageDifference(gfs.LastYearValue, gfs.CurrentYearValue)
            }).ToList()
        };

        return Ok(warehousingServices);
    }

    [HttpGet]
    public async Task<IActionResult> Invoices()
    {
        var shipments = await context.Shipments
            .Include(s => s.Orders)
                .ThenInclude(o => o.Customer)
            .Where(s => s.DeliveryStatus.Status == "On Progress")
            .Take(5).Distinct()
            .ToListAsync();

        var invoices = shipments.Select(s => new InvoicesDto()
        {
            InvoiceId = s.InvoiceId,
            Customer = s.Orders.Customer.Name,
            InvoiceDate = s.Orders.OrderDate.Date,
            Amount = s.Orders.Amount
        }).ToList();

        return Ok(invoices);
    }

    [HttpDelete]
    public async Task<IActionResult> InvoicesDelete(int id)
    {
        var shipment = await context.Shipments.FirstOrDefaultAsync(s => s.InvoiceId == id);

        if (shipment == null)
        {
            return NotFound(new { Message = "Invoice not found." });
        }

        context.Shipments.Remove(shipment);

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> InvoicesEdit(InvoicesEditDto invoicesEdit, int id)
    {
        var findInvoice = await context.Shipments
            .Include(s => s.Orders)
            .ThenInclude(o => o.Customer)
            .FirstOrDefaultAsync(s => s.InvoiceId == id);

        if (findInvoice == null)
        {
            return NotFound(new { message = "Invoice not found." });
        }

        if (findInvoice.Orders == null)
        {
            return BadRequest(new { message = "Associated order not found for the invoice." });
        }

        if (!string.IsNullOrEmpty(invoicesEdit.Customer))
        {
            if (findInvoice.Orders.Customer == null)
            {
                return BadRequest(new { message = "Customer information is missing for the order." });
            }
            findInvoice.Orders.Customer.Name = invoicesEdit.Customer;
        }

        if (invoicesEdit.OrderDate != default && invoicesEdit.OrderDate != findInvoice.Orders.OrderDate)
        {
            findInvoice.Orders.OrderDate = invoicesEdit.OrderDate;
        }

        if (invoicesEdit.Amount != default)
        {
            findInvoice.Orders.Amount = invoicesEdit.Amount;
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

    [HttpPut]
    public async Task<IActionResult> InvoicesUpdateStatus(int id)
    {
        var findInvoice = await context.Shipments.FirstOrDefaultAsync(s => s.InvoiceId == id);
        var newStatus = await context.DeliveryStatuses.FirstOrDefaultAsync(ds => ds.Id == 1);

        findInvoice.DeliveryStatus = newStatus;
        await context.SaveChangesAsync();

        return Ok("updated");
    }

}



