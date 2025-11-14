using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Data;
using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Pages;

public class AddExpenseModel : PageModel
{
    private readonly ExpenseDbContext _context;
    private readonly ILogger<AddExpenseModel> _logger;

    public AddExpenseModel(ExpenseDbContext context, ILogger<AddExpenseModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public decimal Amount { get; set; }
    
    [BindProperty]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;
    
    [BindProperty]
    public int CategoryId { get; set; }
    
    [BindProperty]
    public string? Description { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        // Get the first user (for demo purposes - in production, this would be the logged-in user)
        var user = await _context.Users.FirstOrDefaultAsync();
        if (user == null)
        {
            ModelState.AddModelError("", "No users found in the system");
            await LoadCategoriesAsync();
            return Page();
        }

        // Get the "Draft" status
        var draftStatus = await _context.ExpenseStatus.FirstOrDefaultAsync(s => s.StatusName == "Draft");
        if (draftStatus == null)
        {
            ModelState.AddModelError("", "Draft status not found");
            await LoadCategoriesAsync();
            return Page();
        }

        var expense = new Expense
        {
            UserId = user.UserId,
            CategoryId = CategoryId,
            StatusId = draftStatus.StatusId,
            AmountMinor = (int)(Amount * 100), // Convert pounds to pence
            Currency = "GBP",
            ExpenseDate = ExpenseDate,
            Description = Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Index");
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = await _context.ExpenseCategories
            .Where(c => c.IsActive)
            .Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            })
            .ToListAsync();
    }
}
