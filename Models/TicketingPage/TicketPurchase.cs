using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.LogisticsPage;

namespace DashboardAPI.Models.TicketingPage
{
    public class TicketPurchase
    {
        public int Id { get; set; }
        public Ticket Ticket { get; set; }
        public TravelCompanyCustomer Customer { get; set; }
        public decimal Amount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public TicketStatus Status { get; set; }
    }
}