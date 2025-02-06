using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.TicketingPage
{
    public class TravelCompany
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TravelCompanyCustomer> Customers { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }
}