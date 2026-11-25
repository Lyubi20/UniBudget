using System;

namespace UniBudget.Models
{
    public class BudgetSettings
    {
        public int Id { get; set; }
        public decimal MonthlyBudget { get; set; }
        public decimal FixedExpenses { get; set; }
        public DateTime NextIncomeDate { get; set; }
    }
}