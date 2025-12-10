#!/bin/bash

echo "=== Test 1: GET all configurations (empty) ==="
curl -s http://localhost:5294/api/displayconfigurations | python3 -m json.tool
echo ""

echo "=== Test 2: Create DisplayConfiguration ==="
RESULT=$(curl -s -X POST http://localhost:5294/api/displayconfigurations \
  -H "Content-Type: application/json" \
  -d '{"name":"Town Display","entityTypeName":"Town","isDefault":true,"description":"Test config"}')
echo "$RESULT" | python3 -m json.tool
CONFIG_ID=$(echo "$RESULT" | python3 -c "import sys, json; print(json.load(sys.stdin).get('id', ''))")
echo "Created config ID: $CONFIG_ID"
echo ""

echo "=== Test 3: GET by ID ==="
curl -s "http://localhost:5294/api/displayconfigurations/$CONFIG_ID" | python3 -m json.tool
echo ""

echo "=== Test 4: Publish config ==="
curl -s -X POST "http://localhost:5294/api/displayconfigurations/$CONFIG_ID/publish" | python3 -m json.tool
echo ""

echo "=== Test 5: GET published configs only ==="
curl -s "http://localhost:5294/api/displayconfigurations?includeDrafts=false" | python3 -m json.tool
