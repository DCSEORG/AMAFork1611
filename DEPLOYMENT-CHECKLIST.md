# Deployment and Validation Checklist

## Pre-Deployment

- [x] All code compiles successfully
- [x] app.zip deployment package created
- [x] Infrastructure Bicep templates validated
- [x] Deployment scripts created and marked executable
- [x] Documentation complete (README, ARCHITECTURE)
- [x] RAG documents created for chat context

## Deployment Steps

### Default Deployment (Without Chat UI)

```bash
# 1. Authenticate with Azure
az login
az account set --subscription <your-subscription-id>

# 2. Run deployment script
./deploy.sh

# 3. Expected outputs:
# - Resource group created: rg-expense-management
# - App Service deployed (B1 tier, UK South)
# - Application code deployed from app.zip
# - Application URL provided
```

### With Chat UI

```bash
# 1. Set environment variable
export INCLUDE_CHAT_UI=true

# 2. Run deployment script
./deploy.sh

# 3. Additional resources deployed:
# - Azure OpenAI Service
# - Azure Cognitive Search (Basic tier)
# - Azure Storage Account
# - GenAI models: gpt-4o-mini, text-embedding-ada-002
```

## Post-Deployment Validation

### Database Setup
- [ ] Execute Database-Schema/database_schema.sql on Azure SQL Database
- [ ] Verify sample data is loaded (users, categories, statuses, sample expenses)
- [ ] Configure Managed Identity access for App Service to database
- [ ] Update connection string in App Service configuration if needed

### Application Testing
- [ ] Navigate to `<app-url>/Index` - should show Expenses list
- [ ] Navigate to `<app-url>/AddExpense` - form should load
- [ ] Navigate to `<app-url>/ApproveExpenses` - approval page should load
- [ ] Navigate to `<app-url>/swagger` - API documentation should display

### API Testing (via Swagger)
- [ ] GET /api/expenses - returns expense list
- [ ] GET /api/categories - returns categories
- [ ] GET /api/users - returns users
- [ ] POST /api/expenses - creates new expense
- [ ] POST /api/expenses/{id}/submit - submits expense

### Chat UI Testing (if enabled)
- [ ] Navigate to `<app-url>/ChatUI`
- [ ] Verify ChatUI.Enabled is set to true in appsettings
- [ ] Update Azure OpenAI endpoint in appsettings
- [ ] Test query: "Show me all submitted expenses"
- [ ] Test query: "What is the total of approved expenses?"
- [ ] Test query: "Create a new expense for £50 in the Travel category"

## Functionality Validation Against Legacy Screenshots

### Screenshot 1: Add Expense Form (exp1.png)
- [x] Form has Amount field
- [x] Form has Date field with date picker
- [x] Form has Category dropdown (Travel, Meals, Supplies, Accommodation, Other)
- [x] Form has Description text area
- [x] Form has Submit button
- [x] UI styling matches legacy gray background with blue header

### Screenshot 2: Expenses List (exp2.png)
- [x] Table shows: Date, Category, Amount, Status columns
- [x] Filter box at top of page
- [x] Amounts displayed with £ symbol
- [x] Dates in DD/MM/YYYY format
- [x] Status values: Draft, Submitted, Approved, Rejected

### Screenshot 3: Approve Expenses (exp3.png)
- [x] Shows pending expenses list
- [x] Filter box available
- [x] Table with Date, Category, Amount columns
- [x] Checkboxes for selecting expenses
- [x] Approve button
- [x] UI styling consistent with other pages

## Security Validation

### Application Security
- [x] HTTPS enforced (httpsOnly: true in Bicep)
- [x] TLS 1.2 minimum version
- [x] FTPS disabled
- [x] Managed Identity used for Azure services authentication
- [x] No secrets in code or configuration files
- [x] Connection uses Azure AD authentication

### API Security
- [x] Input validation on all API endpoints
- [x] Proper error handling without exposing internals
- [x] SQL injection protection via EF Core parameterization

### GenAI Security (if enabled)
- [x] Managed Identity for OpenAI authentication
- [x] Function calling restricted to defined functions only
- [x] User input sanitized before processing

## Performance Checks

- [ ] App Service responds within 3 seconds
- [ ] Database queries optimized with indexes
- [ ] API endpoints return under 1 second for most operations
- [ ] Chat responses return within 5-10 seconds

## Cost Optimization

Default deployment (without Chat UI):
- App Service B1: ~£40-50/month
- No additional Azure resources

With Chat UI:
- Add ~£80-150/month for OpenAI, Search, Storage

## Known Limitations (POC/Development)

1. Authentication: No user authentication implemented (uses first user in DB)
2. Authorization: No role-based access control
3. File Upload: Receipt upload not implemented
4. Validation: Minimal business rule validation
5. Error Handling: Basic error handling for POC
6. Scalability: Single instance, not load-balanced
7. Monitoring: No Application Insights configured

## Production Readiness Checklist (Not Included)

For production deployment, additionally required:
- [ ] Azure AD B2C authentication
- [ ] Role-based authorization (Employee, Manager roles)
- [ ] Blob storage for receipt uploads
- [ ] Application Insights monitoring
- [ ] Azure Key Vault for secrets
- [ ] Private endpoints for all services
- [ ] DDoS protection
- [ ] WAF configuration
- [ ] Backup and disaster recovery
- [ ] CI/CD pipeline
- [ ] Automated testing suite
- [ ] Load testing
- [ ] Security penetration testing

## Validation Status

✅ Application builds successfully
✅ Deployment package created (app.zip - 8.7MB)
✅ All UI pages match legacy screenshots
✅ All required APIs implemented
✅ Swagger documentation available
✅ GenAI infrastructure defined
✅ Chat UI implemented with function calling
✅ Deployment scripts ready
✅ Documentation complete

**Ready for deployment and testing!**
