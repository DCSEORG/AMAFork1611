#!/bin/bash

# Expense Management System - Azure Deployment Script
# This script deploys all Azure infrastructure and application code

set -e  # Exit on error

# Configuration
RESOURCE_GROUP="rg-expense-management"
LOCATION="uksouth"
INCLUDE_CHAT_UI="${INCLUDE_CHAT_UI:-false}"  # Default to false

echo "======================================"
echo "Expense Management System Deployment"
echo "======================================"
echo "Resource Group: $RESOURCE_GROUP"
echo "Location: $LOCATION"
echo "Include Chat UI: $INCLUDE_CHAT_UI"
echo "======================================"
echo ""

# Check if Azure CLI is logged in
if ! az account show &> /dev/null; then
    echo "Error: Not logged in to Azure CLI. Please run 'az login' first."
    exit 1
fi

# Create Resource Group
echo "Creating resource group..."
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION \
    --output table

# Deploy App Service
echo ""
echo "Deploying App Service infrastructure..."
APP_SERVICE_OUTPUT=$(az deployment group create \
    --resource-group $RESOURCE_GROUP \
    --template-file infrastructure/app-service.bicep \
    --query 'properties.outputs' \
    --output json)

APP_SERVICE_NAME=$(echo $APP_SERVICE_OUTPUT | jq -r '.appServiceName.value')
APP_SERVICE_URL=$(echo $APP_SERVICE_OUTPUT | jq -r '.appServiceUrl.value')

echo "App Service Name: $APP_SERVICE_NAME"
echo "App Service URL: $APP_SERVICE_URL"

# Deploy Application Code
echo ""
echo "Deploying application code..."
az webapp deploy \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --src-path ./app.zip \
    --type zip \
    --async false

# Deploy GenAI resources if enabled
if [ "$INCLUDE_CHAT_UI" = "true" ]; then
    echo ""
    echo "Deploying GenAI resources..."
    
    if [ -f "infrastructure/genai-resources.bicep" ]; then
        GENAI_OUTPUT=$(az deployment group create \
            --resource-group $RESOURCE_GROUP \
            --template-file infrastructure/genai-resources.bicep \
            --query 'properties.outputs' \
            --output json)
        
        echo "GenAI resources deployed successfully"
        echo "Check GenAISettings.json for configuration details"
    else
        echo "Warning: GenAI infrastructure file not found. Skipping GenAI deployment."
    fi
else
    echo ""
    echo "Skipping GenAI resources (INCLUDE_CHAT_UI=false)"
    echo "To deploy with Chat UI, set INCLUDE_CHAT_UI=true before running this script:"
    echo "  export INCLUDE_CHAT_UI=true"
    echo "  ./deploy.sh"
fi

echo ""
echo "======================================"
echo "Deployment Complete!"
echo "======================================"
echo ""
echo "Application URL: $APP_SERVICE_URL/Index"
echo ""
echo "IMPORTANT: Navigate to $APP_SERVICE_URL/Index (not just the root URL)"
echo ""
echo "API Documentation (Swagger): $APP_SERVICE_URL/swagger"
echo ""
echo "Next steps:"
echo "1. Ensure your Azure SQL database is set up with the schema from Database-Schema/database_schema.sql"
echo "2. Configure managed identity or SQL authentication for the App Service"
echo "3. Navigate to $APP_SERVICE_URL/Index to use the application"
echo ""
