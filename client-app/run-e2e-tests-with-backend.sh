#!/bin/bash
set -e

echo "🚀 Starting E2E Test Runner (with Backend)"
echo "==========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Cleanup function
cleanup() {
    echo -e "\n${YELLOW}🧹 Cleaning up...${NC}"
    if [ ! -z "$BACKEND_PID" ]; then
        echo "Stopping backend (PID: $BACKEND_PID)"
        kill $BACKEND_PID 2>/dev/null || true
    fi
}

trap cleanup EXIT

# Step 1: Build backend
echo -e "${YELLOW}Step 1: Building backend...${NC}"
cd ../
dotnet build --configuration Release
cd client-app

# Step 2: Start backend in background
echo -e "\n${YELLOW}Step 2: Starting backend...${NC}"
cd ../src/SincoMaquinaria
export ConnectionStrings__DefaultConnection="Host=localhost;Database=SincoMaquinaria_Test;Username=postgres;Password=postgres"
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://localhost:5000"

dotnet run --no-build --configuration Release > ../../backend-e2e.log 2>&1 &
BACKEND_PID=$!
echo "Backend started with PID: $BACKEND_PID"

cd ../../client-app

# Step 3: Wait for backend to be ready
echo -e "\n${YELLOW}Step 3: Waiting for backend to be ready...${NC}"
for i in {1..60}; do
    if curl -s http://localhost:5000/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Backend is ready after $i seconds${NC}"
        break
    fi
    echo "Attempt $i/60: Backend not ready yet..."
    sleep 1
done

if ! curl -s http://localhost:5000/health > /dev/null 2>&1; then
    echo -e "${RED}❌ Backend did not start within 60 seconds${NC}"
    echo -e "${YELLOW}Backend logs:${NC}"
    cat ../backend-e2e.log
    exit 1
fi

# Step 4: Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo -e "\n${YELLOW}Step 4: Installing dependencies...${NC}"
    npm ci --legacy-peer-deps
fi

# Step 5: Install Playwright browsers if needed
echo -e "\n${YELLOW}Step 5: Ensuring Playwright browsers are installed...${NC}"
npx playwright install chromium firefox --with-deps

# Step 6: Run E2E tests
echo -e "\n${YELLOW}Step 6: Running E2E tests...${NC}"
npm run test:e2e

echo -e "\n${GREEN}✅ E2E tests completed!${NC}"
echo -e "\n📊 View the report with: ${YELLOW}npx playwright show-report${NC}"
