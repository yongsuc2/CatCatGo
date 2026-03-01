# CLAUDE.md - CatCatGo Unity Project

## 원본 프로젝트 (copybarago)

| 항목 | 값 |
|------|-----|
| 리포 | https://github.com/yongsuc2/copybarago.git |
| 스택 | TypeScript 웹게임 |
| 로컬 경로 (원본) | `C:\copybarago\copybarago\` |
| 최신 커밋 | `e16dddc` Merge pull request #1 from yongsuc2/claude/refactor-anger-system-HH1vt |

---

## Project Overview
- Unity 6 (6000.3.10f1) 2D URP 프로젝트
- 기존 TypeScript 웹게임의 Unity 포팅
- 원본 프로젝트의 도메인 로직을 C#으로 1:1 포팅하는 것이 목표

---

## 절대 원칙

### 1. 요구사항 절대 준수
- 사용자가 제시한 요구사항 = **명시적 변경 요청 전까지 영구 유효**
- "더 나은 방법"이라는 이유로 임의 변경 **금지**

### 2. Fact 기반 작업
- 추측하지 말고 **파일을 읽어서 확인**
- 불확실하면 **사용자에게 질문**
- 답변 전 **팩트 확인 프로세스** 수행: 코드/로그/문서에서 근거를 확인한 후 답변
- 팩트 확인을 하지 못한 경우 **"미확인"임을 명시**
- 확인된 사실과 추론을 **구분하여 표기**
- 금지 표현: "아마도", "추정됩니다", "보통", "일반적으로"

### 3. 불가능 시 즉시 알림
- 기술적으로 불가능하면 **명확히 알림**
- 모순된 요구사항이면 **확인 요청**
- 애매한 상태로 진행 **금지**

### 4. 산출물은 깔끔하게
- 최종 코드 = **완성된 코드만**
- 실수 과정, 변명, 수정 이력 **포함 금지**

### 5. 자율 실행
- 명확한 목표가 주어지면 **중간에 사용자에게 묻지 말고 끝까지 실행**
- 구현 방법이 여러 개여도, 가장 합리적인 방법을 **스스로 선택하고 진행**
- 에러 발생 시 **스스로 원인 분석 → 수정 → 재시도** (최소 3회)
- 정보가 부족하면 **코드/문서/로그를 직접 탐색**해서 파악
- **금지**: "이렇게 진행해도 될까요?" 식의 확인 질문
- **금지**: 구현 방식 선택지를 나열하며 사용자에게 고르라는 행위
- **금지**: 이미 충분한 정보가 있는데 추가 확인을 요청하는 행위
- **사용자에게 물어야 하는 경우** (이 경우에만 질문):
  - 요구사항 자체가 모호하거나 상충할 때
  - 비즈니스 판단이 필요한 결정 (기능 범위, 우선순위)
  - 여러 번 시도해도 해결 불가능할 때 (시도한 내용과 함께 보고)
  - 되돌릴 수 없는 파괴적 작업 (데이터 삭제, force push 등)

---

## 코드 작성 규칙

### 주석 금지
- 주석 **절대 작성 금지**
- 함수명/변수명으로 의도를 명확히 표현

### Over-engineering 금지
- 요청한 것만 구현
- 불필요한 기능 추가 금지

### 중복 코드 금지
- 동일/유사 로직을 여러 곳에 작성 **금지**
- 2곳 이상에서 사용되는 로직은 **공통 함수로 추출**
- 새 코드 작성 시 기존에 동일 기능이 있는지 **먼저 확인**

### 기능 변경 시 연관 코드 동시 수정
- 기능 변경 시 해당 기능을 참조/의존하는 **모든 코드를 함께 수정**

### 밸런스 수치 데이터 테이블화 (필수)
- 게임 밸런스에 영향을 주는 **모든 수치는 데이터 테이블에서 관리**
- **코드 + 기획 문서** 모두 밸런스 수치 하드코딩 **금지**
- 기획 문서(`docs/`)에는 **개념만 명시**하고 구체적 수치는 데이터 테이블 참조만 걸 것
- UI에 표시되는 설명 문자열도 데이터 테이블 값을 참조하여 **동적 생성**
- 데이터 테이블 역할 분리:
  - `BattleDataTable` — 전투 메카닉 계수 (데미지 공식, 분노, 반격, 적 스케일링 등)
  - `EncounterDataTable` — 인카운터 관련 데이터 (가중치, 카운터 임계값 등)
  - `EnemyTable` — 적 스탯/스킬 데이터
- 새로운 수치가 필요할 때 기존 테이블에 추가하거나, 적절한 새 테이블 생성

### 죽은 코드 즉시 삭제 (필수)
- 리팩토링/재설계 후 **쓸모없어진 코드·데이터 파일은 즉시 삭제**
- 수정 전 해당 파일이 **실제로 import/참조되는지** 확인 — 죽은 코드를 수정하면 아무 효과 없이 혼란만 유발

---

## Architecture

### Assembly Structure (컴파일 의존성)
```
CatCatGo.Infrastructure  ← 의존성 없음 (SeededRandom, EventBus, JsonDataLoader)
CatCatGo.Domain          ← Infrastructure 참조 (Entities, Enums, ValueObjects, Battle, Data)
CatCatGo.Services        ← Domain + Infrastructure 참조 (GameManager, BattleManager 등)
CatCatGo.Presentation    ← Domain + Services + Infrastructure 참조 (UI, Screens)
```

### Folder Structure
```
Assets/_Project/
  Scripts/
    Domain/         - 게임 핵심 로직 (엔티티, 배틀, 챕터 등)
    Infrastructure/ - 유틸리티 (PRNG, EventBus, JsonLoader)
    Services/       - 매니저 계층 (GameManager, BattleManager 등)
    Presentation/   - Unity UI (Screens, Components)
  Data/Json/        - 밸런스 데이터 원본 (편집용)
  Resources/        - Resources.Load용 복사본
  Prefabs/          - UI/캐릭터/이펙트 프리팹
  Art/              - 스프라이트, 애니메이션, UI 아트
  Audio/            - BGM, SFX
  Scenes/           - 게임 씬
