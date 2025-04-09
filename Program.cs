using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalBudgetTracker
{
   public enum TransactionType
{
    Income,
    Expense
}
 public class Transaction
 {
     public string Description { get; set; }
     public decimal Amount { get; set; }
     public TransactionType Type { get; set; }
     public string Category { get; set; }
     public DateTime Date { get; set; }
     public Guid Id { get; private set; }
public Transaction(string description, decimal amount, TransactionType type, string category, DateTime date)
{
    if (string.IsNullOrWhiteSpace(description))
        throw new ArgumentException("Description cannot be empty", nameof(description));

    if (amount <= 0)
        throw new ArgumentException("Amount must be greater than zero", nameof(amount));

    if (string.IsNullOrWhiteSpace(category))
        throw new ArgumentException("Category cannot be empty", nameof(category));

    Description = description;
    Amount = amount;
    Type = type;
    Category = category;
    Date = date;
    Id = Guid.NewGuid();
}
         public Transaction()
    {
        Id = Guid.NewGuid();
    }

    public override string ToString()
    {
        string typeSymbol = Type == TransactionType.Income ? "+" : "-";
        return $"{Date.ToShortDateString()} | {typeSymbol}{Amount:C} | {Category} | {Description}";
    }
}
     public class BudgetTracker
 {
     public List<Transaction> Transactions { get; private set; }

     public BudgetTracker()
     {
         Transactions = new List<Transaction>();
     }
     public void AddTransaction(Transaction transaction)
   {
     if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            Transactions.Add(transaction);
        }
      public decimal CalculateTotalIncome()
        {
         return Transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
        }
public decimal CalculateTotalExpenses()
        {
return Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
        }
public decimal CalculateNetSavings()
        {
return CalculateTotalIncome() - CalculateTotalExpenses();
        }
public List<Transaction> GetTransactionsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty", nameof(category));
   return Transactions
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
public decimal GetTotalByCategory(string category, TransactionType type)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty", nameof(category));

            return Transactions
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && t.Type == type)
                .Sum(t => t.Amount);
        }
       public List<string> GetAllCategories()
        {
            return Transactions
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
 public Dictionary<string, decimal> GetCategoryBreakdown(TransactionType type)
        {
            return Transactions
                .Where(t => t.Type == type)
                .GroupBy(t => t.Category)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(t => t.Amount)
                );
        }
 public (string Category, decimal Amount) GetHighestSpendingCategory()
        {
            var expensesByCategory = GetCategoryBreakdown(TransactionType.Expense);

            if (expensesByCategory.Count == 0)
                return ("None", 0);

            var highestCategory = expensesByCategory
                .OrderByDescending(kvp => kvp.Value)
                .First();

            return (highestCategory.Key, highestCategory.Value);
        }
 public List<Transaction> SortTransactionsByDate(bool ascending = true)
        {
            return ascending
                ? Transactions.OrderBy(t => t.Date).ToList()
                : Transactions.OrderByDescending(t => t.Date).ToList();
        }
public List<Transaction> SortTransactionsByAmount(bool ascending = true)
        {
            return ascending
                ? Transactions.OrderBy(t => t.Amount).ToList()
                : Transactions.OrderByDescending(t => t.Amount).ToList();
        }
 public List<Transaction> SortTransactionsByCategory()
        {
            return Transactions.OrderBy(t => t.Category).ToList();
        }
 public string GenerateCategoryChart(TransactionType type)
        {
            var categoryBreakdown = GetCategoryBreakdown(type);
            if (categoryBreakdown.Count == 0)
                return "No data available for chart.";

            decimal total = categoryBreakdown.Values.Sum();
            int chartWidth = 40;
            string result = $"\n{(type == TransactionType.Income ? "Income" : "Expense")} Category Breakdown:\n";

            foreach (var category in categoryBreakdown.OrderByDescending(kvp => kvp.Value))
            {
                decimal percentage = category.Value / total * 100;
                int barLength = (int)Math.Round(percentage * chartWidth / 100);
                string bar = new string('█', barLength);

                result += $"{category.Key,-15} {category.Value,10:C} ({percentage,5:F1}%) {bar}\n";
            }

            return result;
        }
 public string GenerateMonthlyReport()
        {
            var monthlyData = Transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                })
                .ToList();

            if (monthlyData.Count == 0)
                return "No data available for monthly report.";

            string report = "\nMonthly Summary:\n";
            report += "Month      | Income      | Expenses     | Savings\n";
            report += "-------------------------------------------------------\n";

            foreach (var month in monthlyData)
            {
                decimal savings = month.Income - month.Expenses;
                report += $"{month.YearMonth} | {month.Income,11:C} | {month.Expenses,11:C} | {savings,11:C}\n";
            }

            return report;
        }
    }
   
class Program
    {
        private static BudgetTracker budgetTracker = new BudgetTracker();

        static void Main(string[] args)
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("==== Personal Budget Tracker ====");
                Console.WriteLine("1. Add Income");
                Console.WriteLine("2. Add Expense");
                Console.WriteLine("3. View All Transactions");
                Console.WriteLine("4. View Financial Summary");
                Console.WriteLine("5. View Category Analysis");
                Console.WriteLine("6. View Monthly Report");
                Console.WriteLine("7. Exit");
                Console.Write("\nSelect an option: ");

                try
                {
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        case "1":
                            AddTransaction(TransactionType.Income);
                            break;
                        case "2":
                            AddTransaction(TransactionType.Expense);
                            break;
                        case "3":
                            ViewAllTransactions();
                            break;
                        case "4":
                            ViewFinancialSummary();
                            break;
                        case "5":
                            ViewCategoryAnalysis();
                            break;
                        case "6":
                            ViewMonthlyReport();
                            break;
                        case "7":
                            running = false;
                            Console.WriteLine("Thank you for using Personal Budget Tracker!");
                            break;
                        default:
                            Console.WriteLine("Invalid option. Press any key to continue...");
                            Console.ReadKey();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }


