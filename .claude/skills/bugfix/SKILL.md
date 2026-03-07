---
name: bugfix
description: 축적된 Open 버그를 검토하고 수정 작업을 진행. catcat-pd가 심각도 순으로 정리하여 적절한 Agent에게 할당.
user-invocable: true
disable-model-invocation: false
---

# bugfix - 버그 수정 작업

## 사용법

`/bugfix` — 축적된 Open 버그를 검토하고 수정 작업을 진행한다.
`/bugfix BUG-007` — 특정 버그만 지정하여 수정한다.

## 실행 절차

### 1. Open 버그 확인

`docs/qa-agent/Bugs/` 디렉토리에서 상태가 Open인 버그 문서를 모두 읽는다.
- 특정 버그가 지정되었으면 해당 버그만 대상으로 한다.
- Open 버그가 없으면 사용자에게 "처리할 Open 버그가 없습니다" 보고 후 종료.

### 2. /team 스킬 실행

`/team` 스킬을 실행하되, 작업 설명에 다음을 포함한다:

```
Open 버그 수정 작업.
대상 버그: {Open 버그 목록 (번호, 제목, 심각도)}
catcat-pd는 심각도 순(Critical → Major → Minor)으로 버그를 정리하고 적절한 Agent에게 수정을 할당하세요.
수정 완료 후 qa-agent에게 검증을 요청하고, 통과 시 Closed 처리하세요.
```
