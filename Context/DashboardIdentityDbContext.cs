using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Models.Account;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Context
{
    public class DashboardIdentityDbContext : IdentityDbContext<User>
    {
        public DashboardIdentityDbContext(DbContextOptions<DashboardIdentityDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}