# Agent Teams model "inherit" 버그

## 상태
Open (Claude Code 버그 — 수정 대기)

## 발견일
2026-03-07

## 증상

Agent Teams에서 teammate를 spawn할 때, `~/.claude/teams/{team_name}/config.json`의 member `model` 필드가 `"inherit"`로 설정되면 **team agent 간 통신(SendMessage)이 동작하지 않는다.**

## 정상 동작

- config.json의 `model` 필드는 `.claude/agents/{에이전트이름}.md`에 설정된 `model` 값을 따라간다
- 예: agent 정의에 `model: opus`가 명시되어 있으면 config.json에도 `"model": "opus"`로 기록됨
- 이 경우 team agent 간 통신이 정상 동작

## 버그 발생 조건

- config.json의 `model` 필드가 `"inherit"`로 기록되는 경우 발생
- `"inherit"`가 기록되면 agent 간 SendMessage 통신이 불가능해짐

## 워크어라운드

`/team` skill에서 Agent spawn 직후 config.json을 읽어 `model` 필드를 검증하는 단계를 추가하여 대응 중:

```
Agent spawn 후 ~/.claude/teams/{team_name}/config.json을 Read 도구로 읽어서
각 member의 model 필드를 확인한다.

- "inherit"가 발견되면 → 팀 구성 실패로 판단하고 즉시 중단
- 정상(예: "opus")이면 → 작업 진행
```

## 비고

- 공식 문서에서는 `"inherit"`를 부모 세션 모델 상속으로 설명하지만, 실제로는 통신 장애를 유발
- Claude Code 업데이트로 버그가 수정되면 검증 단계 제거 가능
