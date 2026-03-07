# Agent Context 관리 및 Memory 기능

## 작성일
2026-03-07

## 최종 수정일
2026-03-07

## 요약
- 각 subagent/teammate는 독립 context window를 가지며, 부모/리드의 대화 이력을 상속받지 않음
- context ~95% 도달 시 auto-compaction 발생 (이전 대화 요약/압축). CLAUDE.md는 compaction 후에도 디스크에서 재로드
- agent별 persistent context caching 기능은 없음. 세션 간 지식 유지 방법은 CLAUDE.md, agent .md 파일, auto memory (메인 세션만)
- `memory` frontmatter 필드: 공식 문서에 명시되어 있으나, 실제 테스트 결과 현재 버전에서 동작하지 않음 (2026-03-07 검증)
- agent 분리의 실질적 이점: context window 격리 + system prompt 특화 (memory 기능 없이도 유효)

## 상세 내용

### 1. Agent Context 관리 메커니즘 (확인된 사실)

#### 독립 Context Window

각 subagent와 agent teams의 teammate는 독립적인 context window에서 동작한다.

공식 문서 원문 (sub-agents):
> "Each subagent runs in its own context window with a custom system prompt, specific tool access, and independent permissions."

공식 문서 원문 (agent teams):
> "Each teammate has its own context window."

spawn 시 로드되는 것:
- CLAUDE.md (프로젝트 context)
- MCP servers, skills
- 자신의 agent .md 파일에 정의된 system prompt
- 기본 환경 정보 (working directory 등)

spawn 시 로드되지 않는 것:
- 부모/리드의 대화 이력
- 다른 agent의 context

#### Auto-Compaction

context window의 약 95%에 도달하면 자동으로 이전 대화를 요약/압축한다.

| 항목 | 설명 |
|------|------|
| 트리거 | context ~95% 도달 시 자동 (subagent, teammate 모두 동일) |
| 조절 방법 | 환경변수 `CLAUDE_AUTOCOMPACT_PCT_OVERRIDE`로 퍼센트 변경 (예: `50`) |
| CLAUDE.md | compaction 후에도 디스크에서 재로드되어 유지 |
| 대화 중 지시사항 | compaction 시 사라질 수 있음 -> CLAUDE.md에 기록해야 영구 유지 |

#### Agent별 Persistent Context Caching

Claude Code에는 agent별 전용 context caching 기능이 없다. 각 agent는 독립 context window를 가지며, context가 차면 auto-compaction으로 압축될 뿐이다.

세션 간 지식 유지 방법:
- CLAUDE.md: 프로젝트 규칙, 코딩 표준 등 (사용자가 작성)
- agent .md 파일: agent의 system prompt (사용자가 작성)
- auto memory: `~/.claude/projects/<project>/memory/MEMORY.md` (메인 세션에서 Claude가 자동 작성)
- subagent의 `memory` frontmatter: 아래 섹션 참조 (현재 미동작)

### 2. Agent `memory` Frontmatter 기능 검증 결과

#### 공식 문서 내용

출처: `https://code.claude.com/docs/en/sub-agents` - "Enable persistent memory" 섹션

frontmatter 테이블:
> | `memory` | No | Persistent memory scope: `user`, `project`, or `local`. Enables cross-session learning |

스코프별 저장 경로:

| Scope | Location | Use when |
|-------|----------|----------|
| `user` | `~/.claude/agent-memory/<name-of-agent>/` | the subagent should remember learnings across all projects |
| `project` | `.claude/agent-memory/<name-of-agent>/` | the subagent's knowledge is project-specific and shareable via version control |
| `local` | `.claude/agent-memory-local/<name-of-agent>/` | the subagent's knowledge is project-specific but should not be checked into version control |

문서상 동작 설명 (memory 활성화 시):
> When memory is enabled:
> - The subagent's system prompt includes instructions for reading and writing to the memory directory.
> - The subagent's system prompt also includes the first 200 lines of `MEMORY.md` in the memory directory, with instructions to curate `MEMORY.md` if it exceeds 200 lines.
> - Read, Write, and Edit tools are automatically enabled so the subagent can manage its memory files.

공식 권장 기본값: `user`

