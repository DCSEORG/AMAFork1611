using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Data;
using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Pages;

public class ApproveExpensesModel : PageModel
{
    private readonly ExpenseDbContext _context;
    private readonly ILogger<ApproveExpensesModel> _logger;

    public ApproveExpensesModel(ExpenseDbContext context, ILogger<ApproveExpensesModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Expense> PendingExpenses { get; set; } = new();
    public string? FilterText { get; set; }

    [BindProperty]
    public List<int> SelectedExpenseIds { get; set; } = new();

    public async Task OnGetAsync(string? filter)
    {
        FilterText = filter;

        var query = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .Include(e => e.Status)
            .Where(e => e.Status.StatusName == "Submitted")
            .OrderBy(e => e.SubmittedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.ToLower();
            query = query.Where(e =>
                e.Category.CategoryName.ToLower().Contains(filter) ||
                e.Description!.ToLower().Contains(filter));
        }

        PendingExpenses = await query.ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (SelectedExpenseIds == null || !SelectedExpenseIds.Any())
        {
            return RedirectToPage();
        }

        // Get the first manager user (for demo purposes)
        var manager = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Role.RoleName == "Manager");

        if (manager == null)
        {
            return RedirectToPage();
        }

        // Get the "Approved" status
        var approvedStatus = await _context.ExpenseStatus
            .FirstOrDefaultAsync(s => s.StatusName == "Approved");

        if (approvedStatus == null)
        {
            return RedirectToPage();
        }

        // Update selected expenses
        var expenses = await _context.Expenses
            .Where(e => SelectedExpenseIds.Contains(e.ExpenseId))
            .ToListAsync();

        foreach (var expense in expenses)
        {
            expense.StatusId = approvedStatus.StatusId;
            expense.ReviewedBy = manager.UserId;
            expense.ReviewedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}
