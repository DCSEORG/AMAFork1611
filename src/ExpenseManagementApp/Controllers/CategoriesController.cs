using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Data;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ExpenseDbContext _context;

    public CategoriesController(ExpenseDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all expense categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCategories()
    {
        var categories = await _context.ExpenseCategories
            .Where(c => c.IsActive)
            .Select(c => new
            {
                c.CategoryId,
                c.CategoryName
            })
            .ToListAsync();

        return Ok(categories);
    }
}