사용 예시 (공식 문서):
```yaml
---
name: code-reviewer
description: Reviews code for quality and best practices
memory: user
---

You are a code reviewer. As you review code, update your agent memory with
patterns, conventions, and recurring issues you discover.
```

#### 실제 검증 결과 (2026-03-07)

**동작하지 않음.** 다음 현상이 확인됨:

1. `memory: project` 설정 후 subagent spawn -> system prompt에 memory 관련 지시사항 주입되지 않음
2. `.claude/agent-memory/{agent-name}/` 디렉토리 생성되지 않음
3. agent가 전용 memory 경로를 인식하지 못하고, 공용 auto memory(`~/.claude/projects/.../memory/MEMORY.md`)에 잘못 기록

결론: 공식 문서에 명시되어 있으나 현재 버전에서 미구현으로 판단.

#### 문서에서 확인하지 못한 사항 (검증 이전 시점 기준)

- 디렉토리/MEMORY.md 최초 생성 주체 (시스템 자동 vs agent 수동): 문서에 명시 없음
- agent teams의 teammate에도 이 frontmatter가 동일하게 적용되는지: 문서에 명시 없음

### 3. Agent 분리의 실질적 이점

서버/클라이언트처럼 기술 스택이 다른 영역을 별도 agent로 분리하면, `memory` 기능 없이도 다음 이점이 유효하다.

#### Context Window 격리

- 서버 agent는 서버 코드만, 클라이언트 agent는 클라이언트 코드만 context에 로드
- 관련 없는 코드가 context를 소모하지 않아 auto-compaction 빈도 감소
- subagent 또는 agent teams의 teammate로 사용 시 독립 context window 보장

#### System Prompt 특화

- 서버 agent: C#/ASP.NET 패턴, DB 규칙, API 설계 원칙에 집중하는 system prompt
- 클라이언트 agent: Unity/게임 패턴, 리소스 관리, UI 규칙에 집중하는 system prompt
- 하나의 agent에 양쪽 규칙을 모두 담으면 system prompt가 길어져 context 낭비 + 준수율 저하

#### 도구 제한

- 서버 agent: Unity 관련 도구 불필요
- 클라이언트 agent: 서버 빌드/테스트 도구 불필요
- `tools` frontmatter로 각 agent에 필요한 도구만 허용 가능

#### 분리 시 단점

- 연동 작업(서버 API 변경 -> 클라이언트 연동 수정) 시 두 agent간 조율 필요
- agent 설정 파일, 문서, WorkLog 등 관리 대상 증가
- agent teams 사용 시 토큰 비용 증가 (각 agent가 독립 context 소비)

### 4. Subagent Frontmatter 전체 필드 목록 (공식 문서 기준)

| 필드 | 필수 | 설명 |
|------|------|------|
| `name` | Yes | agent 식별자 (소문자 + 하이픈) |
| `description` | Yes | 언제 이 agent를 사용할지 |
| `tools` | No | 사용 가능한 도구 목록. 생략 시 전체 상속 |
| `disallowedTools` | No | 거부할 도구 목록 |
| `model` | No | 사용할 모델: `sonnet`, `opus`, `haiku`, `inherit`. 기본값 `inherit` |
| `permissionMode` | No | 권한 모드: `default`, `acceptEdits`, `dontAsk`, `bypassPermissions`, `plan` |
| `maxTurns` | No | 최대 agentic turn 수 |
| `skills` | No | startup 시 context에 주입할 skill 목록 |
| `mcpServers` | No | MCP 서버 목록 |
| `hooks` | No | 라이프사이클 훅 |
| `memory` | No | 영구 메모리 스코프: `user`, `project`, `local` (현재 미동작) |
| `background` | No | `true`면 항상 백그라운드 실행. 기본값 `false` |
| `isolation` | No | `worktree` 설정 시 git worktree 격리 |

## 출처
- Claude Code Sub-agents: https://code.claude.com/docs/en/sub-agents
- Claude Code Memory: https://code.claude.com/docs/en/memory
- Claude Code Agent Teams: https://code.claude.com/docs/en/agent-teams

## 관련 문서
- docs/claude-code-engineer/Knowledge/Agent_Teams_통신_구조.md
- docs/claude-code-engineer/Knowledge/Claude_토큰_사용_구조.md
