using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.BankingPage
{
    public class BankUserContact
    {
        public int Id { get; set; }
        public int ContactUserId { get; set; }
        public BankUser User { get; set; }
        
    }
}