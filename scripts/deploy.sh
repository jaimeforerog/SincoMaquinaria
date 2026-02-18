#!/bin/bash

# Exit on error
set -e

echo "Starting deployment..."

# Build Frontend
echo "Building frontend..."
cd client-app
npm ci --legacy-peer-deps
npm run build
cd ..

# Build Backend
echo "Building backend..."
dotnet publish SincoMaquinaria.csproj -c Release -o $DEPLOYMENT_TARGET

# Copy frontend to wwwroot
echo "Copying frontend to wwwroot..."
mkdir -p $DEPLOYMENT_TARGET/wwwroot
cp -r client-app/dist/* $DEPLOYMENT_TARGET/wwwroot/

echo "Deployment complete!"
