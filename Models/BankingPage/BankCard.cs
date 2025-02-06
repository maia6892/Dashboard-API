using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.BankingPage
{
    public class BankCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public BankUser BankUser { get; set; }
    }
}