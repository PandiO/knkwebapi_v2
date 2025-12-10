#!/bin/bash
BASE_URL="http://localhost:5294/api"

echo "=== Clean database first ==="
curl -s -X DELETE "$BASE_URL/displayconfigurations/2" > /dev/null
echo "Cleaned"
echo ""

echo "=== Test 1: Create DisplayConfiguration ==="
RESULT=$(curl -s -X POST "$BASE_URL/displayconfigurations" \
  -H "Content-Type: application/json" \
  -d '{"name":"Town Display Config","entityTypeName":"Town","isDefault":true,"description":"Default display for towns"}')
echo "$RESULT" | python3 -m json.tool
CONFIG_ID=$(echo "$RESULT" | python3 -c "import sys, json; print(json.load(sys.stdin).get('id', ''))" 2>/dev/null)
echo "Created ID: $CONFIG_ID"
echo ""

echo "=== Test 2: GET entity-names ==="
curl -s "$BASE_URL/displayconfigurations/entity-names" | python3 -m json.tool
echo ""

echo "=== Test 3: GET default for Town ==="
curl -s "$BASE_URL/displayconfigurations/entity/Town" | python3 -m json.tool | head -15
echo ""

echo "=== Test 4: Update configuration ==="
curl -s -X PUT "$BASE_URL/displayconfigurations/$CONFIG_ID" \
  -H "Content-Type: application/json" \
  -d "{\"id\":\"$CONFIG_ID\",\"name\":\"Updated Town Display\",\"entityTypeName\":\"Town\",\"isDefault\":true,\"description\":\"Updated description\",\"isDraft\":true,\"sectionOrderJson\":\"[]\"}"
echo "Updated (no content = 204)"
echo ""

echo "=== Test 5: Try to publish (should fail - no sections) ==="
curl -s -X POST "$BASE_URL/displayconfigurations/$CONFIG_ID/publish" | python3 -m json.tool
echo ""

echo "=== Test 6: Create reusable section ==="
SECTION_RESULT=$(curl -s -X POST "$BASE_URL/displaysections/reusable" \
  -H "Content-Type: application/json" \
  -d '{"sectionName":"General Info Section","description":"Reusable general info","fieldOrderJson":"[]","actionButtonsConfigJson":"{}"}')
echo "$SECTION_RESULT" | python3 -m json.tool
SECTION_ID=$(echo "$SECTION_RESULT" | python3 -c "import sys, json; print(json.load(sys.stdin).get('id', ''))" 2>/dev/null)
echo "Created Section ID: $SECTION_ID"
echo ""

echo "=== Test 7: GET all reusable sections ==="
curl -s "$BASE_URL/displaysections/reusable" | python3 -m json.tool | head -20
echo ""

echo "=== Test 8: Clone section (Copy mode) ==="
curl -s -X POST "$BASE_URL/displaysections/$SECTION_ID/clone" \
  -H "Content-Type: application/json" \
  -d '{"linkMode":0}' | python3 -m json.tool | head -15
echo ""

echo "=== Test 9: Delete configuration ==="
curl -s -X DELETE "$BASE_URL/displayconfigurations/$CONFIG_ID"
echo "Deleted (no content = 204)"
echo ""

echo "=== All Phase 3-4 tests completed! ==="
