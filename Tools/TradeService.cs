using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Context;
using DashboardAPI.DTOs.CryptocurrencyPage;
using DashboardAPI.Models.CryptocurrencyPage;

namespace DashboardAPI.Tools
{
    public class TradeService(DashboardDbContext context)
    {
        public TradeResponse CalculateAndSaveTrade(TradeRequest request, string currencyName, int userId = 1)
        {
            // 1. Найти валюту по ID
            var currency = context.Currencies.FirstOrDefault(c => c.Name == currencyName);
            if (currency == null)
            {
                throw new Exception($"Currency with Name '{currencyName}' not found.");
            }

            // 2. Найти тип ордера по имени
            var orderType = context.OrderTypes.FirstOrDefault(ot => ot.Type == request.OrderTypeName);
            if (orderType == null)
            {
                throw new Exception($"Order type with name '{request.OrderTypeName}' not found.");
            }

            // 3. Выполнить расчеты
            decimal totalWithoutFees = request.Amount * request.PricePerUnit;
            decimal feesAmount = totalWithoutFees * (request.FeesPercentage / 100);
            decimal totalWithFees = totalWithoutFees - feesAmount;

            // 4. Сохранить сделку в базу данных
            var trade = new Trade
            {
                CurrencyName = currency.Name,
                User = context.CryptoUsers.FirstOrDefault(u => u.Id == userId),
                Currency = currency,
                Amount = request.Amount,
                PricePerUnit = request.PricePerUnit,
                TotalWithoutFees = totalWithoutFees,
                FeesAmount = feesAmount,
                TotalWithFees = totalWithFees,
                OrderTypeName = orderType.Type,
                OrderType = orderType,
                Timestamp = DateTime.UtcNow
            };

            context.Trades.Add(trade);
            context.SaveChanges();

            // 5. Вернуть результат
            return new TradeResponse
            {
                CurrencyName = trade.CurrencyName,
                Amount = trade.Amount,
                PricePerUnit = trade.PricePerUnit,
                TotalWithoutFees = trade.TotalWithoutFees,
                FeesAmount = trade.FeesAmount,
                TotalWithFees = trade.TotalWithFees,
                OrderType = trade.OrderTypeName
            };
        }
    }
}