#!/bin/bash
set -e

echo "🚀 Starting E2E Test Runner (Local)"
echo "===================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Step 1: Check if backend is running
echo -e "\n${YELLOW}Step 1: Checking if backend is running...${NC}"
if curl -s http://localhost:5000/health > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Backend is already running on port 5000${NC}"
else
    echo -e "${RED}❌ Backend is NOT running${NC}"
    echo -e "${YELLOW}Please start the backend first with:${NC}"
    echo -e "  cd ../src/SincoMaquinaria && dotnet run"
    echo -e "\nOr in another terminal run:"
    echo -e "  ./run-e2e-tests-with-backend.sh"
    exit 1
fi

# Step 2: Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo -e "\n${YELLOW}Step 2: Installing dependencies...${NC}"
    npm ci --legacy-peer-deps
fi

# Step 3: Install Playwright browsers if needed
echo -e "\n${YELLOW}Step 3: Ensuring Playwright browsers are installed...${NC}"
npx playwright install chromium firefox --with-deps

# Step 4: Run E2E tests
echo -e "\n${YELLOW}Step 4: Running E2E tests...${NC}"
npm run test:e2e

echo -e "\n${GREEN}✅ E2E tests completed!${NC}"
echo -e "\n📊 View the report with: ${YELLOW}npx playwright show-report${NC}"
