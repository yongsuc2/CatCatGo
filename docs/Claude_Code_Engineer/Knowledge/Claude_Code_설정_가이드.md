# Claude Code 설정 가이드

## 작성일
2026-03-07

## 최종 수정일
2026-03-07

## 요약
- Claude Code 설정은 4개 스코프(Managed > User > Project > Local)로 계층화되며, 배열 설정은 병합된다
- settings.json에서 permissions, hooks, env, sandbox 등 핵심 설정을 관리한다
- Skills(.claude/skills/)와 Sub-agents(.claude/agents/)는 별도 파일로 관리되며, settings.json과 독립적이다

## 상세 내용

### 1. 설정 파일 위치와 우선순위

#### 파일 위치

| 스코프 | 경로 | 용도 | 팀 공유 |
|--------|------|------|---------|
| Managed | Windows: `C:\Program Files\ClaudeCode\managed-settings.json` | 보안 정책, 컴플라이언스 | IT 배포 |
| User | `~/.claude/settings.json` | 개인 전역 설정 | 불가 |
| Project | `.claude/settings.json` | 프로젝트 팀 공유 설정 | git 커밋 |
| Local | `.claude/settings.local.json` | 프로젝트 개인 오버라이드 | 불가 (.gitignore) |

#### 우선순위 (높음 -> 낮음)

```
1. Managed (최고 — 오버라이드 불가)
2. Command line arguments
3. Local (.claude/settings.local.json)
4. Project (.claude/settings.json)
5. User (~/.claude/settings.json)
```

**배열 설정 병합**: `permissions.allow`, `permissions.deny` 등 배열 설정은 여러 스코프에서 **병합**된다 (교체가 아님).

#### 기타 설정 파일

| 파일 | 경로 | 용도 |
|------|------|------|
| User MCP | `~/.claude.json` | 사용자 전역 MCP 서버 |
| Project MCP | `.mcp.json` | 프로젝트 MCP 서버 |
| User Sub-agents | `~/.claude/agents/` | 사용자 전역 서브에이전트 |
| Project Sub-agents | `.claude/agents/` | 프로젝트 서브에이전트 |
| User Memory | `~/.claude/CLAUDE.md` | 사용자 전역 지침 |
| Project Memory | `CLAUDE.md` 또는 `.claude/CLAUDE.md` | 프로젝트 지침 |

---

### 2. Permissions (권한 관리)

#### 구조

```json
{
  "permissions": {
    "allow": ["규칙1", "규칙2"],
    "ask": ["규칙3"],
    "deny": ["규칙4"]
  }
}
```

#### 평가 순서

1. **deny** 규칙 먼저 확인
2. **ask** 규칙 확인
3. **allow** 규칙 확인
4. 첫 번째 매칭 규칙이 결정 (이후 규칙 무시)

#### 규칙 문법: `Tool(specifier)`

| 규칙 형식 | 설명 | 예시 |
|-----------|------|------|
| `Tool` | 해당 도구의 모든 호출 허용 | `Bash`, `Read`, `Edit` |
| `Tool(pattern)` | 패턴 매칭 | `Bash(npm run *)` |
| `Read(path)` | 특정 경로 읽기 | `Read(./.env)` |
| `Edit(path)` | 특정 경로 편집 | `Edit(./src/**)` |
| `WebFetch(domain:...)` | 특정 도메인 | `WebFetch(domain:*.github.com)` |
| `MCP(server_name)` | MCP 서버 도구 | `MCP(memory)` |
| `Agent(agent_name)` | 서브에이전트 | `Agent(code-reviewer)` |
| `Skill(name)` | 특정 스킬 | `Skill(deploy *)` |

#### 경로 패턴

| 패턴 | 의미 |
|------|------|
| `*` | 단일 단계 와일드카드 |
| `**` | 다중 단계 와일드카드 (재귀) |
| `?` | 단일 문자 |

#### 추가 권한 설정

