# Azure Architecture Diagram - Expense Management System

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Azure Resource Group (UK South)                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │                    Core Application (Always Deployed)               │    │
│  │                                                                     │    │
│  │   ┌─────────────────────┐         ┌──────────────────────────┐    │    │
│  │   │   Azure App Service │         │   Azure SQL Database     │    │    │
│  │   │  (B1 - Basic tier)  │────────▶│  (Existing Database)     │    │    │
│  │   │                     │         │  ExpenseManagementDB     │    │    │
│  │   │  - Razor Pages UI   │         │                          │    │    │
│  │   │  - REST APIs        │         │  Tables:                 │    │    │
│  │   │  - Swagger Docs     │         │  - Users                 │    │    │
│  │   └─────────────────────┘         │  - Roles                 │    │    │
│  │            │                       │  - Expenses              │    │    │
│  │            │                       │  - ExpenseCategories     │    │    │
│  │            │                       │  - ExpenseStatus         │    │    │
│  │            │                       └──────────────────────────┘    │    │
│  │            │                                                       │    │
│  └────────────┼───────────────────────────────────────────────────────┘    │
│               │                                                             │
│               │ (Optional: INCLUDE_CHAT_UI=true)                           │
│               ▼                                                             │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │              GenAI Resources (Optional - Not Deployed by Default)   │    │
│  │                                                                     │    │
│  │   ┌──────────────────┐      ┌─────────────────────┐               │    │
│  │   │ Azure OpenAI     │      │ Azure Cognitive     │               │    │
│  │   │ (S0 - Standard)  │◀────▶│ Search (Basic)      │               │    │
│  │   │                  │      │                     │               │    │
│  │   │ - GPT-4o-mini    │      │ For RAG indexing    │               │    │
│  │   │ - Embeddings     │      │ and vector search   │               │    │
│  │   └──────────────────┘      └─────────────────────┘               │    │
│  │            │                          ▲                            │    │
│  │            │                          │                            │    │
│  │            │                          │                            │    │
│  │            ▼                          │                            │    │
│  │   ┌────────────────────────────────────────┐                       │    │
│  │   │    Azure Storage Account               │                       │    │
│  │   │    (Standard_LRS)                      │                       │    │
│  │   │                                        │                       │    │
│  │   │  - rag-documents container             │                       │    │
│  │   │  - Stores contextual info for RAG      │                       │    │
│  │   └────────────────────────────────────────┘                       │    │
│  │                                                                     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  Authentication & Access:                                                   │
│  • Managed Identity (recommended) for service-to-service auth               │
│  • Azure AD Default authentication for SQL Database                         │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘

User Access:
───────────────────────────────────────────────────────────────────────────────
  Internet  ──▶  App Service URL/Index  ──▶  Expense Management UI
            ──▶  App Service URL/swagger ──▶  API Documentation
            ──▶  App Service URL/chatui  ──▶  GenAI Chat (if enabled)


Deployment Modes:
───────────────────────────────────────────────────────────────────────────────
1. Default Mode (INCLUDE_CHAT_UI=false):
   - Deploys App Service only
   - Connects to existing Azure SQL Database
   - Provides traditional UI and REST APIs
   
2. With GenAI Mode (INCLUDE_CHAT_UI=true):
   - Deploys all resources shown above
   - Adds natural language chat interface
   - Implements RAG pattern for contextual responses
   - Integrates OpenAI with expense management APIs
```

## Connection Flow

### Standard Expense Operations:
1. User accesses App Service UI
2. App Service connects to Azure SQL Database via Managed Identity
3. User performs CRUD operations on expenses

### GenAI Chat Operations (when enabled):
1. User sends natural language query via Chat UI
2. App Service calls Azure OpenAI with user query
3. Azure OpenAI uses function calling to invoke appropriate API endpoints
4. Azure Search provides contextual information via RAG pattern
5. Response is generated and returned to user

## Security:
- All resources use HTTPS/TLS 1.2+
- Managed Identity for service authentication (no keys in code)
- Azure SQL uses Azure AD authentication
- Storage and Search endpoints are private by default
