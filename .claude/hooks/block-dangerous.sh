#!/bin/bash
# PreToolUse hook: Bash 도구의 위험 명령 차단
# stdin으로 JSON이 전달됨 (tool_input.command 필드)

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | python -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null)

BLOCKED=false
REASON=""

if echo "$COMMAND" | grep -qE 'git\s+push\s+.*--force|git\s+push\s+-f\b'; then
  BLOCKED=true
  REASON="git push --force 감지. force push는 원격 히스토리를 파괴합니다. 정말 필요하면 사용자에게 확인하세요."
fi

if echo "$COMMAND" | grep -qE 'git\s+reset\s+--hard'; then
  BLOCKED=true
  REASON="git reset --hard 감지. 커밋되지 않은 변경사항이 모두 삭제됩니다. 정말 필요하면 사용자에게 확인하세요."
fi

if echo "$COMMAND" | grep -qE 'rm\s+-rf\s+/|rm\s+-rf\s+\.\s'; then
  BLOCKED=true
  REASON="rm -rf 위험 경로 감지. 루트 또는 현재 디렉토리 전체 삭제는 차단됩니다."
fi

if echo "$COMMAND" | grep -qE 'git\s+clean\s+-f'; then
  BLOCKED=true
  REASON="git clean -f 감지. 추적되지 않는 파일이 모두 삭제됩니다. 정말 필요하면 사용자에게 확인하세요."
fi

if [ "$BLOCKED" = true ]; then
  echo "$REASON"
  exit 2
fi

exit 0
