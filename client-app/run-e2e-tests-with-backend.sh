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

# Step 1: Reset test database
echo -e "${YELLOW}Step 1: Resetting test database...${NC}"
cd ../DbReset
dotnet run -c Release --no-build 2>/dev/null || dotnet run -c Release
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Failed to reset database${NC}"
    exit 1
fi
cd ../

# Step 2: Build backend
echo -e "\n${YELLOW}Step 2: Building backend...${NC}"
dotnet build --configuration Release
cd client-app

# Step 3: Start backend in background
echo -e "\n${YELLOW}Step 3: Starting backend...${NC}"

# Kill any existing process on port 5000
echo "Checking for existing processes on port 5000..."
EXISTING_PID=$(netstat -aon 2>/dev/null | grep ':5000 ' | grep 'LISTENING' | awk '{print $NF}' | head -1)
if [ ! -z "$EXISTING_PID" ] && [ "$EXISTING_PID" != "0" ]; then
    echo "Killing existing process on port 5000 (PID: $EXISTING_PID)"
    taskkill //F //PID $EXISTING_PID 2>/dev/null || kill -9 $EXISTING_PID 2>/dev/null || true
    sleep 2
fi

cd ../src/SincoMaquinaria
export ConnectionStrings__DefaultConnection="Host=localhost;Database=SincoMaquinaria_Test;Username=postgres;Password=postgres"
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="http://localhost:5000"
export Caching__Enabled=false

dotnet run --no-build --configuration Release > ../../backend-e2e.log 2>&1 &
BACKEND_PID=$!
echo "Backend started with PID: $BACKEND_PID"

cd ../../client-app

# Step 4: Wait for backend to be ready
echo -e "\n${YELLOW}Step 4: Waiting for backend to be ready...${NC}"
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

# Step 5: Wait for Marten schema to be ready (fresh DB needs schema creation)
echo -e "\n${YELLOW}Step 5: Waiting for database schema to be ready...${NC}"
for i in {1..30}; do
    SETUP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:5000/auth/setup \
        -H "Content-Type: application/json" \
        -d '{"Nombre":"E2E Test Admin","Email":"e2e-test@sinco.com","Password":"TestPassword123"}' 2>/dev/null)

    if [ "$SETUP_STATUS" = "200" ] || [ "$SETUP_STATUS" = "400" ]; then
        echo -e "${GREEN}✅ Database schema ready after $i attempts${NC}"
        break
    fi
    echo "Schema not ready yet (status: $SETUP_STATUS), waiting 2s..."
    sleep 2
done

# Step 5b: Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo -e "\n${YELLOW}Step 4: Installing dependencies...${NC}"
    npm ci --legacy-peer-deps
fi

# Step 6: Install Playwright browsers if needed
echo -e "\n${YELLOW}Step 6: Ensuring Playwright browsers are installed...${NC}"
npx playwright install chromium firefox --with-deps

# Step 7: Run E2E tests
echo -e "\n${YELLOW}Step 7: Running E2E tests...${NC}"
npm run test:e2e

echo -e "\n${GREEN}✅ E2E tests completed!${NC}"
echo -e "\n📊 View the report with: ${YELLOW}npx playwright show-report${NC}"
