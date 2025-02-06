using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DashboardAPI.Models.LogisticsPage
{
    public class Shipment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
        public Order? Orders { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
    }
}