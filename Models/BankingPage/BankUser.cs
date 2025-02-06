using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.BankingPage
{
    public class BankUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<BankCard> Cards { get; set; }
        public ICollection<BankUserContact> Contacts { get; set; }
        public string Picture { get; set; }
    }
}