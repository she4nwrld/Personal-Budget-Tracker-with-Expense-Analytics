using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalBudgetTracker
{
    public enum TransactionType { Income, Expense }

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
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty", nameof(category));
            Description = description; Amount = amount; Type = type; Category = category; Date = date; Id = Guid.NewGuid();
        }

        public Transaction() { Id = Guid.NewGuid(); }

        public override string ToString()
        {
            string typeSymbol = Type == TransactionType.Income ? "+" : "-";
            return $"{Date.ToShortDateString()} | {typeSymbol}{Amount:C} | {Category} | {Description}";
        }
    }

    public class BudgetTracker
    {
        public List<Transaction> Transactions { get; private set; }
        public BudgetTracker() { Transactions = new List<Transaction>(); }

        public void AddTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            Transactions.Add(transaction);
        }

        public decimal CalculateTotalIncome() => Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal CalculateTotalExpenses() => Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        public decimal CalculateNetSavings() => CalculateTotalIncome() - CalculateTotalExpenses();

        public List<Transaction> GetTransactionsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty", nameof(category));
            return Transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public decimal GetTotalByCategory(string category, TransactionType type)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty", nameof(category));
            return Transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && t.Type == type).Sum(t => t.Amount);
        }

        public List<string> GetAllCategories() => Transactions.Select(t => t.Category).Distinct().OrderBy(c => c).ToList();

        public Dictionary<string, decimal> GetCategoryBreakdown(TransactionType type) =>
            Transactions.Where(t => t.Type == type).GroupBy(t => t.Category).ToDictionary(group => group.Key, group => group.Sum(t => t.Amount));

        public (string Category, decimal Amount) GetHighestSpendingCategory()
        {
            var expensesByCategory = GetCategoryBreakdown(TransactionType.Expense);
            if (expensesByCategory.Count == 0) return ("None", 0);
            var highestCategory = expensesByCategory.OrderByDescending(kvp => kvp.Value).First();
            return (highestCategory.Key, highestCategory.Value);
        }

        public List<Transaction> SortTransactionsByDate(bool ascending = true) =>
            ascending ? Transactions.OrderBy(t => t.Date).ToList() : Transactions.OrderByDescending(t => t.Date).ToList();

        public List<Transaction> SortTransactionsByAmount(bool ascending = true) =>
            ascending ? Transactions.OrderBy(t => t.Amount).ToList() : Transactions.OrderByDescending(t => t.Amount).ToList();

        public List<Transaction> SortTransactionsByCategory() => Transactions.OrderBy(t => t.Category).ToList();

        public string GenerateCategoryChart(TransactionType type)
        {
            var categoryBreakdown = GetCategoryBreakdown(type);
            if (categoryBreakdown.Count == 0) return "No data available for chart.";

            decimal total = categoryBreakdown.Values.Sum();
            int chartWidth = 40;
            string result = $"\n{(type == TransactionType.Income ? "Income" : "Expense")} Category Breakdown:\n";

            foreach (var category in categoryBreakdown.OrderByDescending(kvp => kvp.Value))
            {
                decimal percentage = category.Value / total * 100;
                int barLength = (int)Math.Round(percentage * chartWidth / 100);
                string bar = new string('█', barLength);
                result += $"{category.Key,-15} {category.Value,10:C} ({percentage,5:F1}%) {bar}\n\n";
            }
            return result;
        }

        public string GenerateMonthlyReport()
        {
            var monthlyData = Transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new {
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                }).ToList();

            if (monthlyData.Count == 0) return "No data available for monthly report.";

            string report = "\nMonthly Summary:\n";
            report += "Month      | Income      | Expenses     | Net Savings\n";
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
                Console.WriteLine("1. Add Income\n2. Add Expense\n3. View All Transactions\n4. View Financial Summary");
                Console.WriteLine("5. View Category Analysis\n6. View Monthly Report\n7. Exit");
                Console.Write("\nSelect an option: ");

                try
                {
                    string input = Console.ReadLine();
                    switch (input)
                    {
                        case "1": AddTransaction(TransactionType.Income); break;
                        case "2": AddTransaction(TransactionType.Expense); break;
                        case "3": ViewAllTransactions(); break;
                        case "4": ViewFinancialSummary(); break;
                        case "5": ViewCategoryAnalysis(); break;
                        case "6": ViewMonthlyReport(); break;
                        case "7":
                            running = false;
                            Console.WriteLine("Thank you for using Personal Budget Tracker!");
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please select a number between 1-7.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Clear(); 
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        static void AddTransaction(TransactionType type)
        {
            Console.Clear();
            Console.WriteLine($"==== Add {type} ====");

            try
            {
                string description = "";
                bool validDescription = false;
                while (!validDescription)
                {
                    Console.Write("Description: ");
                    description = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        Console.Clear(); 
                        Console.WriteLine($"==== Add {type} ====");
                        Console.WriteLine("Invalid: Description cannot be empty. Try again.");
                    }
                    else validDescription = true;
                }

                decimal amount = 0;
                bool validAmount = false;
                while (!validAmount)
                {
                    Console.Write("Amount: $");
                    string amountInput = Console.ReadLine();
                    if (!decimal.TryParse(amountInput, out amount) || amount <= 0)
                    {
                        Console.Clear(); 
                        Console.WriteLine($"==== Add {type} ====");
                        Console.WriteLine("Description: " + description);
                        Console.WriteLine("Invalid amount. Please enter a positive number. Try again.");
                    }
                    else validAmount = true;
                }

                string category = "";
                bool validCategory = false;
                while (!validCategory)
                {
                    Console.Write("Category: ");
                    category = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(category))
                    {
                        Console.Clear(); 
                        Console.WriteLine($"==== Add {type} ====");
                        Console.WriteLine("Description: " + description);
                        Console.WriteLine("Amount: $" + amount);
                        Console.WriteLine("Invalid: Category cannot be empty. Try again.");
                    }
                    else validCategory = true;
                }

                DateTime date = DateTime.Today;
                bool validDate = false;
                while (!validDate)
                {
                    Console.Write("Date (YYYY-MM-DD, leave blank for today): ");
                    string dateInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(dateInput)) { date = DateTime.Today; validDate = true; }
                    else
                    {
                        try { date = DateTime.Parse(dateInput); validDate = true; }
                        catch (FormatException)
                        {
                            Console.Clear(); 
                            Console.WriteLine($"==== Add {type} ====");
                            Console.WriteLine("Description: " + description);
                            Console.WriteLine("Amount: $" + amount);
                            Console.WriteLine("Category: " + category);
                            Console.WriteLine("Invalid date format. Would you like to try again or return to main menu? (try/menu): ");
                            string response = Console.ReadLine().ToLower();
                            if (response != "try") { Console.WriteLine("Returning to main menu..."); return; }
                        }
                    }
                }

                var transaction = new Transaction(description, amount, type, category, date);
                budgetTracker.AddTransaction(transaction);
                Console.WriteLine($"==== Add {type} ====");
                Console.Clear();
                Console.WriteLine($"\n{type} added successfully!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.Clear(); 
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        static void ViewAllTransactions()
        {
            Console.Clear();
            Console.WriteLine("==== All Transactions ====");

            if (budgetTracker.Transactions.Count == 0) Console.WriteLine("No transactions recorded yet.");
            else
            {
                string sortOption = "";
                bool validOption = false;
                while (!validOption)
                {
                    Console.WriteLine("Sort by:");
                    Console.WriteLine("1. Date (newest first)\n2. Date (oldest first)\n3. Amount (highest first)");
                    Console.WriteLine("4. Amount (lowest first)\n5. Category");
                    Console.Write("\nSelect an option: ");
                    sortOption = Console.ReadLine();
                    if (sortOption == "1" || sortOption == "2" || sortOption == "3" || sortOption == "4" || sortOption == "5") validOption = true;
                    else
                    {
                        Console.Clear(); 
                        Console.WriteLine("==== All Transactions ====");
                        Console.WriteLine("Invalid option. Try again.");
                    }
                }

                List<Transaction> sortedTransactions;
                switch (sortOption)
                {
                    case "1": sortedTransactions = budgetTracker.SortTransactionsByDate(false); break;
                    case "2": sortedTransactions = budgetTracker.SortTransactionsByDate(true); break;
                    case "3": sortedTransactions = budgetTracker.SortTransactionsByAmount(false); break;
                    case "4": sortedTransactions = budgetTracker.SortTransactionsByAmount(true); break;
                    case "5": sortedTransactions = budgetTracker.SortTransactionsByCategory(); break;
                    default: sortedTransactions = budgetTracker.SortTransactionsByDate(false); break;
                }

                Console.Clear(); 
                Console.WriteLine("==== All Transactions ====");
                Console.WriteLine("\nDate       | Amount        | Category      | Description");
                Console.WriteLine("----------------------------------------------------------");
                foreach (var transaction in sortedTransactions) Console.WriteLine(transaction);
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static void ViewFinancialSummary()
        {
            Console.Clear();
            Console.WriteLine("==== Financial Summary ====");

            decimal totalIncome = budgetTracker.CalculateTotalIncome();
            decimal totalExpenses = budgetTracker.CalculateTotalExpenses();
            decimal netSavings = budgetTracker.CalculateNetSavings();

            Console.WriteLine($"Total Income:   {totalIncome:C}");
            Console.WriteLine($"Total Expenses: {totalExpenses:C}");
            Console.WriteLine($"Net Savings:    {netSavings:C}");

            if (totalIncome > 0)
            {
                decimal savingsRate = netSavings / totalIncome * 100;
                Console.WriteLine($"Savings Rate:   {savingsRate:F1}%");
            }

            var (category, amount) = budgetTracker.GetHighestSpendingCategory();
            Console.WriteLine($"\nHighest Expense Category: {category} ({amount:C})");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static void ViewCategoryAnalysis()
        {
            Console.Clear();
            Console.WriteLine("==== Category Analysis ====");
            Console.WriteLine(budgetTracker.GenerateCategoryChart(TransactionType.Expense));
            Console.WriteLine(budgetTracker.GenerateCategoryChart(TransactionType.Income));
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static void ViewMonthlyReport()
        {
            Console.Clear();
            Console.WriteLine("==== Monthly Report ====");
            Console.WriteLine(budgetTracker.GenerateMonthlyReport());
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
