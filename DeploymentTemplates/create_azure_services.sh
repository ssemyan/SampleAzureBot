#!/bin/bash

# exit when any command fails
set -e

# Use the az CLI to create the necessary Azure elements

# An Azure AD App is used to communicate between the bot service and the web application. An AD App can be only used with one bot service.

AZURE_AD_APP_NAME="" # Display name should be unique. Creating a new Azure AD App with duplicate name will just overwrite the password of the existing one. 
AZURE_AD_APP_ID="" # Set this to an existing AppID or leave blank to create a new app. Note: the App ID can only be used to register one bot.
AZURE_AD_APP_PASS="" # Password associated with the Azure Active Directory App

LOCATION=westus2 # Azure Region to create all resources
RG_NAME=MySampleBot # Resource Group

PLAN_NAME=MyBotServicePlan # App Service Plan to create or reuse
PLAN_SKU=S1 # SKU to use for the plan 

APP_SERVICE_NAME=MyUniqueBotSiteName # Azure App Service name to create or reuse. Name is part of the URI and so must be unique

BOT_REG_NAME=MyUniqueBotRegName # Name of the Azure Bot Service registration. This name must be globally unique
BOT_SVC_SKU=F0 # SKU for the bot service

# If the AppId is not set, a new Azure AD App will be created
if [ -z "$AZURE_AD_APP_ID" ]; then
    echo "Creating new Azure AD App."
    AZURE_AD_APP_ID="$(az ad app create --display-name "$AZURE_AD_APP_NAME" --password "$AZURE_AD_APP_PASS" --available-to-other-tenants --query "appId" -o tsv)"
    echo "Created new Azure AD App with ID $AZURE_AD_APP_ID"
else
    echo "Using existing Azure AD App $AZURE_AD_APP_ID."
fi

# Create Resource Group if not exists
if [ $(az group exists --name $RG_NAME) = false ]; then
    echo "Creating resource group $RG_NAME in $LOCATION."
    az group create -n $RG_NAME --location $LOCATION -o none
else
    echo "Using existing resource group $RG_NAME."
fi

# Create App Service Plan if not exists
if   [ -z "$(az appservice plan show --name $PLAN_NAME -g $RG_NAME)" ]; then
    echo "Creating new app service plan $PLAN_NAME in location $LOCATION"
    az appservice plan create --name $PLAN_NAME -g $RG_NAME --sku $PLAN_SKU --location $LOCATION -o none
else
    echo "Using existing app service plan $PLAN_NAME"
fi

# Create Web App if not exists
if   [ "$(az webapp list --query "[?name == '$APP_SERVICE_NAME']" -g $RG_NAME)" = "[]" ]; then
    echo "Creating new app service $APP_SERVICE_NAME"
    az webapp create --name $APP_SERVICE_NAME -g $RG_NAME -p $PLAN_NAME -o none
else
    echo "Using existing app service $APP_SERVICE_NAME"
fi

# Set app settings
echo "Setting app settings"
az webapp config appsettings set -g $RG_NAME -n $APP_SERVICE_NAME --settings MicrosoftAppId=$AZURE_AD_APP_ID MicrosoftAppPassword=$AZURE_AD_APP_PASS -o none
az webapp config set -g $RG_NAME -n $APP_SERVICE_NAME --web-sockets-enabled true -o none

# Create Bot Registration if not exists
if   [ "$(az resource list --resource-group $RG_NAME --resource-type "Microsoft.BotService/botServices" --query "[?name == '$BOT_REG_NAME']")" = "[]" ]; then
    echo "Creating new bot Registration $BOT_REG_NAME"
    az bot create --appid $AZURE_AD_APP_ID --kind registration --name $BOT_REG_NAME --resource-group $RG_NAME --location $LOCATION \
                  --endpoint "https://$APP_SERVICE_NAME.azurewebsites.net/api/messages" --password $AZURE_AD_APP_PASS --sku $BOT_SVC_SKU -o none
else
    echo "Using existing bot registration $BOT_REG_NAME"
fi

echo "Done."