using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.DTOs.BankingPage
{
    public class GetCardsDto
    {
        public decimal CardBalance { get; set; }
        public string CardNumber { get; set; }
    }
}