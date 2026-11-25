using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using UniBudget.Data;
using UniBudget.Models;

namespace UniBudget
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _db = new AppDbContext();
        private List<Transaction> _currentList = new List<Transaction>();

        public MainWindow()
        {
            InitializeComponent();
            _db.Database.EnsureCreated();
            int payDay = 22;
            var nextDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, payDay);
            if (DateTime.Today >= nextDate) nextDate = nextDate.AddMonths(1);

            var s = _db.BudgetSettings.FirstOrDefault();
            if (s == null) { s = new BudgetSettings(); _db.BudgetSettings.Add(s); }
            s.MonthlyBudget = 900;
            s.FixedExpenses = 400;
            s.NextIncomeDate = nextDate;
            _db.SaveChanges();

            UpdateUI();
        }

        private void UpdateUI()
        {
            _currentList = _db.Transactions.OrderByDescending(t => t.Date).ToList();
            lstExpenses.ItemsSource = _currentList.Select(t =>
                $"{t.Date:dd.MM} | {t.Category}{(string.IsNullOrWhiteSpace(t.Description) ? "" : $" ({t.Description})")} — {(t.Type == "Приход" ? "+" : "-")}{t.Amount:F2} €");

            var s = _db.BudgetSettings.First();
            decimal extraIncome = _currentList.Where(t => t.Type == "Приход").Sum(t => t.Amount);
            decimal expenses = _currentList.Where(t => t.Type != "Приход").Sum(t => t.Amount);
            decimal totalStart = (s.MonthlyBudget - s.FixedExpenses) + extraIncome;
            decimal free = totalStart - expenses;

            int days = Math.Max(1, (s.NextIncomeDate - DateTime.Today).Days);
            decimal daily = free > 0 ? free / days : 0;

            lblBudget.Text = $"Свободен бюджет: {free:F2} € (Фиксирани: {s.FixedExpenses:F0} €)";
            lblDaily.Text = $"Дневен лимит: {daily:F2} € / ден (остават {days} дни)";
            pbBudget.Value = totalStart > 0 && free > 0 ? Math.Min(100, (double)(free / totalStart * 100)) : 0;

            int tripsHome = free > 0 ? (int)(free / 15m) : 0;
            decimal eduSpent = _currentList.Where(t => t.Category.Contains("Образование")).Sum(t => t.Amount);
            lblStudentStats.Text = $"Пътувания до вкъщи (~15 €): {tripsHome} бр.   |   Инвестирано в семестъра: {eduSpent:F2} €";

            if (daily >= 12) { txtStatus.Text = "Спокоен"; statusBadge.Background = pbBudget.Foreground = Brushes.Green; }
            else if (daily >= 4) { txtStatus.Text = "Оцеляващ"; statusBadge.Background = pbBudget.Foreground = Brushes.Orange; }
            else { txtStatus.Text = "Критично"; statusBadge.Background = pbBudget.Foreground = Brushes.Red; }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Въведете валидна сума по-голяма от 0!");
                return;
            }
            bool isIncome = cmbCategory.Text.Contains("(+)");
            _db.Transactions.Add(new Transaction { Amount = amount, Type = isIncome ? "Приход" : "Разход", Category = cmbCategory.Text, Description = txtDesc.Text.Trim(), Date = DateTime.Now });
            _db.SaveChanges();
            txtAmount.Clear(); txtDesc.Clear(); txtAmount.Focus();
            UpdateUI();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstExpenses.SelectedIndex < 0) { MessageBox.Show("Изберете запис от списъка!"); return; }
            _db.Transactions.Remove(_currentList[lstExpenses.SelectedIndex]);
            _db.SaveChanges();
            UpdateUI();
        }
    }
}