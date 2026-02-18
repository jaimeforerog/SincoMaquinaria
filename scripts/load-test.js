const { execSync } = require('child_process');

async function run() {
    try {
        console.log('Authenticating...');
        const loginResponse = await fetch('http://localhost:5000/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: 'admin@sinco.com',
                password: 'Admin123!'
            })
        });

        if (!loginResponse.ok) {
            const text = await loginResponse.text();
            throw new Error(`Login failed: ${loginResponse.status} ${loginResponse.statusText} - ${text}`);
        }

        const { token } = await loginResponse.json();
        console.log('Login successful. Token obtained.');

        console.log('Starting load test on GET /ordenes ...');
        // Run npx autocannon
        // -c 10: 10 concurrent connections
        // -d 10: 10 seconds duration
        const cmd = `npx autocannon -c 10 -d 10 --headers "Authorization: Bearer ${token}" http://localhost:5000/ordenes`;

        execSync(cmd, { stdio: 'inherit' });

    } catch (error) {
        console.error('Load test failed:', error.message);
        process.exit(1);
    }
}

run();
