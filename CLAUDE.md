# CLAUDE.md - CatCatGo Unity Project

## Project Overview
- Unity 6 (6000.3.10f1) 2D URP 프로젝트
- 기존 TypeScript 웹게임(C:\copybarago\copybarago)의 Unity 포팅
- 원본 프로젝트의 도메인 로직을 C#으로 1:1 포팅하는 것이 목표

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

## Coding Rules
- 주석 금지 (함수명/변수명으로 의도 표현)
- Over-engineering 금지
- 중복 코드 금지
- C# namespace: `CatCatGo.Domain`, `CatCatGo.Services`, `CatCatGo.Infrastructure`, `CatCatGo.Presentation`

## Original TypeScript Source Reference
- 원본 코드: `C:\copybarago\copybarago\src\`
- 포팅 시 원본 TypeScript 파일을 반드시 읽고 1:1 대응되도록 변환
- SeededRandom의 PRNG 알고리즘은 세이브 호환을 위해 완전히 동일하게 유지

## Key Porting Notes
- TypeScript `Result<T>` → C# `Result<T>` (동일 패턴)
- TypeScript `Stats` (immutable class) → C# `Stats` (struct)
- TypeScript enum (string) → C# enum (int, 순서 동일하게 유지)
- JSON 데이터는 원본 그대로 사용, C#에서 역직렬화
- GameManager는 MonoBehaviour 싱글톤으로 구현
