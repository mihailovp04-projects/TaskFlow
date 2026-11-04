#!/usr/bin/env bash
# Smoke-тест TaskFlow.Api (неделя 3). Запуск: bash scripts/smoke.sh
BASE_URL="${BASE_URL:-http://localhost:5080}"
FAILED=0

# Отправляет запрос и сравнивает код ответа с ожидаемым
check() {
  local expected="$1" method="$2" path="$3" body="${4:-}"
  local args=(-s -o /dev/null -w "%{http_code}" -X "$method" "$BASE_URL$path")
  if [ -n "$body" ]; then
    args+=(-H "Content-Type: application/json" -d "$body")
  fi
  local actual
  actual=$(curl "${args[@]}")
  if [ "$actual" = "$expected" ]; then
    echo "OK   $actual $method $path"
  else
    echo "FAIL $actual (expected $expected) $method $path"
    FAILED=1
  fi
}

# Создаёт ресурс через POST и печатает id из заголовка Location
create() {
  local path="$1" body="$2"
  curl -s -o /dev/null -D - -X POST "$BASE_URL$path" \
    -H "Content-Type: application/json" -d "$body" \
    | tr -d '\r' | awk 'tolower($1) == "location:" { print $2 }' | sed 's#.*/##'
}

SUFFIX=$(date +%s)
PROJECT_NAME="Smoke $SUFFIX"

PROJECT_ID=$(create /api/projects "{\"name\":\"$PROJECT_NAME\"}")
TASK_ID=$(create /api/tasks "{\"title\":\"Smoke task $SUFFIX\",\"priority\":\"High\",\"projectId\":$PROJECT_ID}")

if [ -z "$PROJECT_ID" ] || [ -z "$TASK_ID" ]; then
  echo "FAIL could not create project or task (is the API running on $BASE_URL?)"
  exit 1
fi
echo "Created project $PROJECT_ID and task $TASK_ID"

check 200 GET /api/projects
check 200 GET "/api/projects/$PROJECT_ID"
check 409 POST /api/projects "{\"name\":\"$PROJECT_NAME\"}"
check 400 POST /api/projects '{"name":""}'
check 200 GET "/api/projects/$PROJECT_ID/tasks"
check 404 GET /api/projects/999999/tasks
check 200 GET /api/tasks
check 200 GET "/api/tasks?status=Todo&priority=High"
check 200 GET "/api/tasks/$TASK_ID"
check 404 GET /api/tasks/999999
check 404 GET /api/tasks/abc
check 400 POST /api/tasks '{"title":""}'
check 400 POST /api/tasks '{"title":"Bad priority","priority":"Urgent"}'
check 204 PUT "/api/tasks/$TASK_ID" '{"title":"Smoke task updated","priority":"Critical"}'
check 404 PUT /api/tasks/999999 '{"title":"Nobody","priority":"Low"}'
check 204 PATCH "/api/tasks/$TASK_ID/status" '{"status":"InProgress"}'
check 409 PATCH "/api/tasks/$TASK_ID/status" '{"status":"InProgress"}'
check 204 PATCH "/api/tasks/$TASK_ID/status" '{"status":"Done"}'
check 400 PATCH "/api/tasks/$TASK_ID/status" '{}'
check 404 PATCH /api/tasks/999999/status '{"status":"Done"}'
check 204 DELETE "/api/tasks/$TASK_ID"
check 404 DELETE "/api/tasks/$TASK_ID"
check 200 GET /api/diagnostics/lifetimes
check 500 GET /api/diagnostics/boom
check 200 GET /openapi/v1.json
check 200 GET /scalar/v1

echo
if [ "$FAILED" -eq 0 ]; then
  echo "All checks passed"
else
  echo "Some checks failed"
fi
exit "$FAILED"