| 키 | 설명 | 값 |
|----|------|-----|
| `additionalDirectories` | 추가 접근 허용 디렉토리 | `["../docs/", "~/shared"]` |
| `defaultMode` | 기본 권한 모드 | `"acceptEdits"`, `"askPerAction"`, `"bypassPermissions"` |
| `disableBypassPermissionsMode` | bypass 모드 비활성화 | `"disable"` |

---

### 3. Hooks (라이프사이클 훅)

#### 개요

Hooks는 Claude Code 라이프사이클의 특정 시점에 자동 실행되는 사용자 정의 핸들러이다. 셸 명령, HTTP 엔드포인트, LLM 프롬프트, 에이전트 4가지 타입을 지원한다.

#### 지원 이벤트

| 이벤트 | 실행 시점 | matcher 대상 |
|--------|----------|-------------|
| `SessionStart` | 세션 시작/재개 시 | 시작 방식 (`startup`, `resume`, `clear`, `compact`) |
| `UserPromptSubmit` | 프롬프트 제출 시 (처리 전) | matcher 미지원 |
| `PreToolUse` | 도구 실행 전 (차단 가능) | 도구 이름 |
| `PermissionRequest` | 권한 대화상자 표시 시 | 도구 이름 |
| `PostToolUse` | 도구 실행 성공 후 | 도구 이름 |
| `PostToolUseFailure` | 도구 실행 실패 후 | 도구 이름 |
| `Notification` | 알림 전송 시 | 알림 타입 |
| `SubagentStart` | 서브에이전트 생성 시 | 에이전트 타입 |
| `SubagentStop` | 서브에이전트 종료 시 | 에이전트 타입 |
| `Stop` | Claude 응답 완료 시 | matcher 미지원 |
| `TeammateIdle` | Agent Teams 팀원 idle 시 | matcher 미지원 |
| `TaskCompleted` | task 완료 처리 시 | matcher 미지원 |
| `InstructionsLoaded` | CLAUDE.md/rules 파일 로드 시 | matcher 미지원 |
| `ConfigChange` | 설정 파일 변경 시 | 설정 소스 |
| `WorktreeCreate` | worktree 생성 시 | matcher 미지원 |
| `WorktreeRemove` | worktree 제거 시 | matcher 미지원 |
| `PreCompact` | context compaction 전 | 트리거 (`manual`, `auto`) |
| `SessionEnd` | 세션 종료 시 | 종료 사유 |

#### 핸들러 타입

| 타입 | 설명 | 기본 timeout |
|------|------|-------------|
| `command` | 셸 명령 실행 | 600초 |
| `http` | HTTP POST 요청 | 30초 |
| `prompt` | Claude 모델에 단일 턴 평가 | 30초 |
| `agent` | 도구 사용 가능한 서브에이전트 생성 | 60초 |

#### 설정 예시

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": ".claude/hooks/block-rm.sh"
          }
        ]
      }
    ],
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "/path/to/lint-check.sh"
          }
        ]
      }
    ]
  }
}
```

#### Exit Code (command 타입)

| exit code | 의미 |
|-----------|------|
| 0 | 성공 — 실행 계속 |
| 1 | 에러 — 실행 중단 |
| 2 | 피드백 전달 — stdout 내용을 Claude에게 전달하여 계속 작업 |

#### MCP 도구 매칭

MCP 도구는 `mcp__<서버>__<도구>` 패턴으로 명명된다. regex 패턴으로 매칭 가능:
- `mcp__memory__.*` — memory 서버의 모든 도구
- `mcp__.*__write.*` — 모든 서버의 write 도구

---

### 4. Skills (스킬)

#### 개요

Skills는 Claude의 기능을 확장하는 지침 파일이다. `SKILL.md` 파일에 지침을 작성하면 Claude가 자동으로 사용하거나, 사용자가 `/skill-name`으로 직접 호출할 수 있다.

#### 파일 구조

```
.claude/skills/<skill-name>/
  SKILL.md           # 메인 지침 (필수)
  template.md        # 템플릿 (선택)
  examples/          # 예시 (선택)
  scripts/           # 스크립트 (선택)
