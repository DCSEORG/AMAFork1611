using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Data;
using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Pages;

public class IndexModel : PageModel
{
    private readonly ExpenseDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ExpenseDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Expense> Expenses { get; set; } = new();
    public string? FilterText { get; set; }

    public async Task OnGetAsync(string? filter)
    {
        FilterText = filter;

        var query = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .Include(e => e.Status)
            .OrderByDescending(e => e.ExpenseDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.ToLower();
            query = query.Where(e =>
                e.Category.CategoryName.ToLower().Contains(filter) ||
                e.Description!.ToLower().Contains(filter) ||
                e.Status.StatusName.ToLower().Contains(filter));
        }

        Expenses = await query.ToListAsync();
    }
}
