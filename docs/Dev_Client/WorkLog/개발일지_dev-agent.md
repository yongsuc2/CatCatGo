# 개발일지 - dev-agent

## 2026-03-07

### 리소스 참조 검증 Editor 도구 추가

**파일**: `Assets/_Project/Scripts/Editor/ResourceValidator.cs`

Unity Editor 메뉴 `Tools/Resource Validator`에서 실행 가능한 리소스 검증 도구 개발.

**검증 항목**:
- Character Resources: enemy.data.json 기반 Chars 폴더/애니메이션 프레임 존재 및 연속성
- Status Effect Icons: 9종 상태이펙트 아이콘 존재
- Equipment Icons: 7 슬롯 x 6 등급 장비 아이콘 존재
- Skill Icons: active-skill-tier.data.json 기반 스킬 아이콘 존재
- JSON Data Files: Data/Json 원본과 Resources 복사본 존재/동기화
- Unused Character Resources: 데이터에서 참조하지 않는 Chars 폴더 탐지

**발견 사항**:
- `spider` 캐릭터 폴더가 Chars에 존재하나 enemy.data.json에 참조 없음

### EnemyTable 죽은 코드 삭제 및 pools/baseStats 폴백 정리

**QA BUG-001/BUG-002 대응**

**삭제 항목**:
- `enemy.data.json`에서 `pools`, `baseStats` 섹션 삭제 (Resources 복사본 동기화)
- `EnemyTable.cs`에서 미사용 메서드 삭제: `GetBaseEnemyStats()`, `GetBaseBossStats()`, `GetChapterEnemyPool()`, `GetChapterBossPool()`, `GetRandomEliteId()`
- `EnemyTable.cs`에서 미사용 필드 삭제: `_enemyPool`, `_elitePool`, `_bossPool`, `_baseEnemyStats`, `_baseBossStats`

**변경 항목**:
- 적 풀을 `pools` JSON 하드코딩(Theme 1만) 대신 `templates` 기반 동적 생성으로 변경
  - `_allEnemyIds`: isBoss=false 이고 dungeon_ 접두사 아닌 전체 적
  - `_allBossIds`: isBoss=true 이고 dungeon_ 접두사 아닌 전체 보스
- `GetEnemyPoolForChapter()`, `GetElitePoolForChapter()`, `GetBossPoolForChapter()` 폴백을 전체 풀로 변경
- `GetBossAssignmentForChapter()` 폴백에서 전체 풀 사용
- `GetRandomEnemyId()`, `GetRandomBossId()`가 전 테마 적에서 선택하도록 변경
  - Tower, CatacombDungeon에서 호출 시 Theme 1에 국한되지 않음

**파일**:
- `Assets/_Project/Scripts/Domain/Data/EnemyTable.cs`
- `Assets/_Project/Data/Json/enemy.data.json`
- `Assets/Resources/_Project/Data/Json/enemy.data.json`

### Tower/CatacombDungeon 하드코딩 밸런스 수치 데이터 테이블 분리

**CLAUDE.md 규칙 "밸런스 수치 코드 하드코딩 금지" 대응**

**데이터 테이블 추가** (`dungeon.data.json`에 tower/catacomb 섹션):
- tower: maxFloor, stagesPerFloor, rewardStages, goldPerFloor(stage5/stage10), rewardPerStage(stage5/stage10 보상 목록)
- catacomb: battlesPerFloor, goldPerFloor, baseEquipmentStone

**코드 변경**:
- `DungeonDataTable.cs`: TowerConfig, CatacombConfig 클래스 추가, JSON 로딩, Tower/Catacomb 프로퍼티
- `Tower.cs`: MaxFloor, StagesPerFloor를 readonly 필드 → DungeonDataTable.Tower 참조 프로퍼티로 변경, GetReward()에서 데이터 테이블 기반 동적 보상 생성
- `CatacombDungeon.cs`: BattlesPerFloor를 readonly 필드 → DungeonDataTable.Catacomb 참조 프로퍼티로 변경, GetFloorReward()에서 데이터 테이블 수치 사용

**파일**:
- `Assets/_Project/Scripts/Domain/Data/DungeonDataTable.cs`
- `Assets/_Project/Scripts/Domain/Content/Tower.cs`
- `Assets/_Project/Scripts/Domain/Content/CatacombDungeon.cs`
- `Assets/_Project/Data/Json/dungeon.data.json`
- `Assets/Resources/_Project/Data/Json/dungeon.data.json`