```

#### SKILL.md Frontmatter

```yaml
---
name: my-skill
description: 스킬 설명
user-invocable: true
disable-model-invocation: false
allowed-tools: Read, Grep, Glob
model: opus
context: fork
agent: Explore
---
```

| 필드 | 필수 | 설명 | 기본값 |
|------|------|------|--------|
| `name` | 아니오 | 표시 이름, `/` 명령어 이름. 소문자+숫자+하이픈, 최대 64자 | 디렉토리명 |
| `description` | 권장 | 스킬 설명. Claude가 자동 적용 판단에 사용 | 본문 첫 문단 |
| `argument-hint` | 아니오 | 자동완성 힌트 | 없음 |
| `disable-model-invocation` | 아니오 | `true`면 Claude 자동 호출 차단 (수동 전용) | `false` |
| `user-invocable` | 아니오 | `false`면 `/` 메뉴에서 숨김 (배경 지식용) | `true` |
| `allowed-tools` | 아니오 | 스킬 활성 시 무승인 도구 목록 | 없음 |
| `model` | 아니오 | 스킬 활성 시 사용 모델 | 없음 |
| `context` | 아니오 | `fork`면 격리된 서브에이전트에서 실행 | 없음 |
| `agent` | 아니오 | `context: fork` 시 사용할 에이전트 타입 | `general-purpose` |
| `hooks` | 아니오 | 스킬 라이프사이클 훅 | 없음 |

#### 호출 제어

| Frontmatter 설정 | 사용자 호출 | Claude 호출 | context 로딩 |
|------------------|-----------|------------|-------------|
| (기본값) | 가능 | 가능 | description 항상 로드, 호출 시 전체 로드 |
| `disable-model-invocation: true` | 가능 | 불가 | description 미로드 |
| `user-invocable: false` | 불가 | 가능 | description 항상 로드 |

#### 저장 위치 우선순위

| 위치 | 경로 | 적용 범위 |
|------|------|----------|
| Enterprise | managed settings | 조직 전체 |
| Personal | `~/.claude/skills/<name>/SKILL.md` | 모든 프로젝트 |
| Project | `.claude/skills/<name>/SKILL.md` | 해당 프로젝트 |
| Plugin | `<plugin>/skills/<name>/SKILL.md` | 플러그인 활성 시 |

동일 이름 충돌 시: Enterprise > Personal > Project. Plugin은 네임스페이스로 분리.

#### 문자열 치환

| 변수 | 설명 |
|------|------|
| `$ARGUMENTS` | 호출 시 전달된 전체 인자 |
| `$ARGUMENTS[N]` / `$N` | N번째 인자 (0-based) |
| `${CLAUDE_SESSION_ID}` | 현재 세션 ID |
| `${CLAUDE_SKILL_DIR}` | SKILL.md가 위치한 디렉토리 |

#### 동적 컨텍스트 주입

`` !`command` `` 문법으로 셸 명령 결과를 스킬 콘텐츠에 삽입할 수 있다:

```yaml
---
name: pr-summary
context: fork
---
PR diff: !`gh pr diff`
Changed files: !`gh pr diff --name-only`
```

#### 번들 스킬 (기본 제공)

| 스킬 | 용도 |
|------|------|
| `/simplify` | 최근 변경 파일의 코드 품질/효율성 리뷰 후 수정 |
| `/batch <instruction>` | 대규모 변경을 병렬 처리 (git worktree 사용) |
| `/debug [description]` | 현재 세션 디버그 로그 분석 |
| `/claude-api` | Claude API 레퍼런스 로드 |

---

### 5. Sub-agents (서브에이전트)

`.claude/agents/` 디렉토리에 markdown 파일로 정의한다. Agent Teams와는 별개 기능이다.

```yaml
---
name: my-agent
description: 에이전트 설명
model: opus
---

