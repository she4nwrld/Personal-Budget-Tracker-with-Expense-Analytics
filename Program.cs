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
