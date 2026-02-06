#!/bin/bash

# ========================================
# SincoMaquinaria - Azure Deployment Script
# ========================================
# This script deploys the infrastructure and application to Azure
# Prerequisites:
# - Azure CLI installed and logged in
# - Bicep CLI installed
# - Required environment variables set or .env file present

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed. Please install it from https://docs.microsoft.com/cli/azure/install-azure-cli"
    exit 1
fi

print_success "Azure CLI found"

# Check if Bicep is installed
if ! command -v az bicep &> /dev/null; then
    print_info "Installing Bicep..."
    az bicep install
fi

print_success "Bicep CLI found"

# Check if logged in to Azure
print_info "Checking Azure login status..."
if ! az account show &> /dev/null; then
    print_error "Not logged in to Azure. Running 'az login'..."
    az login
fi

print_success "Logged in to Azure"

# Get current directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Load environment variables from .env file if it exists
if [ -f "$SCRIPT_DIR/.env" ]; then
    print_info "Loading environment variables from .env file..."
    export $(cat "$SCRIPT_DIR/.env" | grep -v '^#' | xargs)
fi

# Set default values
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-sincomaquinaria-prod}"
LOCATION="${LOCATION:-eastus}"
ENVIRONMENT="${ENVIRONMENT:-prod}"
BASE_NAME="${BASE_NAME:-sincomaquinaria}"

# Prompt for required secrets if not set
if [ -z "$POSTGRES_ADMIN_PASSWORD" ]; then
    read -sp "Enter PostgreSQL administrator password: " POSTGRES_ADMIN_PASSWORD
    echo
fi

if [ -z "$JWT_SECRET_KEY" ]; then
    read -sp "Enter JWT secret key: " JWT_SECRET_KEY
    echo
fi

# Validate required parameters
if [ -z "$POSTGRES_ADMIN_PASSWORD" ] || [ -z "$JWT_SECRET_KEY" ]; then
    print_error "Required parameters missing. Please set POSTGRES_ADMIN_PASSWORD and JWT_SECRET_KEY"
    exit 1
fi

print_info "Deployment Configuration:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  Environment: $ENVIRONMENT"
echo "  Base Name: $BASE_NAME"
echo

# Create resource group if it doesn't exist
print_info "Creating resource group if it doesn't exist..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
print_success "Resource group ready"

# Deploy Bicep template
print_info "Deploying infrastructure to Azure..."
DEPLOYMENT_NAME="sincomaquinaria-deploy-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$SCRIPT_DIR/main.bicep" \
    --parameters "$SCRIPT_DIR/parameters.json" \
    --parameters postgresAdminPassword="$POSTGRES_ADMIN_PASSWORD" \
    --parameters jwtSecretKey="$JWT_SECRET_KEY" \
    --parameters environment="$ENVIRONMENT" \
    --parameters baseName="$BASE_NAME" \
    --parameters location="$LOCATION"

print_success "Infrastructure deployed successfully!"

# Get deployment outputs
print_info "Retrieving deployment outputs..."
CONTAINER_APP_URL=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.containerAppUrl.value \
    -o tsv)

ACR_LOGIN_SERVER=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.acrLoginServer.value \
    -o tsv)

POSTGRES_FQDN=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.postgresServerFqdn.value \
    -o tsv)

REDIS_HOSTNAME=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.redisCacheHostname.value \
    -o tsv)

APP_INSIGHTS_KEY=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.appInsightsInstrumentationKey.value \
    -o tsv)

echo
print_success "Deployment completed successfully!"
echo
echo "================================================"
echo "Deployment Information"
echo "================================================"
echo "Container App URL: https://$CONTAINER_APP_URL"
echo "ACR Login Server: $ACR_LOGIN_SERVER"
echo "PostgreSQL Server: $POSTGRES_FQDN"
echo "Redis Cache: $REDIS_HOSTNAME"
echo "Application Insights Key: $APP_INSIGHTS_KEY"
echo "================================================"
echo
print_info "Next steps:"
echo "  1. Build and push Docker image to ACR"
echo "  2. The Container App will automatically pull and deploy the image"
echo "  3. Access your application at: https://$CONTAINER_APP_URL"
echo
print_info "To push the Docker image manually:"
echo "  az acr login --name ${ACR_LOGIN_SERVER%%.*}"
echo "  docker build -t $ACR_LOGIN_SERVER/sincomaquinaria:latest ."
echo "  docker push $ACR_LOGIN_SERVER/sincomaquinaria:latest"
echo