에이전트 지침 내용...
```

상세 내용은 `docs/Claude_Code_Engineer/Knowledge/Agent_Teams_통신_구조.md`의 "Agent Teams vs Sub-agents 구분" 참조.

---

### 6. 환경 변수 (env)

settings.json의 `env` 필드로 환경 변수를 설정한다.

```json
{
  "env": {
    "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS": "1",
    "NODE_ENV": "development"
  }
}
```

#### 주요 환경 변수

| 변수 | 용도 |
|------|------|
| `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` | Agent Teams 활성화 |
| `CLAUDE_CODE_USE_BEDROCK` | AWS Bedrock 사용 |
| `CLAUDE_CODE_USE_VERTEX` | Google Vertex AI 사용 |
| `CLAUDE_CODE_SIMPLE` | 최소 시스템 프롬프트 |
| `CLAUDE_CODE_DISABLE_FAST_MODE` | 빠른 모드 비활성화 |
| `CLAUDE_CODE_SHELL` | 셸 오버라이드 |
| `CLAUDE_CODE_MAX_OUTPUT_TOKENS` | 최대 출력 토큰 (기본 32000) |
| `CLAUDE_CODE_DISABLE_1M_CONTEXT` | 1M 컨텍스트 비활성화 |
| `CLAUDE_CODE_DISABLE_AUTO_MEMORY` | 자동 메모리 비활성화 |
| `CLAUDE_AUTOCOMPACT_PCT_OVERRIDE` | auto-compaction 임계값 조절 |
| `DISABLE_AUTOUPDATER` | 자동 업데이트 비활성화 |
| `DISABLE_TELEMETRY` | 원격 측정 비활성화 |
| `SLASH_COMMAND_TOOL_CHAR_BUDGET` | 스킬 description 로딩 예산 |

---

### 7. 기타 주요 설정

#### 모델/성능

| 키 | 설명 | 예시 |
|----|------|------|
| `model` | 기본 모델 | `"claude-sonnet-4-6"` |
| `availableModels` | 사용 가능 모델 제한 | `["sonnet", "haiku"]` |
| `alwaysThinkingEnabled` | Extended Thinking 기본 활성화 | `true` |

#### UI/UX

| 키 | 설명 | 예시 |
|----|------|------|
| `language` | 응답 언어 | `"japanese"` |
| `outputStyle` | 출력 스타일 | `"Explanatory"` |
| `showTurnDuration` | 턴 소요 시간 표시 | `false` |

#### Agent Teams

| 키 | 설명 | 값 |
|----|------|-----|
| `teammateMode` | 팀원 표시 방식 | `"in-process"`, `"tmux"`, `"auto"` |

#### Git

| 키 | 설명 | 기본값 |
|----|------|--------|
| `includeGitInstructions` | Git 워크플로우 지침 포함 | `true` |

#### Sandbox

| 키 | 설명 |
|----|------|
| `sandbox.enabled` | 샌드박싱 활성화 |
| `sandbox.autoAllowBashIfSandboxed` | 샌드박싱 시 Bash 자동 승인 |
| `sandbox.filesystem.allowWrite` | 쓰기 허용 경로 |
| `sandbox.filesystem.denyWrite` | 쓰기 거부 경로 |
| `sandbox.filesystem.denyRead` | 읽기 거부 경로 |
| `sandbox.network.allowedDomains` | 네트워크 허용 도메인 |

---

### 8. CatCatGo 프로젝트 현재 설정

프로젝트 설정 파일: `.claude/settings.json`

```json
{
  "permissions": {
    "allow": [
      "Agent"
    ]
  },
  "env": {
    "CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS": "1"
  }
}
```

현재 최소 설정만 되어 있다. `Agent` 도구만 자동 허용하고, Agent Teams를 활성화한 상태이다.

## 출처
- Claude Code Settings: https://code.claude.com/docs/en/settings
- Claude Code Hooks Reference: https://code.claude.com/docs/en/hooks
- Claude Code Skills: https://code.claude.com/docs/en/skills
- Claude Code Sub-agents: https://code.claude.com/docs/en/sub-agents

## 관련 문서
- docs/Claude_Code_Engineer/Knowledge/Agent_Teams_통신_구조.md
- docs/Claude_Code_Engineer/Knowledge/Claude_토큰_사용_구조.md
