using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;
using OpenAI.Chat;
using ExpenseManagementApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ExpenseManagementApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ExpenseDbContext _context;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IConfiguration configuration, ExpenseDbContext context, ILogger<ChatController> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            // Check if Chat UI is enabled
            var isChatUIEnabled = _configuration.GetValue<bool>("ChatUI:Enabled", false);
            if (!isChatUIEnabled)
            {
                return BadRequest(new ChatResponse 
                { 
                    Response = "Chat UI is not enabled. Please configure GenAI resources and set ChatUI:Enabled to true in configuration." 
                });
            }

            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var deploymentName = _configuration["AzureOpenAI:DeploymentName"];
            
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName))
            {
                return BadRequest(new ChatResponse 
                { 
                    Response = "Azure OpenAI configuration is incomplete. Please check your GenAISettings." 
                });
            }

            // Create Azure OpenAI client
            var client = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
            var chatClient = client.GetChatClient(deploymentName);

            // Define available functions for the AI to call
            var functions = new[]
            {
                ChatTool.CreateFunctionTool(
                    functionName: "get_expenses",
                    functionDescription: "Get a list of expenses, optionally filtered by status (Draft, Submitted, Approved, Rejected)",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "status": {
                                "type": "string",
                                "description": "Filter by expense status (optional)",
                                "enum": ["Draft", "Submitted", "Approved", "Rejected"]
                            }
                        }
                    }
                    """)
                ),
                ChatTool.CreateFunctionTool(
                    functionName: "create_expense",
                    functionDescription: "Create a new expense claim",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "amount": {
                                "type": "number",
                                "description": "The expense amount in GBP"
                            },
                            "category": {
                                "type": "string",
                                "description": "The expense category",
                                "enum": ["Travel", "Meals", "Supplies", "Accommodation", "Other"]
                            },
                            "description": {
                                "type": "string",
                                "description": "Description of the expense"
                            }
                        },
                        "required": ["amount", "category"]
                    }
                    """)
                ),
                ChatTool.CreateFunctionTool(
                    functionName: "get_expense_summary",
                    functionDescription: "Get a summary of expenses including total amounts by status",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {}
                    }
                    """)
                )
            };

            // Create chat messages
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful AI assistant for an expense management system. " +
                    "You can help users view expenses, create new expenses, and get summaries. " +
                    "Be concise and friendly in your responses. " +
                    "When showing amounts, always include the £ symbol and format as currency."),
                new UserChatMessage(request.Message)
            };

            // Call OpenAI with function calling
            var chatOptions = new ChatCompletionOptions();
            foreach (var function in functions)
            {
                chatOptions.Tools.Add(function);
            }

            var response = await chatClient.CompleteChatAsync(messages, chatOptions);
            
            // Check if AI wants to call a function
            if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                // Process function calls
                foreach (var toolCall in response.Value.ToolCalls)
                {
                    if (toolCall is ChatToolCall functionCall)
                    {
                        var functionResult = await ExecuteFunctionAsync(functionCall.FunctionName, functionCall.FunctionArguments.ToString());
                        
                        // Add function result to messages and get final response
                        messages.Add(new AssistantChatMessage(response.Value));
                        messages.Add(new ToolChatMessage(functionCall.Id, functionResult));
                        
                        var finalResponse = await chatClient.CompleteChatAsync(messages, chatOptions);
                        return Ok(new ChatResponse { Response = finalResponse.Value.Content[0].Text });
                    }
                }
            }

            return Ok(new ChatResponse { Response = response.Value.Content[0].Text });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return Ok(new ChatResponse 
            { 
                Response = "I apologize, but I encountered an error. Please try again or contact support if the problem persists." 
            });
        }
    }

    private async Task<string> ExecuteFunctionAsync(string functionName, string argumentsJson)
    {
        try
        {
            switch (functionName)
            {
                case "get_expenses":
                    return await GetExpensesAsync(argumentsJson);
                
                case "create_expense":
                    return await CreateExpenseAsync(argumentsJson);
                
                case "get_expense_summary":
                    return await GetExpenseSummaryAsync();
                
                default:
                    return $"Unknown function: {functionName}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function {FunctionName}", functionName);
            return $"Error executing {functionName}: {ex.Message}";
        }
    }

    private async Task<string> GetExpensesAsync(string argumentsJson)
    {
        var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argumentsJson);
        string? statusFilter = args?.ContainsKey("status") == true ? args["status"].GetString() : null;

        var query = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Category)
            .Include(e => e.Status)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(e => e.Status.StatusName == statusFilter);
        }

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Take(10)
            .Select(e => new
            {
                e.ExpenseId,
                Date = e.ExpenseDate.ToString("dd/MM/yyyy"),
                Category = e.Category.CategoryName,
                Amount = e.AmountGBP,
                Status = e.Status.StatusName,
                Description = e.Description,
                UserName = e.User.UserName
            })
            .ToListAsync();

        return JsonSerializer.Serialize(expenses, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> CreateExpenseAsync(string argumentsJson)
    {
        var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argumentsJson);
        
        if (args == null)
            return "Invalid arguments for creating expense";

        var amount = args["amount"].GetDecimal();
        var categoryName = args["category"].GetString();
        var description = args.ContainsKey("description") ? args["description"].GetString() : null;

        // Get first user (in real app, would use logged-in user)
        var user = await _context.Users.FirstOrDefaultAsync();
        if (user == null)
            return "Error: No users found in system";

        var category = await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
        if (category == null)
            return $"Error: Category '{categoryName}' not found";

        var draftStatus = await _context.ExpenseStatus.FirstOrDefaultAsync(s => s.StatusName == "Draft");
        if (draftStatus == null)
            return "Error: Draft status not found";

        var expense = new Models.Expense
        {
            UserId = user.UserId,
            CategoryId = category.CategoryId,
            StatusId = draftStatus.StatusId,
            AmountMinor = (int)(amount * 100),
            Currency = "GBP",
            ExpenseDate = DateTime.Today,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return JsonSerializer.Serialize(new
        {
            Success = true,
            ExpenseId = expense.ExpenseId,
            Amount = expense.AmountGBP,
            Category = categoryName,
            Status = "Draft",
            Message = $"Expense created successfully with ID {expense.ExpenseId}"
        });
    }

    private async Task<string> GetExpenseSummaryAsync()
    {
        var expenses = await _context.Expenses
            .Include(e => e.Status)
            .ToListAsync();

        var summary = expenses
            .GroupBy(e => e.Status.StatusName)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(e => e.AmountGBP)
            })
            .ToList();

        var totalAmount = expenses.Sum(e => e.AmountGBP);

        return JsonSerializer.Serialize(new
        {
            TotalExpenses = expenses.Count,
            TotalAmount = totalAmount,
            ByStatus = summary
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}

public record ChatRequest(string Message);
public record ChatResponse
{
    public string Response { get; set; } = string.Empty;
}