```

### Data Flow
- 밸런스 수치는 `Data/Json/*.data.json`에서 관리
- Resources 폴더에 복사하여 런타임 로드 (JsonDataLoader)
- 코드에 밸런스 수치 하드코딩 금지

---

## Key Porting Notes
- TypeScript `Result<T>` → C# `Result<T>` (동일 패턴)
- TypeScript `Stats` (immutable class) → C# `Stats` (struct)
- TypeScript enum (string) → C# enum (int, 순서 동일하게 유지)
- JSON 데이터는 원본 그대로 사용, C#에서 역직렬화
- GameManager는 MonoBehaviour 싱글톤으로 구현
- SeededRandom의 PRNG 알고리즘은 세이브 호환을 위해 완전히 동일하게 유지
- C# namespace: `CatCatGo.Domain`, `CatCatGo.Services`, `CatCatGo.Infrastructure`, `CatCatGo.Presentation`

---

## 작업 프로세스

```
1. 요구사항 확인
2. 정보 수집
3. 작업 수행
4. 검토
5. 검증 - 예상 결과를 구체적으로 정의하고, 실제 결과와 정확히 일치하는지 비교
```

### 작업 선호도
- 시간이 오래 걸리더라도 **높은 수준의 결과물** 선호
- 미래의 **유지보수 비용을 줄이는 것**이 중요
- 문서(코드)를 기반으로 한 **정확한 내용** 공유
- 작업 후 **검증 수행** 또는 **검증 방법 제시**

---

## 문서 동기화 (필수)

코드 작업 완료 시, 아래 문서의 동기화 여부를 **반드시** 확인하고 업데이트할 것:

| 문서 | 경로 | 업데이트 시점 |
|------|------|-------------|
| 기획 문서 | `docs/00~12_*.md` | 해당 시스템의 동작 방식/흐름이 변경되었을 때 |
| UI 기획서 | `docs/화면기획문서/*.md` | 화면 구성, 컴포넌트, 상호작용이 변경되었을 때. **구체적 수치(px, 색상코드 등) 금지 — 기획 의도·구도·흐름만 기술** |
| 작업 목록 | `docs/91_작업목록_{담당자}.md` | 작업 완료/추가 시 **본인 파일만** 업데이트 |
| 개발일지 | `docs/99_개발일지_{담당자}.md` | 매 세션 종료 시 **본인 파일만** 업데이트. **버그 수정은 기록하지 않음** |

### 담당자별 문서

- **yongsuc2**: `91_작업목록_yongsuc2.md` / `99_개발일지_yongsuc2.md`
- **kkwndud**: `91_작업목록_kkwndud.md` / `99_개발일지_kkwndud.md`
- 기존 `91_작업목록.md`, `99_개발일지.md`는 마스터 아카이브로 유지

**빠뜨리지 않도록 커밋 전에 문서 동기화 체크리스트를 확인할 것.**

---

## 문서 자동 업데이트

- 작업에 필요한 정보가 문서에 없어서 탐색/질문으로 알게 된 경우
- 문서 내용이 실제와 달라서 수정이 필요한 경우
- 반복될 수 있는 작업 패턴이나 명령어를 발견한 경우
- 새로운 용어, 규칙, 제약사항을 알게 된 경우

업데이트 시 사용자에게 알리고, 간결하게 핵심만 추가할 것.
