#!/bin/bash
set -e

BASE_URL="${1:-http://localhost:5000}"
CURL="curl -s -m 10"
PASS=0
FAIL=0
ERRORS=""

assert_status() {
    local name="$1"
    local expected="$2"
    local actual="$3"
    if [ "$actual" -eq "$expected" ]; then
        echo "  PASS  $name (${actual})"
        PASS=$((PASS + 1))
    else
        echo "  FAIL  $name (expected ${expected}, got ${actual})"
        FAIL=$((FAIL + 1))
        ERRORS="${ERRORS}\n  - ${name}: expected ${expected}, got ${actual}"
    fi
}

echo "=== CatCatGo API Integration Test ==="
echo "Target: $BASE_URL"
echo ""

echo "[Health Check]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/")
assert_status "GET /" 200 "$STATUS"

echo ""
echo "[Auth]"
DEVICE_ID="inttest-$(date +%s)"
REGISTER_BODY=$($CURL -X POST "$BASE_URL/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"deviceId\":\"$DEVICE_ID\",\"deviceName\":\"IntegrationTest\"}")
STATUS=$(echo "$REGISTER_BODY" | grep -o '"accessToken"' > /dev/null 2>&1 && echo 200 || echo 500)
TOKEN=$(echo "$REGISTER_BODY" | sed -n 's/.*"accessToken":"\([^"]*\)".*/\1/p')
REFRESH_TOKEN=$(echo "$REGISTER_BODY" | sed -n 's/.*"refreshToken":"\([^"]*\)".*/\1/p')

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/refresh" \
    -H "Content-Type: application/json" \
    -d "{\"refreshToken\":\"$REFRESH_TOKEN\"}")
assert_status "POST /api/auth/refresh" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"deviceId\":\"$DEVICE_ID\"}")
assert_status "POST /api/auth/login" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"deviceId":"nonexistent-device-xyz"}')
assert_status "POST /api/auth/login (unknown device -> 401)" 401 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"deviceId\":\"$DEVICE_ID\",\"deviceName\":\"IntegrationTest\"}")
assert_status "POST /api/auth/register (duplicate -> 200)" 200 "$STATUS"

AUTH="Authorization: Bearer $TOKEN"

echo ""
echo "[Save]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/save" -H "$AUTH")
assert_status "GET /api/save (no save -> 404)" 404 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/save/sync" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"data":"{\"gold\":100}","clientTimestamp":1000000,"version":1,"checksum":"test123"}')
assert_status "POST /api/save/sync" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/save" -H "$AUTH")
assert_status "GET /api/save (after sync -> 200)" 200 "$STATUS"

echo ""
echo "[Battle]"
BATTLE_BODY=$($CURL -X POST "$BASE_URL/api/battle/start" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"chapterId":1,"day":1,"encounterType":"normal"}')
STATUS=$(echo "$BATTLE_BODY" | grep -o '"battleId"' > /dev/null 2>&1 && echo 200 || echo 500)
assert_status "POST /api/battle/start" 200 "$STATUS"

BATTLE_ID=$(echo "$BATTLE_BODY" | sed -n 's/.*"battleId":"\([^"]*\)".*/\1/p')
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/battle/report" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d "{\"battleId\":\"$BATTLE_ID\",\"seed\":123,\"result\":\"win\",\"turnCount\":5,\"playerSkillIds\":[\"s1\"],\"enemyTemplateId\":\"e1\",\"goldReward\":50}")
assert_status "POST /api/battle/report" 200 "$STATUS"

echo ""
echo "[Shop]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/shop/catalog" -H "$AUTH")
assert_status "GET /api/shop/catalog" 200 "$STATUS"

echo ""
echo "[Arena]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/arena/match" -H "$AUTH")
assert_status "POST /api/arena/match" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/arena/ranking" -H "$AUTH")
assert_status "GET /api/arena/ranking" 200 "$STATUS"

echo ""
echo "[Gacha]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/gacha/pull" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"poolId":"standard","count":1}')
assert_status "POST /api/gacha/pull" 200 "$STATUS"

echo ""
echo "[Cleanup]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/reset-data" -H "$AUTH")
assert_status "POST /api/auth/reset-data" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X DELETE "$BASE_URL/api/auth/account" -H "$AUTH")
assert_status "DELETE /api/auth/account" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"deviceId\":\"$DEVICE_ID\"}")
assert_status "POST /api/auth/login (deleted -> 401)" 401 "$STATUS"

echo ""
echo "==============================="
echo "  Total: $((PASS + FAIL))  Pass: $PASS  Fail: $FAIL"
if [ $FAIL -gt 0 ]; then
    echo -e "  Failures:$ERRORS"
    echo "==============================="
    exit 1
else
    echo "  All tests passed!"
    echo "==============================="
    exit 0
fi
