#!/bin/bash

# Start the API server in the background
dotnet run &

# Store the process ID
API_PID=$!

# Wait for the server to start (max 10 seconds)
echo "Waiting for API server to start..."
for i in {1..20}; do
    if curl -s http://localhost:5294/swagger/index.html > /dev/null; then
        echo "API server is ready!"
        break
    fi
    sleep 0.5
done

# Open Swagger in the default browser
open http://localhost:5294/swagger

# Wait for the API process to finish
wait $API_PID
