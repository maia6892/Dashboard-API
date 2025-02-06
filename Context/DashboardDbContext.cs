using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models;
using DashboardAPI.Models.BankingPage;
using DashboardAPI.Models.CryptocurrencyPage;
using DashboardAPI.Models.EducationPage;
using DashboardAPI.Models.HospitalManagementPage;
using DashboardAPI.Models.Invoicing;
using DashboardAPI.Models.LogisticsPage;
using DashboardAPI.Models.ManagementPage;
using DashboardAPI.Models.PointOfSalesPage;
using DashboardAPI.Models.RestaurantPage;
using DashboardAPI.Models.TicketingPage;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Context
{
    public class DashboardDbContext : DbContext
    {
        //  Logistics Page

        public DbSet<Order> Orders { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SideOfWorld> SidesOfWorld { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<DeliveryStatus> DeliveryStatuses { get; set; }
        public DbSet<Cost> Costs { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<CostService> CostServices { get; set; }
        public DbSet<FeeService> FeeServices { get; set; }


        //  Education Page

        public DbSet<SchoolDashboard> SchoolDashboards { get; set; }
        public DbSet<GeneralTask> Tasks { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<AbsenceStatus> AbsenceStatuses { get; set; }
        public DbSet<LunchChoice> LunchChoices { get; set; }
        public DbSet<ArrivalStatus> ArrivalStatuses { get; set; }
        public DbSet<TaskProgress> TaskProgresses { get; set; }

        //  Cryptocurrency Page

        public DbSet<Currency> Currencies { get; set; }
        public DbSet<WalletActivityStatus> WalletActivityStatuses { get; set; }
        public DbSet<SellBuyOrder> SellBuyOrders { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<WalletActivityOperation> WalletActivityOperations { get; set; }
        public DbSet<WalletActivity> WalletActivities { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeOrder> TradeOrders { get; set; }
        public DbSet<CryptoUser> CryptoUsers { get; set; }



        //  Invoicing Page

        public DbSet<InvoicingUser> InvoicingUsers { get; set; }
        public DbSet<SpendingList> SpendingLists { get; set; }
        public DbSet<SpendingListLimit> SpendingListLimits { get; set; }
        public DbSet<TransactionStatus> TransactionStatuses { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<InvoiceTransaction> InvoiceTransactions { get; set; }


        //  Restaurant Page


        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<RestaurantVisitor> RestaurantVisitors { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishOrder> DishOrders { get; set; }
        public DbSet<DishOrderDish> DishOrderDishes { get; set; }
        public DbSet<DishOrderStatus> DishOrderStatuses { get; set; }


        // Banking Page

        public DbSet<BankUser> BankUsers { get; set; }
        public DbSet<BankCard> BankCards { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<BankTransactionCategory> BankTransactionCategories { get; set; }
        public DbSet<BankTransactionStatus> BankTransactionStatuses { get; set; }
        public DbSet<BankUserContact> BankUserContacts { get; set; }


        // Management  Page


        public DbSet<EmployeeCompany> EmployeeCompanies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeActivity> EmployeeActivities { get; set; }
        public DbSet<EmployeeDashboard> EmployeeDashboards { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<EmployeeTeam> EmployeeTeams { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<ManagementEventsCalendar> ManagementEventsCalendars { get; set; }

        // Ticketing  Page

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketPurchase> TicketPurchases { get; set; }
        public DbSet<TravelCompanyCustomer> TravelCompanyCustomers { get; set; }
        public DbSet<TravelCompany> TravelCompanies { get; set; }


        // Point Of Sale  Page


        public DbSet<Market> Markets { get; set; }
        public DbSet<MarketActivity> MarketActivities { get; set; }
        public DbSet<MarketCategory> MarketCategories { get; set; }
        public DbSet<MarketCustomer> MarketCustomers { get; set; }
        public DbSet<MarketCustomerCity> MarketCustomerCities { get; set; }
        public DbSet<MarketCustomerCountry> MarketCustomerCountries { get; set; }
        public DbSet<MarketPackage> MarketPackages { get; set; }
        public DbSet<MarketPackageStatus> MarketPackageStatuses { get; set; }
        public DbSet<MarketProduct> MarketProducts { get; set; }
        public DbSet<MarketProductStatus> MarketProductStatuses { get; set; }
        public DbSet<SocialPlatform> SocialPlatforms { get; set; }


        // Hospital Management  Page


        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
        public DbSet<DoctorStatus> DoctorStatuses { get; set; }
        public DbSet<HospitalActivity> HospitalActivities { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Treatment> Treatments { get; set; }





        public DashboardDbContext(DbContextOptions<DashboardDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Logistics Page

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderStatus)
                .WithMany();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Country)
                .WithMany();

            modelBuilder.Entity<Shipment>()
                .HasOne(o => o.DeliveryStatus)
                .WithMany();

            modelBuilder.Entity<Country>()
                .HasOne(c => c.SideOfWorld);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Shipments)
                .WithOne(o => o.Orders)
                .HasForeignKey(o => o.OrderId);

            modelBuilder.Entity<CostService>()
                .HasOne(cs => cs.Costs)
                .WithMany();

            modelBuilder.Entity<FeeService>()
                .HasOne(fs => fs.Fees)
                .WithMany();




            //  Education Page

            modelBuilder.Entity<Student>()
                .HasMany(s => s.SchoolDashboards)
                .WithOne(sd => sd.Students)
                .HasForeignKey(sd => sd.StudentId);

            modelBuilder.Entity<GeneralTask>()
                .HasMany(gt => gt.Students)
                .WithMany(s => s.Tasks);

            modelBuilder.Entity<SchoolDashboard>()
                .HasOne(s => s.AbsenceStatuses)
                .WithMany();

            modelBuilder.Entity<SchoolDashboard>()
                .HasOne(s => s.LunchChoices)
                .WithMany();

            modelBuilder.Entity<SchoolDashboard>()
                .HasOne(s => s.ArrivalStatuses)
                .WithMany();

            modelBuilder.Entity<GeneralTask>()
                .HasOne(s => s.Progress)
                .WithMany();

            modelBuilder.Entity<GeneralTask>()
                .HasOne(s => s.Subject)
                .WithMany();

            modelBuilder.Entity<GeneralTask>()
                .HasOne(s => s.Grade)
                .WithMany();



            // Restaurant Page



            modelBuilder.Entity<DishOrder>()
                .Property(d => d.OrderDate)
                .HasConversion(
                    v => v.ToUniversalTime(), // Преобразование при сохранении в базу
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Устанавливает UTC при загрузке
                );

            modelBuilder.Entity<DishOrderDish>()
                .HasOne(dod => dod.DishOrder)
                .WithMany(dod => dod.DishOrderDishes)
                .HasForeignKey(dod => dod.DishOrderId);

            modelBuilder.Entity<DishOrderDish>()
                .HasOne(dod => dod.Dish)
                .WithMany(dod => dod.DishOrderDishes)
                .HasForeignKey(dod => dod.DishId);




            // Banking Page







        }
    }
}