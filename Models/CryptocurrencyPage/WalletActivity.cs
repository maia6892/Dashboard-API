using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models.CryptocurrencyPage
{
    public class WalletActivity
    {
        public int Id { get; set; }
        public CryptoUser User { get; set; }
        public WalletActivityOperation TransactionType { get; set; }
        public decimal Amount { get; set; }
        public WalletActivityStatus Status { get; set; }
        public bool IsInstallment { get; set; }
        public DateTime Timestamp { get; set; }
    }
}