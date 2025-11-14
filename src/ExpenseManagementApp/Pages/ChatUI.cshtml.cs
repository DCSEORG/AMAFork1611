using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ExpenseManagementApp.Pages;

public class ChatUIModel : PageModel
{
    private readonly IConfiguration _configuration;

    public ChatUIModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsChatUIEnabled { get; set; }

    public void OnGet()
    {
        // Check if Chat UI is enabled in configuration
        IsChatUIEnabled = _configuration.GetValue<bool>("ChatUI:Enabled", false);
    }
}
