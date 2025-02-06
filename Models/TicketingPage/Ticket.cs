using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.TicketingPage
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TicketCategory Category { get; set; }
        public TravelCompany Company { get; set; }
        public DateTime PublishDate { get; set; }
    }
}