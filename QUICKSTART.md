# Quick Start Guide - Expense Management System

## 🚀 Deploy in 3 Steps

### 1. Prerequisites
```bash
# Install Azure CLI (if not already installed)
# macOS: brew install azure-cli
# Windows: Download from https://aka.ms/installazurecliwindows
# Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login to Azure
az login

# Set your subscription
az account set --subscription "<your-subscription-id>"
```

### 2. Deploy Application
```bash
# Clone the repository
git clone <your-repo-url>
cd AMAFork1611

# Deploy (default - without Chat UI)
./deploy.sh

# OR Deploy with GenAI Chat UI
export INCLUDE_CHAT_UI=true
./deploy.sh
```

### 3. Configure Database
```bash
# Execute the schema on your Azure SQL Database
# Connection string is in src/ExpenseManagementApp/appsettings.json
# Update with your actual server name

# Run the SQL file:
# Database-Schema/database_schema.sql
```

## 📍 Access Your Application

After deployment completes, you'll see output like:
```
Application URL: https://app-expense-mgmt-abc123.azurewebsites.net
```

**Navigate to:**
- **Main App**: `https://<your-app>.azurewebsites.net/Index`
- **Add Expense**: `https://<your-app>.azurewebsites.net/AddExpense`
- **Approve**: `https://<your-app>.azurewebsites.net/ApproveExpenses`
- **API Docs**: `https://<your-app>.azurewebsites.net/swagger`
- **AI Chat** (if enabled): `https://<your-app>.azurewebsites.net/ChatUI`

⚠️ **IMPORTANT**: Always navigate to `/Index` endpoint, not just the root URL!

## 🎯 Key Features

### For Employees
- ✅ Submit expense claims
- ✅ Track expense status
- ✅ Filter and search expenses
- ✅ View expense history

### For Managers
- ✅ Review pending expenses
- ✅ Approve/reject claims
- ✅ Filter expenses by status
- ✅ View team expenses

### For Developers
- ✅ REST API endpoints
- ✅ Swagger documentation
- ✅ Azure SQL integration
- ✅ Entity Framework Core

### AI Features (Optional)
- 🤖 Natural language queries
- 🤖 Create expenses via chat
- 🤖 Get expense summaries
- 🤖 Contextual responses

## 💰 Costs

**Default (without Chat UI)**
- Azure App Service B1: ~£40-50/month
- Total: ~£40-50/month + database costs

**With Chat UI**
- Add Azure OpenAI: ~£20-100/month
- Add Cognitive Search: ~£60/month
- Add Storage: ~£5/month
- Total: ~£125-220/month + database costs

💡 **Tip**: Use F1 (Free) tier for testing - edit `infrastructure/app-service.bicep`

## 📚 Documentation

- **README.md** - Full documentation
- **ARCHITECTURE.md** - System architecture
- **DEPLOYMENT-CHECKLIST.md** - Validation guide
- **SUMMARY.md** - Project overview

## 🔧 Troubleshooting

### App won't start?
```bash
# Check logs in Azure Portal
az webapp log tail --name <app-name> --resource-group rg-expense-management
```

### Database connection fails?
1. Verify connection string in appsettings.json
2. Ensure Managed Identity is enabled on App Service
3. Grant SQL permissions to App Service identity
4. Check SQL firewall rules allow Azure services

### Chat UI not working?
1. Verify `INCLUDE_CHAT_UI=true` was set during deployment
2. Check `ChatUI:Enabled = true` in appsettings.json
3. Update Azure OpenAI endpoint in configuration
4. Verify OpenAI models are deployed

## 🆘 Support

**Documentation Issues?**
- Check SUMMARY.md for complete overview
- Review ARCHITECTURE.md for system design
- See DEPLOYMENT-CHECKLIST.md for validation steps

**Deployment Issues?**
- Verify Azure CLI is installed and authenticated
- Check subscription has appropriate permissions
- Ensure resource names are unique

**Runtime Issues?**
- Check App Service logs in Azure Portal
- Verify database schema is deployed
- Test API endpoints via Swagger

## 🎓 Example Queries (Chat UI)

Try these natural language queries:
- "Show me all submitted expenses"
- "What is the total of approved expenses?"
- "Create a new expense for £50 in the Travel category"
- "List all expense categories"
- "How many expenses are pending approval?"

## 📝 Next Steps

1. ✅ Deploy application
2. ✅ Configure database
3. ✅ Test UI pages
4. ✅ Test API endpoints
5. ✅ Enable Chat UI (optional)
6. 🚀 **Go live!**

---

**Need Help?** Check the documentation files or raise an issue in the repository.

**Ready to Deploy?** Just run `./deploy.sh`! 🚀
