using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.TicketingPage
{
    public class TravelCompanyCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public TravelCompany TravelCompany { get; set; }
    }
}