namespace ExpenseManagementApp.Models;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Role Role { get; set; } = null!;
    public User? Manager { get; set; }
    public ICollection<User> Subordinates { get; set; } = new List<User>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Expense> ReviewedExpenses { get; set; } = new List<Expense>();
}

public class ExpenseCategory
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class ExpenseStatus
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Expense
{
    public int ExpenseId { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int AmountMinor { get; set; } // Amount in pence (e.g., £12.34 -> 1234)
    public string Currency { get; set; } = "GBP";
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptFile { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
    public ExpenseStatus Status { get; set; } = null!;
    public User? Reviewer { get; set; }
    
    // Helper property to get amount in pounds
    public decimal AmountGBP => AmountMinor / 100m;
}
