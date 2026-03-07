# Claude 토큰 사용 구조

## 작성일
2026-03-06

## 최종 수정일
2026-03-06

## 요약
- Claude API는 stateless로 매 요청마다 전체 대화를 전송하지만, Prompt Caching으로 실제 비용은 새로 추가된 부분 위주로 발생 (90% 절감)
- Compaction이 context window 한계 접근 시 이전 대화를 요약하여 무한 누적을 방지
- Agent Teams idle 팀원도 토큰을 소모한다고 공식 문서에 명시되어 있으나, 구체적인 금액/주기는 미공개

## 상세 내용

### 1. Claude API -- Stateless 구조

Claude API는 stateless이다. 서버가 대화 상태를 보관하지 않으며, 매 요청마다 클라이언트가 `messages` 배열에 전체 대화 내역을 담아 보낸다.

```
요청 1: [user: "안녕"]                          → input: 2 토큰
요청 2: [user: "안녕", assistant: "반갑습니다", user: "이름이 뭐야?"]  → input: 10 토큰
요청 3: [이전 전체 대화 + 새 메시지]              → input: 계속 증가
```

대화가 길어질수록 매 요청의 input 토큰이 선형적으로 증가한다.

### 2. Prompt Caching -- 비용 절감

Prompt Caching이 비용 문제를 크게 완화한다.

| 토큰 유형 | 비용 (Opus 4.6 기준) | 설명 |
|-----------|---------------------|------|
| Base Input | $5/MTok | 캐시 없이 처리되는 토큰 |
| Cache Write | $6.25/MTok (5분) / $10/MTok (1시간) | 최초 캐싱 시 비용 |
| Cache Read | $0.50/MTok | 캐시 히트 시 -- 90% 절감 |

동작 원리:
- API 요청의 prefix(앞부분)가 이전 요청과 동일하면 캐시에서 읽음
- 대화에서는 이전 턴이 항상 prefix로 유지되므로, 새로 추가된 메시지만 실제 처리 비용 발생
- 시스템 프롬프트, 도구 정의 등 매번 동일한 부분도 캐시 재사용
- 캐시 유효 시간: 기본 5분, 설정 시 1시간

```
요청 1: [system + tools + user1]           → 전체 처리 + 캐시 저장
요청 2: [system + tools + user1 + asst1 + user2]
         ^^^^^^^^^^^^^^^^^^^^^^^^ 캐시 히트 (90% 절감)
                                  ^^^^^^^^^^^^^^^^^ 새로 처리
```

매 요청마다 전체 대화를 보내긴 하지만, 캐싱 덕분에 실제 비용은 새로 추가된 부분 위주로 발생한다.

### 3. Compaction -- Context Window 관리

대화가 context window 한계(200K 또는 1M 토큰)에 가까워지면 Compaction이 동작한다.

| 계층 | Compaction 방식 |
|------|----------------|
| Claude API (server-side) | beta 기능. `context_management.edits`로 활성화. threshold 도달 시 자동 요약 |
| Claude Code (client-side) | auto-compaction. context의 약 95% 도달 시 자동 요약 (`CLAUDE_AUTOCOMPACT_PCT_OVERRIDE`로 조절 가능) |

Compaction이 발생하면:
1. 이전 대화를 요약(summarize)
2. 요약본으로 대체하여 context 크기 축소
3. 이후 요청은 요약본 + 최근 대화만 전송

따라서 장시간 대화해도 compaction이 여러 번 발생하여 실제 전송되는 context는 훨씬 작다.

### 4. Agent Teams 토큰 소모

| 상태 | 토큰 소모 |
|------|----------|
| 팀원 생성 시 | spawn prompt + 프로젝트 context(CLAUDE.md, 환경 정보) 로드 → 초기 비용 발생 |
| 작업 중 | 일반 세션과 동일 (prompt caching, compaction 적용) |
| idle 상태 | 공식 문서에 "토큰을 소모한다"고 명시. 구체적 금액/주기는 미공개 |
| shutdown 후 | 소모 없음 |

공식 문서 원문 (Agent Teams 비용 섹션):
> "Clean up teams when work is done. Active teammates continue consuming tokens even if idle."

공식 문서 원문 (Background token usage 섹션 -- Claude Code 세션 일반):
> "Conversation summarization: Background jobs that summarize previous conversations for the `claude --resume` feature"
> "These background processes consume a small amount of tokens (typically under $0.04 per session) even without active interaction."

**주의**: 위 두 섹션은 별개이다. $0.04/session은 Claude Code 세션 일반의 background 비용이며, Agent Teams idle 팀원의 비용과 직접 연결된 수치가 아니다. Agent Teams idle 팀원의 구체적 소모량은 공식 문서에 명시되어 있지 않다.

결론: 불필요한 팀원은 shutdown 시키는 것이 권장된다.

### 비용 관리 흐름 요약

```
매 요청: 전체 messages 배열 전송 (stateless)
    ↓
Prompt Caching: prefix 동일 부분은 캐시 히트 → 90% 절감
    ↓
Compaction: context window 한계 접근 시 → 이전 대화 요약으로 대체
    ↓
결과: 실제 비용은 "새로 추가된 내용" 위주로 발생
```

## 출처
- Claude API Context Windows: https://platform.claude.com/docs/en/docs/build-with-claude/context-windows
- Claude API Compaction: https://platform.claude.com/docs/en/build-with-claude/compaction
- Claude API Prompt Caching: https://platform.claude.com/docs/en/build-with-claude/prompt-caching
- Claude Code 비용 관리: https://code.claude.com/docs/en/costs
- Claude Code Agent Teams: https://code.claude.com/docs/en/agent-teams

## 관련 문서
- 없음
