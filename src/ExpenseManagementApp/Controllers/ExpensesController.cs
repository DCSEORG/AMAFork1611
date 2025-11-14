using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Data;
using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseDbContext _context;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(ExpenseDbContext context, ILogger<ExpensesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all expenses
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetExpenses([FromQuery] string? status = null)
    {
        var query = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .Include(e => e.Status)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(e => e.Status.StatusName.ToLower() == status.ToLower());
        }

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new
            {
                e.ExpenseId,
                e.ExpenseDate,
                Category = e.Category.CategoryName,
                Amount = e.AmountGBP,
                e.Currency,
                e.Description,
                Status = e.Status.StatusName,
                UserName = e.User.UserName,
                e.SubmittedAt,
                e.ReviewedAt
            })
            .ToListAsync();

        return Ok(expenses);
    }

    /// <summary>
    /// Get a specific expense by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetExpense(int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .Include(e => e.Status)
            .Include(e => e.Reviewer)
            .FirstOrDefaultAsync(e => e.ExpenseId == id);

        if (expense == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            expense.ExpenseId,
            expense.ExpenseDate,
            Category = expense.Category.CategoryName,
            Amount = expense.AmountGBP,
            expense.Currency,
            expense.Description,
            Status = expense.Status.StatusName,
            UserName = expense.User.UserName,
            expense.SubmittedAt,
            ReviewerName = expense.Reviewer?.UserName,
            expense.ReviewedAt
        });
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        var category = await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);
        if (category == null)
        {
            return BadRequest("Category not found");
        }

        var draftStatus = await _context.ExpenseStatus.FirstOrDefaultAsync(s => s.StatusName == "Draft");
        if (draftStatus == null)
        {
            return BadRequest("Draft status not found");
        }

        var expense = new Expense
        {
            UserId = dto.UserId,
            CategoryId = dto.CategoryId,
            StatusId = draftStatus.StatusId,
            AmountMinor = (int)(dto.Amount * 100),
            Currency = "GBP",
            ExpenseDate = dto.ExpenseDate,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpense), new { id = expense.ExpenseId }, new
        {
            expense.ExpenseId,
            expense.ExpenseDate,
            Category = category.CategoryName,
            Amount = expense.AmountGBP,
            expense.Currency,
            expense.Description,
            Status = draftStatus.StatusName
        });
    }

    /// <summary>
    /// Submit an expense for approval
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult> SubmitExpense(int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Status)
            .FirstOrDefaultAsync(e => e.ExpenseId == id);

        if (expense == null)
        {
            return NotFound();
        }

        if (expense.Status.StatusName != "Draft")
        {
            return BadRequest("Only draft expenses can be submitted");
        }

        var submittedStatus = await _context.ExpenseStatus.FirstOrDefaultAsync(s => s.StatusName == "Submitted");
        if (submittedStatus == null)
        {
            return BadRequest("Submitted status not found");
        }

        expense.StatusId = submittedStatus.StatusId;
        expense.SubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Expense submitted successfully" });
    }

    /// <summary>
    /// Approve an expense
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult> ApproveExpense(int id, [FromBody] ApproveExpenseDto dto)
    {
        var expense = await _context.Expenses
            .Include(e => e.Status)
            .FirstOrDefaultAsync(e => e.ExpenseId == id);

        if (expense == null)
        {
            return NotFound();
        }

        if (expense.Status.StatusName != "Submitted")
        {
            return BadRequest("Only submitted expenses can be approved");
        }

        var approvedStatus = await _context.ExpenseStatus.FirstOrDefaultAsync(s => s.StatusName == "Approved");
        if (approvedStatus == null)
        {
            return BadRequest("Approved status not found");
        }

        expense.StatusId = approvedStatus.StatusId;
        expense.ReviewedBy = dto.ReviewerId;
        expense.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Expense approved successfully" });
    }

    /// <summary>
    /// Reject an expense
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectExpense(int id, [FromBody] ApproveExpenseDto dto)
    {
        var expense = await _context.Expenses
            .Include(e => e.Status)
            .FirstOrDefaultAsync(e => e.ExpenseId == id);

        if (expense == null)
        {
            return NotFound();
        }

        if (expense.Status.StatusName != "Submitted")
        {
            return BadRequest("Only submitted expenses can be rejected");
        }

        var rejectedStatus = await _context.ExpenseStatus.FirstOrDefaultAsync(s => s.StatusName == "Rejected");
        if (rejectedStatus == null)
        {
            return BadRequest("Rejected status not found");
        }

        expense.StatusId = rejectedStatus.StatusId;
        expense.ReviewedBy = dto.ReviewerId;
        expense.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Expense rejected successfully" });
    }

    /// <summary>
    /// Delete an expense
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Expense deleted successfully" });
    }
}

public record CreateExpenseDto(int UserId, int CategoryId, decimal Amount, DateTime ExpenseDate, string? Description);
public record ApproveExpenseDto(int ReviewerId);
