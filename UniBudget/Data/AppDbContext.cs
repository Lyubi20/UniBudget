using Microsoft.EntityFrameworkCore;
using UniBudget.Models;

namespace UniBudget.Data
{
  
    public class AppDbContext : DbContext
    {
       
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BudgetSettings> BudgetSettings { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=unibudget.db");
        }
    }
}