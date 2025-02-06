using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DashboardAPI.Models.EducationPage;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DashboardAPI.Models.LogisticsPage;
using DashboardAPI.Models.CryptocurrencyPage;
using System.Text.Json;
using RestSharp;
using DashboardAPI.Tools;
using DashboardAPI.Models.Invoicing;
using DashboardAPI.Models.RestaurantPage;
using DashboardAPI.Models.BankingPage;
using DashboardAPI.Models.TicketingPage;
using DashboardAPI.Models.PointOfSalesPage;
using DashboardAPI.Models.HospitalManagementPage;

namespace DashboardAPI.Context
{
    public class DbInitializer
    {
        public static async Task Seed(IServiceProvider serviceProvider)
        {
            using (var context = new DashboardDbContext(serviceProvider.GetRequiredService<DbContextOptions<DashboardDbContext>>()))
            {
                //   Logistics Page

                if (!context.DeliveryStatuses.Any())
                {
                    context.DeliveryStatuses.AddRange(
                        new DeliveryStatus { Status = "Delivered" },
                        new DeliveryStatus { Status = "On Progress" }
                    );
                    context.SaveChanges();
                }

                if (!context.SidesOfWorld.Any())
                {
                    context.SidesOfWorld.AddRange(
                        new SideOfWorld { Side = "East" },
                        new SideOfWorld { Side = "West" },
                        new SideOfWorld { Side = "North" },
                        new SideOfWorld { Side = "South" }
                    );
                    context.SaveChanges();
                }

                if (!context.OrderStatuses.Any())
                {
                    context.OrderStatuses.AddRange(
                        new OrderStatus { Status = "Pending" },
                        new OrderStatus { Status = "Canseled" },
                        new OrderStatus { Status = "Refunded" }
                    );
                    context.SaveChanges();
                }

                //   Education Page

                if (!context.AbsenceStatuses.Any())
                {
                    context.AbsenceStatuses.AddRange(
                        new AbsenceStatus { Status = "Sick" },
                        new AbsenceStatus { Status = "Unexcused" }
                    );

                    context.SaveChanges();
                }

                if (!context.LunchChoices.Any())
                {
                    context.LunchChoices.AddRange(
                        new LunchChoice { Choice = "Skipping Lunch" },
                        new LunchChoice { Choice = "School Lunch" },
                        new LunchChoice { Choice = "Lunch from Home" }
                    );

                    context.SaveChanges();
                }

                if (!context.ArrivalStatuses.Any())
                {
                    context.ArrivalStatuses.AddRange(
                        new ArrivalStatus { Status = "Late" },
                        new ArrivalStatus { Status = "On-Time" }
                    );

                    context.SaveChanges();
                }


                if (!context.TaskProgresses.Any())
                {
                    context.TaskProgresses.AddRange(
                        new TaskProgress { Progress = "Completed" },
                        new TaskProgress { Progress = "In Progress" },
                        new TaskProgress { Progress = "Pending" }
                    );

                    context.SaveChanges();
                }

                if (!context.Grades.Any())
                {
                    context.Grades.AddRange(
                        new Grade { Letter = "A" },
                        new Grade { Letter = "B" },
                        new Grade { Letter = "C" },
                        new Grade { Letter = "D" },
                        new Grade { Letter = "E" }
                    );

                    context.SaveChanges();
                }

                if (!context.Subjects.Any())
                {
                    context.Subjects.AddRange(
                        new Subject { Name = "Mathematics" },
                        new Subject { Name = "Literature" },
                        new Subject { Name = "History" },
                        new Subject { Name = "Geography" },
                        new Subject { Name = "Physics" },
                        new Subject { Name = "Chemistry" },
                        new Subject { Name = "Biology" },
                        new Subject { Name = "Foreign Language" },
                        new Subject { Name = "English" },
                        new Subject { Name = "Art" },
                        new Subject { Name = "Music" },
                        new Subject { Name = "Physical Education" },
                        new Subject { Name = "Technology" },
                        new Subject { Name = "Computer Science" },
                        new Subject { Name = "Civics" },
                        new Subject { Name = "Religious Studies" },
                        new Subject { Name = "Philosophy" },
                        new Subject { Name = "Economics" },
                        new Subject { Name = "Social Studies" },
                        new Subject { Name = "Health Education" }
                    );

                    context.SaveChanges();
                }


                //   Cryptocurrency Page


                if (!context.Currencies.Any())
                {
                    var desiredIds = new[] { "bitcoin", "ethereum", "litecoin", "ripple" };

                    var url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd";
                    var headers = new Dictionary<string, string> { { "accept", "application/json" } };
                    var jsonResponse = await Helper.GetJsonResponseAsync(url, headers);

                    var currencies = jsonResponse.RootElement
                        .EnumerateArray()
                        .Where(coin => desiredIds.Contains(coin.GetProperty("id").GetString()))
                        .Select(coin => new Currency
                        {
                            Name = coin.GetProperty("name").GetString(),
                            Logo = coin.GetProperty("image").GetString()
                        })
                        .ToList();

                    context.Currencies.AddRange(currencies);
                    await context.SaveChangesAsync();
                }

                if (!context.WalletActivityStatuses.Any())
                {
                    context.WalletActivityStatuses.AddRange(
                        new WalletActivityStatus { Status = "Completed" },
                        new WalletActivityStatus { Status = "Canceled" },
                        new WalletActivityStatus { Status = "Pending" }
                    );

                    context.SaveChanges();
                }

                if (!context.OrderTypes.Any())
                {
                    context.OrderTypes.AddRange(
                        new OrderType { Type = "Sell" },
                        new OrderType { Type = "Buy" }
                    );

                    context.SaveChanges();
                }

                if (!context.WalletActivityOperations.Any())
                {
                    context.WalletActivityOperations.AddRange(
                        new WalletActivityOperation { Operation = "Withdrawal" },
                        new WalletActivityOperation { Operation = "Top Up" }
                    );

                    context.SaveChanges();
                }



                // Invoicing Page


                if (!context.SpendingLists.Any())
                {
                    context.SpendingLists.AddRange(
                        new SpendingList { Name = "Investment" },
                        new SpendingList { Name = "Restaurant" },
                        new SpendingList { Name = "Installment" },
                        new SpendingList { Name = "Property" },
                        new SpendingList { Name = "Hobbies" },
                        new SpendingList { Name = "Travel" }
                    );

                    context.SaveChanges();
                }


                if (!context.TransactionStatuses.Any())
                {
                    context.TransactionStatuses.AddRange(
                        new TransactionStatus { Status = "Completed" },
                        new TransactionStatus { Status = "Canceled" },
                        new TransactionStatus { Status = "Pending" }
                    );

                    context.SaveChanges();
                }

                if (!context.TransactionTypes.Any())
                {
                    context.TransactionTypes.AddRange(
                        new TransactionType { Type = "Cashback" },
                        new TransactionType { Type = "Transfer In" },
                        new TransactionType { Type = "Transfer Out" }
                    );

                    context.SaveChanges();
                }


                //  Restaurant Page

                if (!context.DishOrderStatuses.Any())
                {
                    context.DishOrderStatuses.AddRange(
                        new DishOrderStatus { Status = "Delivered" },
                        new DishOrderStatus { Status = "Completed" },
                        new DishOrderStatus { Status = "On Process" },
                        new DishOrderStatus { Status = "New orders" },
                        new DishOrderStatus { Status = "Canceled" }
                    );

                    context.SaveChanges();
                }


                // Banking Page

                if (!context.BankTransactionStatuses.Any())
                {
                    context.BankTransactionStatuses.AddRange(
                        new BankTransactionStatus { Name = "Pending" },
                        new BankTransactionStatus { Name = "Completed" },
                        new BankTransactionStatus { Name = "Canceled" }
                    );

                    context.SaveChanges();
                }


                //  Ticketing  Page


                if (!context.TicketStatuses.Any())
                {
                    context.TicketStatuses.AddRange(
                        new TicketStatus { Name = "Paid" },
                        new TicketStatus { Name = "Pending" },
                        new TicketStatus { Name = "Refunded" }
                    );

                    context.SaveChanges();
                }

                if (!context.TicketCategories.Any())
                {
                    context.TicketCategories.AddRange(
                        new TicketCategory { Name = "VIP" },
                        new TicketCategory { Name = "Exclusive" },
                        new TicketCategory { Name = "Regular" },
                        new TicketCategory { Name = "Economy" }
                    );

                    context.SaveChanges();
                }


                //   Point of Sale  Page

                if (!context.MarketCategories.Any())
                {
                    context.MarketCategories.AddRange(
                        new MarketCategory { Name = "Clothing" },
                        new MarketCategory { Name = "Innnerwear" },
                        new MarketCategory { Name = "Footwear" },
                        new MarketCategory { Name = "Bags" },
                        new MarketCategory { Name = "Accessories" }
                    );

                    context.SaveChanges();
                }

                if (!context.SocialPlatforms.Any())
                {
                    context.SocialPlatforms.AddRange(
                        new SocialPlatform { Name = "Instagram" },
                        new SocialPlatform { Name = "Facebook" },
                        new SocialPlatform { Name = "Tiktok" },
                        new SocialPlatform { Name = "Amazon" }
                    );

                    context.SaveChanges();
                }

                if (!context.MarketProductStatuses.Any())
                {
                    context.MarketProductStatuses.AddRange(
                        new MarketProductStatus { Name = "Available" },
                        new MarketProductStatus { Name = "On Review" },
                        new MarketProductStatus { Name = "Out of Stock" }
                    );

                    context.SaveChanges();
                }

                if (!context.MarketPackageStatuses.Any())
                {
                    context.MarketPackageStatuses.AddRange(
                        new MarketPackageStatus { Name = "To be Packed" },
                        new MarketPackageStatus { Name = "To be Shipped" },
                        new MarketPackageStatus { Name = "To be Delivered" }
                    );

                    context.SaveChanges();
                }
                
                
                
                //   Hospital Management  Page


                if (!context.AppointmentStatuses.Any())
                {
                    context.AppointmentStatuses.AddRange(
                        new AppointmentStatus { Name = "Confirmed" },
                        new AppointmentStatus { Name = "Pending" },
                        new AppointmentStatus { Name = "Follow Up" }
                    );

                    context.SaveChanges();
                }


                if (!context.DoctorStatuses.Any())
                {
                    context.DoctorStatuses.AddRange(
                        new DoctorStatus { Name = "Available" },
                        new DoctorStatus { Name = "Unavailable" },
                        new DoctorStatus { Name = "Leave" }
                    );

                    context.SaveChanges();
                }






            }
        }
    }
}