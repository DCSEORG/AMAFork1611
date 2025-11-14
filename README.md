![Header image](https://github.com/DougChisholm/App-Mod-Assist/blob/main/repo-header.png)

# Expense Management System - Modernized Azure Application

A modern, cloud-native expense management system built with ASP.NET Core, deployed on Azure App Service, with optional GenAI chat capabilities.

This project demonstrates how GitHub Copilot can transform legacy application screenshots and database schemas into a fully functional Azure-based solution.

## Features

### Core Functionality (Always Available)
- ✅ **Expense Submission** - Employees can create and submit expense claims
- ✅ **Expense Approval** - Managers can review and approve/reject expenses
- ✅ **Expense Tracking** - View all expenses with filtering capabilities
- ✅ **RESTful APIs** - Complete API suite for expense management operations
- ✅ **Swagger Documentation** - Interactive API documentation and testing
- ✅ **Azure SQL Integration** - Connects to existing Azure SQL Database

### Optional Features (INCLUDE_CHAT_UI=true)
- 🤖 **Natural Language Chat UI** - Interact with the system using conversational AI
- 🧠 **GenAI Integration** - Powered by Azure OpenAI (GPT-4o-mini)
- 📚 **RAG Pattern** - Retrieval-Augmented Generation for contextual responses
- 🔍 **Intelligent Search** - Azure Cognitive Search for enhanced querying

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture diagrams and connection flows.

**Core Stack:**
- ASP.NET Core 8.0 (Razor Pages + Web API)
- Azure App Service (Linux, B1 tier)
- Azure SQL Database (existing)
- Entity Framework Core

**Optional GenAI Stack:**
- Azure OpenAI Service
- Azure Cognitive Search
- Azure Storage Account

## Quick Start

### Prerequisites
- Azure CLI installed and authenticated (`az login`)
- Azure subscription with appropriate permissions
- Existing Azure SQL Database (see Database Setup below)

### Default Deployment (Without Chat UI)

```bash
# 1. Clone the repository
git clone <your-fork-url>
cd AMAFork1611

# 2. Login to Azure
az login
az account set --subscription <your-subscription-id>

# 3. Deploy infrastructure and application
./deploy.sh
```

**That's it!** The script will:
1. Create a resource group in UK South
2. Deploy Azure App Service (B1 tier)
3. Deploy the application code
4. Output the application URL

### Access Your Application

After deployment completes:
- **Main Application**: `https://<app-name>.azurewebsites.net/Index`
- **API Documentation**: `https://<app-name>.azurewebsites.net/swagger`

⚠️ **IMPORTANT**: Navigate to `/Index` endpoint, not just the root URL!

### Optional: Deploy with GenAI Chat UI

To deploy with natural language chat capabilities:

```bash
# Set environment variable before deployment
export INCLUDE_CHAT_UI=true
./deploy.sh
```

This will additionally deploy:
- Azure OpenAI Service
- Azure Cognitive Search
- Azure Storage Account
- Chat UI components

## Database Setup

The application connects to an existing Azure SQL Database with the schema defined in `Database-Schema/database_schema.sql`.

**Connection String** (configured in `appsettings.json`):
```
Server=tcp:sql-expense-mgmt-xyz.database.windows.net,1433;
Initial Catalog=ExpenseManagementDB;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
Authentication=Active Directory Default;
```

### Setting Up Your Database

1. Create an Azure SQL Database named `ExpenseManagementDB`
2. Execute the schema from `Database-Schema/database_schema.sql`
3. Configure Managed Identity access for the App Service
4. Update the connection string in `appsettings.json` if needed

## API Endpoints

All APIs are documented in Swagger. Key endpoints include:

### Expenses
- `GET /api/expenses` - List all expenses (with optional status filter)
- `GET /api/expenses/{id}` - Get specific expense
- `POST /api/expenses` - Create new expense
- `POST /api/expenses/{id}/submit` - Submit expense for approval
- `POST /api/expenses/{id}/approve` - Approve expense
- `POST /api/expenses/{id}/reject` - Reject expense
- `DELETE /api/expenses/{id}` - Delete expense

### Categories
- `GET /api/categories` - List expense categories

### Users
- `GET /api/users` - List users
- `GET /api/users/{id}` - Get specific user

## Configuration

### Resource Settings

Edit `deploy.sh` to customize:
- `RESOURCE_GROUP` - Azure resource group name (default: `rg-expense-management`)
- `LOCATION` - Azure region (default: `uksouth`)
- `INCLUDE_CHAT_UI` - Enable GenAI features (default: `false`)

### Infrastructure Settings

App Service configuration in `infrastructure/app-service.bicep`:
- SKU: `B1` (Basic tier - can be changed to `F1` for free tier)
- Runtime: .NET 8.0 on Linux
- Always On: Enabled (except for F1 tier)

GenAI configuration in `infrastructure/genai-resources.bicep`:
- OpenAI: S0 tier (standard)
- Search: Basic tier
- Storage: Standard_LRS

## Development

### Local Development

```bash
cd src/ExpenseManagementApp

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Navigate to https://localhost:5001/Index
```

### Building the Deployment Package

```bash
cd src/ExpenseManagementApp

# Publish the application
dotnet publish -c Release -o ../../publish

# Create deployment zip
cd ../../publish
zip -r ../app.zip .
```

## Cost Optimization

**Default deployment (without Chat UI):**
- App Service B1: ~£40-50/month
- SQL Database: Depends on existing configuration
- **Total**: ~£40-50/month + database costs

**With Chat UI enabled:**
- Add Azure OpenAI: ~£20-100/month (usage-based)
- Add Cognitive Search Basic: ~£60/month
- Add Storage: <£5/month
- **Total**: ~£125-220/month + database costs

💡 **Tip**: Use F1 (Free) tier for App Service during development/testing to reduce costs.

## Troubleshooting

### Application won't start
- Check App Service logs in Azure Portal
- Verify database connection string is correct
- Ensure Managed Identity has access to SQL Database

### Database connection fails
- Configure Managed Identity for the App Service
- Grant appropriate SQL permissions
- Check firewall rules on Azure SQL Database

### Chat UI not working
- Verify `INCLUDE_CHAT_UI=true` was set during deployment
- Check `GenAISettings.json` for correct endpoints
- Ensure OpenAI models are deployed

## Security Considerations

This is a **development/POC deployment**. For production use:

1. Enable Application Insights for monitoring
2. Configure Azure Key Vault for secrets
3. Implement proper authentication (Azure AD B2C)
4. Enable Azure WAF and DDoS protection
5. Use private endpoints for all services
6. Implement row-level security in database
7. Enable audit logging
8. Regular security scans and updates

## Contributing

When contributing to this repository:
1. Fork the repository (rename to avoid confusion with main repo)
2. Test your changes thoroughly
3. Ensure deployment scripts still work
4. Update documentation as needed

## License

See [LICENSE](LICENSE) file for details.

## Support

For issues and questions:
1. Check [ARCHITECTURE.md](ARCHITECTURE.md) for system design
2. Review Swagger documentation at `/swagger`
3. Check Azure Portal logs for runtime issues
4. Review database schema in `Database-Schema/`
