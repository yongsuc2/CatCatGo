# Agent Teams 통신 구조

## 작성일
2026-03-06

## 최종 수정일
2026-03-06

## 요약
- 팀원끼리 1:1 직접 메시지 가능 (팀 리드를 거치지 않음)
- 각 팀원은 독립된 context window를 가지며, 정보 공유는 메시지 또는 파일 시스템을 통해 수행
- 공유 Task List로 작업을 조율하며, 팀원이 자율적으로 task를 claim하고 협업 가능

## 상세 내용

### 1. 메시지 통신 방식

팀원끼리 팀 리드를 거치지 않고 직접 1:1 메시지를 보낸다. Mailbox 시스템을 통해 자동 전달되며, 수신자가 polling할 필요 없이 inbox에 도착한다.

| 메시지 타입 | 동작 |
|------------|------|
| message | 특정 팀원 1명에게 직접 전송 |
| broadcast | 전체 팀원에게 동시 전송. 비용이 팀원 수만큼 배수로 발생하므로 자제 권장 |

메시지 구조 (현재 세션의 SendMessage 도구 기준):

```json
{
  "type": "message",
  "recipient": "teammate-name",
  "content": "전달할 내용 (텍스트)",
  "summary": "5-10 단어 요약"
}
```

파일, 이미지 등 바이너리 첨부 기능은 없다. 파일 공유는 경로를 알려주거나 내용을 텍스트로 복사하는 방식.

### 2. Context 공유 -- 완전히 독립

각 팀원은 자기만의 독립된 context window를 가진다.

- 팀 리드의 대화 이력을 상속받지 않음
- 팀원 A가 파일을 읽은 결과가 팀원 B의 context에 자동으로 들어가지 않음
- 정보를 공유하려면 메시지로 직접 전달해야 함

팀원 생성 시 로드되는 것:
- CLAUDE.md (프로젝트 context)
- MCP servers, skills
- 팀 리드가 보낸 spawn prompt

### 3. 공유되는 자원

| 자원 | 저장 위치 | 설명 |
|------|----------|------|
| Task List | `~/.claude/tasks/{team-name}/` | 전체 팀 공유. pending/in_progress/completed 상태. 의존성(dependency) 지원 |
| Team Config | `~/.claude/teams/{team-name}/config.json` | members 배열 (name, agent ID, agent type). 팀원이 이 파일을 읽어 다른 팀원 발견 |
| 파일 시스템 | 작업 디렉토리 (같은 repo) | 한 팀원이 수정한 파일을 다른 팀원이 읽을 수 있음. 같은 파일 동시 수정은 충돌 위험 |

Task claiming은 file locking으로 race condition을 방지한다. 의존성이 있는 task는 선행 task 완료 시 자동으로 unblock된다.

### 4. 자율 협업

팀 리드가 task를 만들고 할당하면, 팀원들이 자율적으로:
- task를 claim
- 서로 메시지를 주고받으며 토론/협업
- 완료 시 다음 unassigned task를 자동으로 pick up

공식 문서 예시:
> "Spawn 5 agent teammates to investigate different hypotheses. Have them talk to each other to try to disprove each other's theories, like a scientific debate."

### 5. 사용자도 팀원에게 직접 소통 가능

- In-process 모드: `Shift+Down`으로 팀원 간 전환, 직접 메시지 입력
- Split-pane 모드: 팀원 pane을 클릭하여 해당 세션에 직접 입력

### 6. 주의사항

- 같은 파일을 두 팀원이 동시 수정하면 덮어쓰기 발생. 팀원별 담당 파일을 분리할 것
- broadcast는 팀원 수 x 메시지 비용이 발생하므로 꼭 필요할 때만 사용
- 팀원은 자기 팀을 만들거나 다른 팀원을 spawn할 수 없음 (nested teams 불가)

## 출처
- Claude Code Agent Teams: https://code.claude.com/docs/en/agent-teams
- 현재 세션의 SendMessage 도구 스키마 (메시지 포맷 참조)

## 관련 문서
- docs/Claude_Code_Engineer/Knowledge/Claude_토큰_사용_구조.md
