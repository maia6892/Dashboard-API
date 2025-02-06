using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.Invoicing
{
    public class SpendingListLimit
    {
        public int Id { get; set; }
        public SpendingList SpendingList { get; set; }
        public int Limit { get; set; }
    }
}