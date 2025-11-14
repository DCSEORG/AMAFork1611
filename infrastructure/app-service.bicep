// Azure App Service for Expense Management System
// Location: UK South
// SKU: Development (F1 Free tier)

@description('The name of the App Service')
param appServiceName string = 'app-expense-mgmt-${uniqueString(resourceGroup().id)}'

@description('The name of the App Service Plan')
param appServicePlanName string = 'plan-expense-mgmt-${uniqueString(resourceGroup().id)}'

@description('Location for all resources')
param location string = 'uksouth'

@description('The SKU of the App Service Plan')
@allowed([
  'F1'  // Free tier for development
  'B1'  // Basic tier for low-cost dev
])
param sku string = 'B1'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: sku
    tier: sku == 'F1' ? 'Free' : 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: sku != 'F1' // Always On not available on Free tier
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
  }
}

output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServiceName string = appService.name
output appServicePlanName string = appServicePlan.name
