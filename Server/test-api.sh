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

assert_json_field() {
    local name="$1"
    local body="$2"
    local field="$3"
    if echo "$body" | grep -q "\"$field\""; then
        echo "  PASS  $name (has $field)"
        PASS=$((PASS + 1))
    else
        echo "  FAIL  $name (missing $field)"
        FAIL=$((FAIL + 1))
        ERRORS="${ERRORS}\n  - ${name}: missing field $field"
    fi
}

echo "=== CatCatGo API Integration Test ==="
echo "Target: $BASE_URL"
echo ""

# ─── Health Check ───
echo "[Health Check]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/")
assert_status "GET /" 200 "$STATUS"

# ─── Auth ───
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

# ─── Save ───
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

# ─── Talent (ApiResponse + StateDelta) ───
echo ""
echo "[Talent]"
TALENT_BODY=$($CURL -X POST "$BASE_URL/api/talent/upgrade" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"statType":"ATK"}')
STATUS=$(echo "$TALENT_BODY" | grep -q '"success"' && echo 200 || echo 500)
assert_status "POST /api/talent/upgrade" 200 "$STATUS"
assert_json_field "POST /api/talent/upgrade (has delta)" "$TALENT_BODY" "delta"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/talent/status" -H "$AUTH")
assert_status "GET /api/talent/status" 200 "$STATUS"

MILESTONE_BODY=$($CURL -X POST "$BASE_URL/api/talent/claim-milestone" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"milestoneLevel":10}')
STATUS=$(echo "$MILESTONE_BODY" | grep -q '"success"' && echo 200 || echo 500)
assert_status "POST /api/talent/claim-milestone" 200 "$STATUS"

CLAIM_ALL_BODY=$($CURL -X POST "$BASE_URL/api/talent/claim-all-milestones" \
    -H "$AUTH" -H "Content-Type: application/json")
STATUS=$(echo "$CLAIM_ALL_BODY" | grep -q '"success"' && echo 200 || echo 500)
assert_status "POST /api/talent/claim-all-milestones" 200 "$STATUS"

# ─── Equipment (ApiResponse + StateDelta) ───
echo ""
echo "[Equipment]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/equipment/upgrade" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"equipmentId":"test-eq-1"}')
assert_status "POST /api/equipment/upgrade" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/equipment/equip" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"equipmentId":"test-eq-1"}')
assert_status "POST /api/equipment/equip" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/equipment/unequip" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"slotType":"WEAPON","slotIndex":0}')
assert_status "POST /api/equipment/unequip" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/equipment/sell" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"equipmentId":"test-eq-1"}')
assert_status "POST /api/equipment/sell" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/equipment/forge" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"equipmentIds":["eq-1","eq-2","eq-3"]}')
assert_status "POST /api/equipment/forge" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/equipment/bulk-forge" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/equipment/bulk-forge" 200 "$STATUS"

# ─── Pet (ApiResponse + StateDelta) ───
echo ""
echo "[Pet]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/pet/hatch" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/pet/hatch" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/pet/feed" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"petId":"test-pet-1","amount":1}')
assert_status "POST /api/pet/feed" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/pet/deploy" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"petId":"test-pet-1"}')
assert_status "POST /api/pet/deploy" 200 "$STATUS"

# ─── Gacha (ApiResponse + StateDelta) ───
echo ""
echo "[Gacha]"
GACHA_BODY=$($CURL -X POST "$BASE_URL/api/gacha/pull" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"chestType":"EQUIPMENT","count":1}')
STATUS=$(echo "$GACHA_BODY" | grep -q '"success"' && echo 200 || echo 500)
assert_status "POST /api/gacha/pull" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/gacha/pull10" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/gacha/pull10" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/gacha/pity" -H "$AUTH")
assert_status "GET /api/gacha/pity" 200 "$STATUS"

# ─── Heritage (ApiResponse + StateDelta) ───
echo ""
echo "[Heritage]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/heritage/upgrade" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"route":"SKULL"}')
assert_status "POST /api/heritage/upgrade" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/heritage/status" -H "$AUTH")
assert_status "GET /api/heritage/status" 200 "$STATUS"

# ─── Chapter (ApiResponse + StateDelta) ───
echo ""
echo "[Chapter]"
CHAPTER_BODY=$($CURL -X POST "$BASE_URL/api/chapter/start" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"chapterId":1,"chapterType":"SIXTY_DAY"}')
STATUS=$(echo "$CHAPTER_BODY" | grep -q '"success"' && echo 200 || echo 500)
assert_status "POST /api/chapter/start" 200 "$STATUS"
SESSION_ID=$(echo "$CHAPTER_BODY" | sed -n 's/.*"sessionId":"\([^"]*\)".*/\1/p')

