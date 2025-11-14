using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Data;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ExpenseDbContext _context;

    public UsersController(ExpenseDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.IsActive)
            .Select(u => new
            {
                u.UserId,
                u.UserName,
                u.Email,
                Role = u.Role.RoleName
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.UserId,
            user.UserName,
            user.Email,
            Role = user.Role.RoleName,
            ManagerName = user.Manager?.UserName
        });
    }
}
