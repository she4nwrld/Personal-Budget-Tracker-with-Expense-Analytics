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



