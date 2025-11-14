// Azure OpenAI and supporting resources for GenAI Chat UI
// Location: UK South
// SKU: Development (lowest cost)

@description('Location for all resources')
param location string = 'uksouth'

@description('Name of the Azure OpenAI resource')
param openAIName string = 'oai-expense-mgmt-${uniqueString(resourceGroup().id)}'

@description('Name of the Cognitive Search service for RAG')
param searchServiceName string = 'srch-expense-mgmt-${uniqueString(resourceGroup().id)}'

@description('Name of the Storage Account for document storage')
param storageAccountName string = 'stexpense${uniqueString(resourceGroup().id)}'

@description('Deploy model or not')
param deployModel bool = true

// Azure OpenAI
resource openAI 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIName
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'  // Standard tier (required for Azure OpenAI)
  }
  properties: {
    customSubDomainName: openAIName
    publicNetworkAccess: 'Enabled'
  }
}

// Deploy GPT-4o-mini model for chat
resource gpt4Deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = if (deployModel) {
  parent: openAI
  name: 'gpt-4o-mini'
  sku: {
    name: 'Standard'
    capacity: 10  // Minimal capacity for dev
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o-mini'
      version: '2024-07-18'
    }
  }
}

// Deploy text-embedding-ada-002 for RAG/embeddings
resource embeddingDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = if (deployModel) {
  parent: openAI
  name: 'text-embedding-ada-002'
  sku: {
    name: 'Standard'
    capacity: 10  // Minimal capacity for dev
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
  }
  dependsOn: [
    gpt4Deployment  // Deploy sequentially to avoid conflicts
  ]
}

// Azure Cognitive Search for RAG
resource searchService 'Microsoft.Search/searchServices@2023-11-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: 'basic'  // Lowest cost tier for dev
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
  }
}

// Storage Account for document/context storage
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'  // Lowest cost redundancy
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

// Blob container for RAG documents
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource ragContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'rag-documents'
  properties: {
    publicAccess: 'None'
  }
}

output openAIEndpoint string = openAI.properties.endpoint
output openAIName string = openAI.name
output searchServiceEndpoint string = 'https://${searchService.name}.search.windows.net'
output searchServiceName string = searchService.name
output storageAccountName string = storageAccount.name
output storageAccountEndpoint string = storageAccount.properties.primaryEndpoints.blob