if [ -n "$SESSION_ID" ]; then
    STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/chapter/advance-day" \
        -H "$AUTH" -H "Content-Type: application/json" \
        -d "{\"sessionId\":\"$SESSION_ID\"}")
    assert_status "POST /api/chapter/advance-day" 200 "$STATUS"

    STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/chapter/resolve-encounter" \
        -H "$AUTH" -H "Content-Type: application/json" \
        -d "{\"sessionId\":\"$SESSION_ID\",\"choiceIndex\":0}")
    assert_status "POST /api/chapter/resolve-encounter" 200 "$STATUS"

    STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/chapter/select-skill" \
        -H "$AUTH" -H "Content-Type: application/json" \
        -d "{\"sessionId\":\"$SESSION_ID\",\"skillId\":\"skill_1\"}")
    assert_status "POST /api/chapter/select-skill" 200 "$STATUS"

    STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/chapter/reroll" \
        -H "$AUTH" -H "Content-Type: application/json" \
        -d "{\"sessionId\":\"$SESSION_ID\"}")
    assert_status "POST /api/chapter/reroll" 200 "$STATUS"

    STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/chapter/battle-result" \
        -H "$AUTH" -H "Content-Type: application/json" \
        -d "{\"sessionId\":\"$SESSION_ID\",\"battleSeed\":123,\"result\":\"win\",\"turnCount\":5,\"playerRemainingHp\":80}")
    assert_status "POST /api/chapter/battle-result" 200 "$STATUS"

    STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/chapter/abandon" \
        -H "$AUTH" -H "Content-Type: application/json" \
        -d "{\"sessionId\":\"$SESSION_ID\"}")
    assert_status "POST /api/chapter/abandon" 200 "$STATUS"
fi

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/chapter/state" -H "$AUTH")
assert_status "GET /api/chapter/state" 200 "$STATUS"

# ─── Treasure (ApiResponse + StateDelta) ───
echo ""
echo "[Treasure]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/treasure/claim" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"milestoneId":"milestone_1"}')
assert_status "POST /api/treasure/claim" 200 "$STATUS"

# ─── Content (ApiResponse + StateDelta) ───
echo ""
echo "[Content]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/tower/challenge" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/content/tower/challenge" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/dungeon/challenge" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"dungeonType":"BEEHIVE"}')
assert_status "POST /api/content/dungeon/challenge" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/dungeon/sweep" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"dungeonType":"BEEHIVE"}')
assert_status "POST /api/content/dungeon/sweep" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/goblin/mine" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/content/goblin/mine" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/goblin/cart" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/content/goblin/cart" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/catacomb/start" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/content/catacomb/start" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/catacomb/battle" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/content/catacomb/battle" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/content/catacomb/end" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/content/catacomb/end" 200 "$STATUS"

# ─── Daily (ApiResponse + StateDelta) ───
echo ""
echo "[Daily]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/daily/attendance" -H "$AUTH")
assert_status "GET /api/daily/attendance" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/daily/attendance/claim" \
    -H "$AUTH" -H "Content-Type: application/json")
assert_status "POST /api/daily/attendance/claim" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/daily/quest" -H "$AUTH")
assert_status "GET /api/daily/quest" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/daily/quest/claim" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"eventId":"daily_event","missionId":"daily_kill_10"}')
assert_status "POST /api/daily/quest/claim" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/daily/quest/claim-all" \
    -H "$AUTH" -H "Content-Type: application/json" \
    -d '{"eventId":"daily_event"}')
assert_status "POST /api/daily/quest/claim-all" 200 "$STATUS"

# ─── Battle ───
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

# ─── Shop ───
echo ""
echo "[Shop]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/shop/catalog" -H "$AUTH")
assert_status "GET /api/shop/catalog" 200 "$STATUS"

# ─── Arena ───
echo ""
echo "[Arena]"
STATUS=$($CURL -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/arena/match" -H "$AUTH")
assert_status "POST /api/arena/match" 200 "$STATUS"

STATUS=$($CURL -o /dev/null -w "%{http_code}" "$BASE_URL/api/arena/ranking" -H "$AUTH")
assert_status "GET /api/arena/ranking" 200 "$STATUS"

# ─── Cleanup ───
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

# ─── Summary ───
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
