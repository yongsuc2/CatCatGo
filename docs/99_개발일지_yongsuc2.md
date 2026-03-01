# 카피바라고! 개발일지 — yongsuc2

---

## 2026-02-12 (Day 1)

### 완료 작업
- 역기획서 12개 문서 작성 (00~11)
  - 게임개요, 전투, 캐릭터성장, 장비, 스킬, 스테이지/던전, 가챠, 재화, PvP, 펫, 이벤트, 과금
- OOP 설계 문서 작성 (90_OOP설계문서.md)
  - 4레이어 아키텍처, 도메인 모델, 서비스 레이어, 이벤트 버스, 데이터 테이블
- 프로그래밍 작업 목록 작성 (91_작업목록.md)
  - 8개 Phase, 총 125개 작업 정의

### 결정 사항
- 1차 구현: 웹 브라우저 프로토타입
- 2차 구현: Unity 이식
- OOP 설계를 기술 스택 독립적으로 작성하여 이식 용이하게 함
- AI Agent(Claude Code)가 문서를 보고 구현하는 방식으로 개발 진행

---

## 2026-02-12 (Day 1 - 오후)

### 완료 작업
- **Phase 0: 프로젝트 셋업** (작업 0-1 ~ 0-5)
- **Phase 1A: 열거형 + 값 객체** (작업 1A-1 ~ 1A-4)
- **Phase 1B: 핵심 엔티티** (작업 1B-1 ~ 1B-10)
- **Phase 2A: 전투 시스템** (작업 2A-1 ~ 2A-11)
- **Phase 2B: 챕터 시스템** (작업 2B-1 ~ 2B-12)
- **Phase 2C: 적 데이터** (작업 2C-1 ~ 2C-3)
- **Phase 3: 성장 시스템 서비스** (Forge, EquipmentManager, PetManager)
- **Phase 4: 콘텐츠 시스템** (Tower, CatacombDungeon, DailyDungeon, Arena, Travel, GoblinMiner)
- **Phase 5: 경제 시스템** (TreasureChest, Collection, DailyResetSystem)
- **Phase 6: 메타 시스템** (GameEvent, SaveManager, OfflineRewardCalculator, DailyRoutineScheduler, ResourceAllocator)
- **Phase 7: UI/프레젠테이션** (GameManager, GameContext, 모든 화면)
- **Phase 8: 빌드 검증** (tsc + vitest + vite build)

---

## 2026-02-13 (Day 2)

### 완료 작업
- **UI 아이콘 추가** (lucide-react)
- **전체 한글화**
- **전투 비주얼 UI 구현** (CharacterSprite, BattleArena, PlayerStatsBar, 공격 애니메이션)
- **모험 화면 기획서 작성**
- **7D-1: 인벤토리 시스템** (장비 보관함 탭)
- **7D-4: 퀘스트 화면** (일일/주간 퀘스트)

---

## 2026-02-14 (Day 3)

### 완료 작업
- **모험 시스템 기획서 분리** (docs/12_모험시스템.md)
- **챕터 보물상자 시스템 구현** (Phase 9)
- **챕터 결과 오버레이 추가**
- **작업 목록/개발일지 2인 분할 관리 구조 도입**
- **인카운터 UI 한글화** (선택지 라벨/설명 한글 변환, ENCOUNTER_TYPE_LABEL 매핑)
- **스킬 아이콘 추가** (24종 스킬에 이모지 아이콘 부여, Skill.icon 필드)
- **퀘스트 테스트 코드 확장** (21개 테스트: 미션 ID 일관성, 통합 테스트, 엣지 케이스)
- **AdventureStage 상시 스테이지 구현**
  - AdventureStage.tsx 신규: 전투/인카운터/대기 모드 통합
  - 인카운터 이모지 비주얼 매핑 (7종 + CHANCE 세분화)
  - CSS: adventure-stage, stage-encounter-visual, @keyframes shake 추가
  - ChapterScreen.tsx JSX 구조 재배치 (4분기 → 3분기 통합)
  - 모험화면_기획서.md 업데이트

---

## 2026-02-15 (Day 4)

### 완료 작업
- **데이터 테이블 분리 리팩토링**
  - 하드코딩된 게임 데이터를 중앙화된 데이터 테이블로 분리
  - EncounterDataTable: 인카운터 타입 라벨/설명, 출현 가중치, 수치/옵션 템플릿
  - EquipmentDataTable: 장비 등급/슬롯 라벨, 판매가
  - ResourceDataTable: 재화 라벨(전체/약어), 색상, 던전 라벨
  - Skill 엔티티 description 필드 추가 + SkillTable 24종 스킬 설명 부여
  - 소비자 파일 6개 업데이트 (EncounterGenerator, ChapterScreen, EquipmentScreen, DebugPanel, ChapterTreasureScreen, QuestScreen, ContentScreen)

- **다단 히트 → 확률 기반 추가 타격으로 변경**
  - 기존: Multi-Hit Mastery가 고정 3회 타격
  - 변경: 공격 시 50% 확률로 1회 추가 타격 (총 2회)
  - BattleUnit: multiHitCount → multiHitChance (확률값)
  - Battle.processAttack(): 고정 루프 → 기본 1회 + 확률 추가 1회
  - animateTurn(): 히트별 개별 접근→타격→후퇴 연출 (groupByHit)
  - ON_ATTACK 스킬은 추가 타격에서도 동일 발동

- **쪽박/중박/대박 카운터 기반 룰렛 시스템 구현**
  - 원작 카피바라고의 카운터 기반 룰렛 시스템 반영
  - ANGEL=중박(12회→중박 룰렛), DEMON=대박(7회→대박 룰렛), CHANCE=쪽박
  - ROULETTE/LUCKY_MACHINE/MERCHANT 인카운터 타입 제거
  - JUNGBAK_ROULETTE/DAEBAK_ROULETTE 신규 타입 추가
  - Chapter에 jungbakCount/daebakCount 카운터 추가
  - advanceDay()에서 카운터 임계값 체크 후 룰렛 자동 발동
  - resolveEncounter()에서 ANGEL/DEMON 해결 시 카운터 증가
  - 인카운터 가중치 재배분: COMBAT(40%), ANGEL(25%), DEMON(15%), CHANCE(20%)
  - ChapterScreen에 중박/대박 카운터 진행도 표시 UI 추가

- **전투 딜 그래프 구현**
  - 전투 중 토글 버튼(📊)으로 딜 그래프 표시/숨김
  - 데미지 소스별 누적 집계: 일반 공격, 스킬별(광창/수리검 등), 반격, 독 피해
  - 내림차순 정렬, 막대그래프 + 수치 + 비율(%) 표시
  - DamageGraph 컴포넌트 신규 생성

- **흡혈 스킬 정의 명시**
  - 일반 공격 피해에만 적용 (15%), 스킬 데미지 미적용
  - 기획서(01_전투시스템.md, 04_스킬시스템.md) 업데이트

- **분노(Rage) 게이지 시스템 구현**
  - 원작 카피바라고의 분노 시스템 반영
  - BattleUnit: rage/maxRage/ragePerAttack/bonusRagePerAttack/rageDamageMultiplier 필드 추가
  - Battle: processAttack 끝에 분노 충전, processRageAttack(ATK×2.0×multiplier), processOnRageSkills
  - EffectType: RAGE_POWER/RAGE_BOOST, TriggerCondition: ON_RAGE, BattleLogType: RAGE_ATTACK 추가
  - rage_mastery 변경: ATK_UP 버프 → RAGE_POWER (분노 공격 데미지 +100%)
  - complete_rage_mastery 추가: RAGE_BOOST (게이지 2배속 충전)
  - 분노 공격 스킬 3종 추가: rage_lightning(⚡30), rage_lance(🔱35), rage_flame_wave(🔥25)
  - BattleArena: 플레이어 HP 바 아래 분노 게이지 바 (주황→빨강 그라데이션)
  - ChapterScreen: RAGE_ATTACK 딜 그래프/애니메이션 처리
  - SYNERGY_MAP 업데이트 (SKULL/KNIGHT/GHOST에 분노 스킬 추가)

- **엘리트/중간보스 강제 전투 구간 구현**
  - 원작 카피바라고의 특정 일차 강제 전투 구간 반영
  - 20일차: 엘리트 전투 (CHAPTER_ELITE_POOL), 30일차: 중간보스 전투 (CHAPTER_BOSS_POOL)
  - 승리 시 금상자: 신화 스킬 3개 중 1개 선택
  - Chapter: isEliteDay()/isMidBossDay() 판정, createEliteBattle/createMidBossBattle 생성
  - EncounterDataTable: forcedBattleDays = { elite: 20, midBoss: 30 }
  - BattleArena: battleLabel prop (엘리트/보스/최종 보스 배지)
  - ChapterScreen: battleType 상태, startEliteBattle/startMidBossBattle, 금상자 UI
  - 패배 시 챕터 실패 (기존 패배 로직 동일)

- **몬스터 분노/스킬/2마리 전투 시스템 구현**
  - 몬스터 분노: ragePerAttack 필드를 EnemyTemplate에 추가, Alpha Wolf(20), Demon Lord(15)에 적용
  - 몬스터 스킬 확장: Orc(연타), Dark Knight(반격+방어막), Stone Golem(반격+방어막), Alpha Wolf(연타+반격)
  - 방어막(Shield) 시스템: EffectType.SHIELD 추가, BattleUnit.takeDamage()에 쉴드 우선 흡수, iron_shield 스킬 추가
  - 복수 적 전투(1vs2): Battle 클래스 enemies 배열화, 50% 확률로 2마리 등장 (각 0.7배 스탯, 서로 다른 종류)
  - 순차 공격 타겟팅: 앞 적부터 처치, 죽으면 다음 적 자동 타겟
  - BattleArena: 복수 적 렌더링, 방어막 바, 적 분노 게이지, 사망 적 반투명 처리
  - ChapterScreen: 복수 적 HP 추적 (RunningHps), 적별 애니메이션 (activeEnemyIndex)

---

## 2026-02-16 (Day 5 - 오전)

### 완료 작업
- **스킬 티어/업그레이드 시스템 구현**
  - 모든 스킬 패밀리에 4단계 티어 도입 (tier 1=일반 → tier 2=전설 → tier 3=신화 → tier 4=불멸)
  - Skill 엔티티: `grade` 파라미터를 `tier: number`로 교체, `grade`는 getter로 파생
  - SkillDataTable: 26개 패밀리 × 4티어 밸런스 수치 정의, getSkillDescription(id, tier) 시그니처 변경
  - SkillTable: 패밀리 정의 배열 + tier 루프로 104개 Skill 인스턴스 생성, getNextTierSkill/getTier1Skills/isSpecialSkill 메서드 추가
  - EncounterGenerator: 스킬 풀을 "새 스킬(tier 1) + 업그레이드(현재 tier+1)" 혼합으로 재구성
  - 천사 인카운터: tier 1~2만 제공, 악마/중박 룰렛: 전체, 대박 룰렛/금상자: tier 3(신화급)
  - Chapter.resolveEncounter: 동일 패밀리 보유 시 교체(업그레이드), 미보유 시 추가
  - ChapterScreen 금상자: tier 3 풀 생성 시 보유 스킬 tier 확인, 교체/추가 분기
  - angel_power/demon_power: 예외 — 단일 티어 4(불멸), 대박 룰렛에서만 획득
  - 스킬 이름 티어 접미사: tier 2+=`II`, `III`, `IV`

---

## 2026-02-16 (Day 5 - 오후)

### 완료 작업
- **스킬 시스템 전면 재설계 구현** (Y-9)
  - 기존 단일 Skill/SkillTable/SkillDataTable → 완전히 새로운 아키텍처
  - **계층 구조**: BUILTIN/UPPER/LOWER/LOWEST 4단계, 역방향 연쇄 금지
  - **CompoundTrigger**: 3개 AND 조건 (When + Probability + Special)
  - **공격 타입**: PHYSICAL/MAGIC/FIXED 분리, 각각 다른 DEF 감소율
  - **스킬 효과**: ATTACK/TRIGGER_SKILL/INJECT_EFFECT/HEAL_HP/ADD_RAGE/CONSUME_RAGE/DEBUFF 7종
  - **INJECT_EFFECT**: 장착 시점에 타겟 스킬의 resolvedEffects에 효과 주입
  - **패시브 분리**: PassiveSkill + PassiveEffect union type (8종 PassiveType)
  - **SkillExecutionEngine**: 재귀적 스킬 체인 실행, MAX_SKILL_CHAIN_DEPTH=3
  - **SkillValidator**: 계층 검증 규칙 5종
  - **데이터**: ActiveSkillTierData(14 패밀리 × 4티어) + PassiveSkillTierData(17 패밀리 × 4티어)
  - **레지스트리**: ActiveSkillRegistry(27 패밀리) + PassiveSkillRegistry(17 패밀리)
  - 전투 시스템(BattleUnit, Battle) 전면 리팩터
  - 연관 파일 전체 동시 수정 (Chapter, Encounter, EncounterGenerator, EnemyTemplate, BattleManager, Arena, ContentScreen, ChapterScreen)
  - 기존 테스트 전면 업데이트 + 신규 테스트 2개 파일 추가

---

## 2026-02-16 (Day 5 - 밸런스 + 비주얼)

### 완료 작업
- **패시브 스킬 밸런스 조정**
  - 천사의 힘: ATK +100% → +30%
  - 중복 패시브 병합: ATK%(atk_proficiency만 유지), DEF%(def_proficiency만 유지), CRIT(crit_mastery만 유지)
  - 반격 메카닉 변경: 피해 반사 → 확률 기반 일반 공격 발동 (triggerChance 0.1~0.5)
  - 연타 확률 하향: 0.1~0.35
  - 기본 크리티컬 확률 5% → 0%

- **인카운터 밸런스 조정**
  - 천사: 체력 회복 제거, 스킬 3개 중 1개 선택으로 변경
  - 악마 확률 15% → 10%, 우연 확률 20% → 25%
  - 우연: 상자 → 스킬 3개 선택, 축복 → 챕터 기반 랜덤 골드
  - 치유의 샘: 50% → 15%

- **전투/경제 밸런스 조정**
  - 전투 골드: 일수(perDay) 계수 추가, perChapter 5→2
  - 챕터 클리어 보상: 데이터 테이블 분리, 챕터별 차등 골드+보석
  - 일수 기반 몬스터 스케일링: 챕터 내 진행에 따라 최대 +80%
  - 재능 스탯: HP 20→15, ATK 5→3, DEF 3→2

- **스킬 비주얼 이펙트 구현**
  - 스킬 투사체: skillIcon이 대상에게 날아가는 애니메이션 (캐릭터 이동 대신)
  - 크리티컬 강화: 24px + CRIT! 라벨 + text-shadow 글로우 + scale 애니메이션
  - 데미지 팝업에 스킬 아이콘 표시

- **상태 효과 아이콘 + DoT 중첩 규칙 구현** (Y-12)
  - 캐릭터 HP바/분노바 아래 상태 효과 아이콘 표시 (버프=녹색, 디버프=빨간 테두리)
  - 남은 턴 수 뱃지, DoT 중첩 수(×N) 표시
  - DoT 중첩 규칙: 같은 스킬(sourceSkillId) → 상위 등급 덮어쓰기, 다른 스킬 → 공존
  - StatusEffect에 sourceSkillId 추가, BattleUnit.addStatusEffect() 분기 로직

- **뽑기 시스템 통합** (Y-13)
  - 브론즈/실버/골드 3종 → 단일 장비 상자로 통합
  - 합성 비율(3:1) 기반 확률 분배: 일반 66.76%, 우수 22.25%, 희귀 7.42%, 에픽 2.47%, 전설 0.82%, 신화 0.27%
  - 비용 150보석, 10연차 1,350보석, 천장 180회 → 신화 확정
  - S-등급 대상 에픽 이상으로 확장
  - 엘리트/보스 몬스터 강화, 보물상자 보상 50% 하향

---

## 2026-02-17 (Day 6)

### 완료 작업
- **데이터 테이블 JSON 분리** (Y-14)
  - 15개 데이터 테이블의 순수 데이터를 JSON 파일로 분리
  - TypeScript 래퍼 파일이 JSON을 import하여 타입 안전성 + 헬퍼 함수 유지
  - 소비자(consumer) 코드 변경 없이 동일 export API 유지
  - JSON 파일은 Excel/VS Code에서 직관적으로 편집 가능
  - 변환 대상: BattleDataTable, EncounterDataTable, ActiveSkillTierData, PassiveSkillTierData, SkillDataTable, EnemyTable, EquipmentDataTable, EquipmentTable, EquipmentPassiveTable, PetTable, ResourceDataTable, AttendanceDataTable, TalentTable, HeritageTable, ChapterTreasureTable
  - 변환하지 않은 파일 (로직 중심): ActiveSkillRegistry, PassiveSkillRegistry, SkillTable
  - tsconfig.app.json에 resolveJsonModule 추가

---

## 2026-02-17 (Day 6 - 오후)

### 완료 작업
- **챕터 획득 골드 UI** — 챕터 헤더 카드에 sessionGold 표시 (Coins 아이콘 + 골드색)
- **상태효과 틱 애니메이션 분리** — DOT_DAMAGE/HOT_HEAL을 적 공격 애니메이션에서 분리, 턴 끝에 캐릭터 이동 없이 데미지 팝업만 표시
- **전투 연출 순서도 기획서** — 01_전투시스템.md에 페이즈 타이밍, 턴 연출 흐름, 히트 그룹 분할, HP 중간 갱신 문서화
- **DoT 메카닉 기획서** — 04_스킬시스템.md에 즉발 피해 vs 지속 피해 구분, DoT 틱 타이밍 문서화
- **엘리트/보스 dayProgress 스케일링** — 일반 몬스터만 적용되던 일수 보너스를 엘리트/보스에도 적용
- **엘리트/보스 챕터별 고정 배치** (Y-15) — 랜덤 풀 → chapterBossAssignment JSON 테이블 기반 고정 배치 (3패턴 순환)

---

## 2026-02-18 (Day 7)

### 완료 작업
- **분노 즉시 발동** — 일반 공격으로 분노 게이지가 차면 같은 턴에 즉시 분노 공격 발동 (다음 턴 대기 제거)
- **PROFICIENCY 스킬 카테고리 삭제** (Y-16)
  - SkillCategory.PROFICIENCY enum 값 제거
  - hp/atk/def/crit_proficiency 4개 스킬 정의, 16개 티어 데이터, 설명 빌더 전체 삭제
  - PassiveSkillRegistry에서 atk/def_proficiency 2개 항목 삭제
  - PassiveSkillId 타입에서 제거
  - 테스트 코드 동기화
- **스킬 밸런스 하향 조정**
  - lifesteal tier 1: 0.15→0.08 (비례 하향)
  - multi_hit_mastery tier 1: 0.25→0.1 (비례 하향)
  - crit_mastery tier 1: 0.15→0.07 (비례 하향)
  - shrink_magic tier 1 reduction: 0.2→0.1 (비례 하향)
  - 각 스킬 tier 2~4도 동일 비율로 하향
- **회복 시스템 최대체력 비례(%) 전환**
  - REGEN (StatusEffect): 고정값→ `Math.floor(maxHp * value)` 비례 회복
  - HEAL_HP (SkillExecutionEngine): 고정값→ `Math.floor(maxHp * amount)` 비례 회복
  - skill-tier.data.json regen: 10/18/28/42 → 0.02/0.035/0.05/0.07
  - passive-skill-tier.data.json regen: 동일 변환
  - active-skill-tier.data.json hp_recovery: 동일 변환
  - equipment-passive.data.json 신발 REGEN: 3~60 → 0.005~0.05
  - pet.data.json REGEN 펫 3종: 고정값→비율값 변환

- **죽은 코드 삭제** — SkillTable.ts, SkillDataTable.ts, skill-tier.data.json (미사용 파일 제거)
- **밸런스 실제 데이터 반영** — 이전 밸런스 변경이 죽은 코드에만 적용된 문제 수정, passive-skill-tier/active-skill-tier에 재적용
- **속성 마스터리 패시브 3종 추가** (SKILL_MODIFIER 구현)
  - 번개 마스터리, 수리검 마스터리, 광창 마스터리 — 태그 기반 데미지 증가 (+8%~25%)
  - BattleUnit에 skillTagBonuses 저장, SkillExecutionEngine ATTACK 케이스에서 태그 배율 적용
- **크리티컬 물리 전용** — 마법/고정 공격에서 크리티컬 제거, 물리만 적용
- **데미지 분산 제거** — variance(0.9~1.1) 랜덤 배율 삭제
- **폭군의 일격 밸런스 하향** — coefficient 0.3→0.11 (비례 하향)
- **연타 밸런스 하향** — chance 0.1→0.07 (비례 하향)
- **연타 일반 공격 전용 + UPPER 스킬 연계** — 연타 발동을 일반 공격에서만 적용, 연타 시 UPPER 스킬(번개/검기/광창 등)도 재발동, 연타 재발동은 차단
- **기획 문서 수치 제거** — 하드코딩된 밸런스 수치(50%, 80%, 20%) 제거, 개념만 남김
- **뇌우 스킬 삭제** — 번개 강타와 컨셉 중복, 코드/데이터/문서 완전 삭제
- **소환 횟수 스킬 정체 구간 제거** — 번개/광창/검기 강타, 분노 번개/광창/화염파 모두 T1~T4 = 1/2/3/4회로 변경
- **몬스터 스케일링 곡선 개선** — scalingPerChapter 1.12→1.25, dayProgressMaxBonus 0.8→0.3 (챕터 N+1 1일 ≈ 챕터 N 50일)
- **몬스터 컨셉 리밸런스** — 일반 몬스터 총 전투력 균등화 (체력형/균형형/공격형/돌격형/방어형)
- **골드 보상 UI 버그 수정** — 인카운터 골드 보상이 sessionGold에 미반영되던 문제 수정

---

## 2026-02-21 (Day 10)

### 완료 작업
- **스킬 연출 순서 그룹화** (Y-19)
  - reorderBySkillType() 함수 추가: 같은 skillName끼리 묶어서 연속 연출
  - ATTACK/RAGE_ATTACK 경계 기준 배치 분리 (연타 배치는 별도 유지)
  - 투사체 이펙트: CRIT 타입도 감지하도록 확장, approach 시점에 damageEntries 선설정

- **로비 상세 스탯 팝업** (Y-20)
  - Player.getStatsBreakdown(): 기본 스탯을 출처별(기본/재능/장비/유산/펫) 분해 반환
  - Player.getCombatPassives(): 장비 패시브 기반 전투 스탯(방어막/연타/재생/버프/마법강화/분노충전) + 고정 전투 스탯(치명타 데미지/흡혈/회피/반격) 반환
  - MainScreen에 [상세] 버튼 + modal-overlay 팝업 UI
  - 기본 스탯 섹션: 총합 + 0이 아닌 출처만 하위 표시
  - 전투 스탯 섹션: 0이어도 전부 표시 (향후 확장 대비)
  - 장비 패시브 섹션: 장착 장비 패시브 합산값 표시

---

## 2026-02-21 (Day 10 - 오후)

### 완료 작업
- **체력 강화 패시브 (hp_fortify)** — T1~T4 최대 체력 5%~15% 증가, 세션 레벨+전투 레벨 이중 적용 방지
- **마법 마스터리 패시브 (magic_mastery)** — T1~T4 마법 계수 +0.1~0.35
- **검기 AoE 공격** — sword_aura_summon에 isAoe 플래그 추가, 모든 생존 적에게 피해
- **방어 공식 변경** — 고정 차감 → 퍼센트 감소 `ATK × (k / (k + DEF))` (k=100 물리, k=150 마법)
- **세션 maxHp 연동** — baseSessionMaxHp/recalcSessionMaxHp/getBattlePassiveSkills 도입, HP 비율 보존
- **연속 스킬 연출 속도 조정** — CONSECUTIVE_SKILL_BOOST 1.7→1.45
- **전투 시작 함수 통합** — startEliteBattle/startMidBossBattle/startBossBattle → startSpecialBattle(type)
- **테스트 코드 추가** — BattleUnit(hp_fortify/magic_mastery/STAT_MODIFIER 순서/방어막), Chapter(세션HP/비율보존/getBattlePassiveSkills), SkillExecutionEngine(AoE/방어공식)
- **스킬 특징(traits) 필드 추가** (Y-22) — ActiveSkillFamilyDef·PassiveSkillFamilyDef에 traits: string[] 필드 추가, 기획/개발 참고용(UI 미표시), 04_스킬시스템.md에 스킬 특징 테이블 문서화

---

## 2026-02-22 (Day 11)

### 완료 작업
- **인카운터 UI 세션 스탯 반영** (Y-23) — PlayerStatsBar에 세션 패시브 스킬의 STAT_MODIFIER 효과 반영, 기본 스탯 대신 버프 적용된 effectiveAtk/effectiveDef 표시
- **40·50일차 확률적 엘리트 전투** (Y-24) — optionalEliteDays 데이터 테이블 추가, Chapter.rollOptionalElite() 확률 판정(30%), isOptionalEliteDay() 플래그, ChapterScreen에서 엘리트 전투 트리거
- **재능 UI 레벨당 증가분 표시** (Y-25) — TalentScreen에 각 스탯의 레벨당 증가량 표시 (TalentTable.getStatPerLevel 참조)
- **스킬 교환 인카운터** (Y-26) — CHANCE 하위 이벤트로 스킬 교환 추가, 보유 T1 스킬 1개를 랜덤 선택하고 새 T1 스킬 3개 중 1개로 교환, EncounterReward에 skillIdsToRemove 필드 추가
- **스킬 리롤 기능** (Y-27) — 스킬 선택 인카운터에서 리롤 버튼 추가, 세션당 2회 제한, Chapter.rerollEncounter()로 같은 타입 재생성
- **장비 부스탯 시스템** (Y-28) — 등급별 0~5개 랜덤 부스탯 시스템 추가, 슬롯별 후보 풀(메인 스탯 제외), 뽑기 시 등급 맞게 생성, 합성 등급 업 시 기존 유지+1개 추가, equipment-substats.data.json 데이터 테이블, EquipmentSubStatTable 래퍼, Equipment/Forge/TreasureChest/SaveSerializer/EquipmentScreen 수정
- **장비 % 패시브 가산 처리** — 장비 간 동일 스탯 % 패시브를 곱연산에서 가산으로 변경 (BattleManager.mergePercentageStatModifiers), 세션 스킬과는 별도 곱연산 유지
- **공격 강화 패시브 스킬** (Y-29) — atk_fortify T1~T4 (6%/10%/14%/18%), STAT_MODIFIER ATK %, SKULL 시너지
- **재능 등급업 보상** (Y-30) — 등급별 스탯 % 보너스 (ATK%/DEF%/HP%) 자동 적용, gradeRewards 데이터 테이블, Player.computeStats()에서 누적 % 계산, 등급업 팝업 UI, StatsBreakdown에 talentGrade 항목 추가
- **스킬 티어 스케일링 가속 곡선** (Y-31) — 전 스킬 티어별 수치를 1:2.3:4:6 비율로 재조정, 증가분이 티어마다 커지는 가속 곡선 적용, 확률 캡(1.0) 스킬은 T1 역산 조정 (revive/shuriken 3종), duration/count/단일티어 스킬 별도 처리

---

## 2026-02-23 (Day 12)

### 완료 작업
- **분노 초과분 유지** (Y-32) — 분노 100 소모 후 초과분을 버리지 않고 유지하도록 변경, SkillExecutionEngine에서 Math.min 캡 제거
- **수리검 강타 스킬** (Y-33) — 매 일반 공격 시 수리검 소환하는 UPPER 스킬, onSkillActivation('ilban_attack') 트리거, count 1→2→4→6
- **방어 강화 패시브** (Y-34) — def_fortify T1~T4 (8%→18%→32%→48%), STAT_MODIFIER DEF %, KNIGHT 시너지
- **독 수리검 스킬** (Y-35) — 2턴마다 수리검에 확률적 독 주입, INJECT_EFFECT poison_inject, SHURIKEN+POISON 태그, RANGER 시너지
- **인카운터 정리** (Y-36) — ANGEL 타입 제거→우연(CHANCE) 통합, T2 이하 제한 해제, DEMON 가중치 10→7%, CHANCE 가중치 53%, 하위 가중치 skillBox 60/spring 15/blessing 15/skillSwap 10, 중박/대박 카운터 증가 비활성화(시스템 유지), 카운터 바 UI 제거
- **체력 비례 공격/방어 패시브** (Y-37) — LOW_HP_MODIFIER 타입 신규 추가, 배수진(low_hp_atk: 체력 낮을수록 ATK 증가, 최대 T4 +120%), 불굴(low_hp_def: 체력 낮을수록 DEF 증가, 최대 T4 +120%), getEffectiveAtk/Def에서 매 공격/방어 시 HP 비율 기반 실시간 보정
- **최대 HP 비례 물리 추가 데미지 패시브** (Y-38) — MAX_HP_DAMAGE 타입 신규 추가, 압도(max_hp_damage): 물리 공격 시 maxHP×coefficient만큼 ATK에 추가(방어 적용), T1 2%~T4 12%, KNIGHT 시너지
- **적 HP 비례 물리 공격 + 기절 스킬** (Y-39) — 분쇄(hp_crush): 적 maxHP 비례 물리 데미지(isTargetHpBased), SKULL 시너지 / 기절(stun_apply): LOWEST SkillEffectType.STUN(chance+duration), 강타(stun_strike): UPPER TRIGGER_SKILL→stun_apply, KNIGHT 시너지, processEnemyTurn 기절 체크
- **재능 등급 골드 보상** (Y-40) — gradeRewards에 goldPercent 추가(2%~12%, 누적30%), Player.getGoldMultiplier(), 챕터 내 전투/인카운터/클리어 골드에만 적용, 등급업 팝업 표시
- **재능 등급 보상 마일스톤 재설계** (Y-41) — 기존 등급 스탯% 보너스 삭제 → 등급업 ATK/DEF 교대 보너스(10/15/30/60/120) + 레벨 간격 마일스톤(골드 일시/골드 획득량% 교대), 통합 프로그레스 바 UI(미수령 보상/현재 레벨 자동 스크롤), Player.getGoldMultiplier() 챕터 내 골드 적용

---

## 2026-02-24 (Day 13)

### 완료 작업
- **재능 서브 등급(단) 시스템** (Y-42) — 200개 서브 등급(30레벨/단 = 공10+방10+체10), 스탯별 10레벨 캡 + 서브 등급 진급 시 리셋, 마일스톤 10레벨 간격(서브 등급당 2개), 총 레벨 max 2010, 등급 경계(60/210/510/1020/2010), gradeConfig 기반 비용 자동 생성, 서브/메인 등급 전환 시 ATK/DEF 교대 스탯 보너스, 골드 마일스톤 = 강화비용×3, TalentScreen UI 재설계(등급 진행 바 + 마일스톤 카드 + 서브 등급 레벨 표시)

---

## 2026-02-27 (Day 14~15)

### 완료 작업
- **[Unity] 도메인 로직 포팅** (Y-43) — TS→C# 전체 포팅 (67파일+22테스트), 250개 테스트 통과, 크로스 검증 완료
- **[Unity] Presentation 레이어 전체 구현** (Y-44~Y-56)
  - **Core 프레임워크**: UIManager(싱글톤 Screen 전환/Popup 스택), BaseScreen/BasePopup 추상 클래스, ScreenType enum(12개), SpriteManager/SpriteDatabase/PlaceholderGenerator(런타임 플레이스홀더 스프라이트), ColorPalette(등급/UI 색상), NumberFormatter(K/M/B), SafeAreaFitter
  - **전투 연출 시스템** (Y-44, Y-45): BattleView(Coroutine 오케스트레이터), CharacterView(HP/분노/쉴드 바+상태이상 아이콘), DamagePopup/Pool(DOTween 부유+페이드), ProjectileView(스킬 투사체), StatusEffectIconView, 배속 1x/2x
  - **ChapterScreen** (Y-46): 인카운터 선택, 전투 진입/결과, 스킬 획득 카드(3택+리롤), 일차 진행, 중박/대박 카운터, 설정 오버레이(세션 스킬 목록+모험 포기/계속), 데미지 그래프, ChapterResultPopup/EliteRewardPopup
  - **MainScreen** (Y-47): 플레이어 스탯 요약 카드, 메뉴 카드 4개(모험/컨텐츠/재능/가챠), StatsDetailPopup(출처별 기본/전투/장비패시브 스탯 분해)
  - **EquipmentScreen** (Y-48): 페이퍼 돌 7슬롯 그리드, 인벤토리 필터(전체/등급별), 장착/해제, 골드 강화, 합성(Forge) 탭, 부스탯 표시, 등급별 색상
  - **TalentScreen** (Y-49): 3열 스탯 강화 버튼(ATK/HP/DEF), 슬라이딩 윈도우 마일스톤 프로그레스 바, 서브 등급(단) 진행 표시, 유산 카드
  - **PetScreen** (Y-50): 리소스 카드, 펫 쇼케이스+스탯 카드, 5열 펫 그리드, 먹이주기/등급업/장착
  - **GachaScreen** (Y-51): 장비/펫/보석 상자 탭, 1회/10회 뽑기, 천장 진행도 바, 결과 카드 연출
  - **ContentScreen** (Y-52): 던전/탑/아레나/여행/고블린/카타콤 6개 메뉴 카드, 각 서브패널(전투/결과)
  - **QuestScreen + EventScreen** (Y-53): 일일/주간 퀘스트 탭, 미션 진행도 바, 보상 수령, 출석 캘린더 그리드
  - **SettingsScreen + DebugScreen** (Y-54): 저장/불러오기/삭제/내보내기/가져오기, 리소스 추가/챕터 설정/퀘스트 완료 치트
  - **공통 컴포넌트** (Y-55): NavBarView(10탭), ResourceBarView(골드/젬/스태미나/토큰/티켓), PlayerStatsBarView, ProgressBarView, DamageGraphView, TabBarView
  - **ChapterTreasureScreen** (Y-56): 챕터별 마일스톤 보상, 진행도 바
  - 총 ~45개 C# 파일 (Core 7 + Utils 3 + Components 7 + Battle 7 + Screens 12 + Popups 6 + 기타 3)
  - 모든 UI 프로그래매틱 빌드 (프리팹 없음), SpriteDatabase 슬롯에 이미지 드래그&드롭만으로 교체 가능
- **죽은 코드 삭제** — StatsDisplayView.cs, EquipmentIconView.cs, PetIconView.cs (화면 재작성 후 미참조 컴포넌트 삭제)
- **[Unity] UI 레이아웃 품질 수정** (Y-57)
  - 12개 화면 스크롤 콘텐츠 RectTransform offset 초기화: anchor 변경 후 offsetMin/offsetMax = Vector2.zero 누락 수정 (TalentScreen, ChapterTreasureScreen, ContentScreen, GachaScreen, QuestScreen, EventScreen, SettingsScreen, DebugScreen, PetScreen, EquipmentScreen×2, ChapterScreen, MainScreen)
  - 폰트 크기 증가: ChapterTreasureScreen(13→20, 11→18), ContentScreen(14→22), QuestScreen(13→20), GachaScreen(13→20), EventScreen(10→14), SettingsScreen(10→16)
  - ResourceType 한글 라벨: NumberFormatter.FormatResourceType() 추가 (14종 리소스 매핑), ChapterTreasureScreen/ContentScreen/QuestScreen/GachaScreen 적용
  - MainScreen: 그리드 cellSize 490→400 (좁은 화면 오버플로 방지), StatsDetailPopup에 VerticalLayoutGroup+LayoutElement 추가
  - ProgressBarView: Initialize()에서 고정 sizeDelta 제거, LayoutGroup이 외부 크기 제어
- **딜그래프 분류 버그 수정 + 테스트** (Y-58)
  - BattleLogCategorizer 유틸 클래스 추출 (TS: `BattleLogCategorizer.ts`, C#: `BattleLogCategorizer.cs`) — 전투 로그 엔트리를 damageMap/healMap으로 분류하는 순수 함수
  - **스킬 크리티컬 분류 버그**: CRIT 타입 엔트리에 SkillName이 있으면(스킬 크리티컬) 해당 스킬명으로 분류하도록 수정 (기존: "일반 공격"으로 잘못 분류)
  - **HEAL 타입 누락 버그**: BattleLogType.HEAL(스킬 힐)이 healMap에 집계되지 않던 문제 수정, 스킬명으로 분류
  - ChapterScreen(TS+C#) 분류 로직을 BattleLogCategorizer로 교체
  - 14개 테스트 케이스 추가 (TS: `BattleLogCategorizer.test.ts`, C#: `BattleLogCategorizerTests.cs`)
    - 유닛 테스트: 일반 공격/크릿/스킬 데미지/스킬 크릿/반격/분노/독/흡혈/재생/부활/스킬 힐 분류
    - 통합 테스트: 실제 전투 실행 후 크릿 스킬명 분류 검증, 전체 딜 합산 일치 검증
  - 전체 260개 테스트 통과
- **딜그래프 연출 동기화 + UI 개선** (Y-58 연장)
  - **BattleView 연출 동기화**: OnTurnEntries를 턴 시작 시 한번에 호출 → 애니메이션 그룹별 호출로 변경, 공격 연출이 발생할 때마다 그래프가 점진적으로 업데이트
  - **DamageGraphView UI 개선**: 헤더에 총합 표시 ("총 12.5K"), 각 바에 비율% 표시, 라벨 폰트 크기 22→24 + Bold, 라벨 너비 140→160px + minWidth 100px, 값 영역 70→110px, 행 높이 28→32px
- **재능 화면 아이콘 렌더링 수정** (Y-58 연장)
  - TMP에서 emoji(🪙📈)가 렌더링 안 되는 문제: TMP 호환 유니코드/문자로 교체
    - 골드 마일스톤: "G" (골드색), 골드 획득량: "%" (초록색), 서브 등급 전환: "▲", 메인 등급 전환: "★" (골드색), 수령 완료: "✓" (초록색)
  - 노드별 IconColor 필드 추가, 상태(reached/locked)에 따라 색상+투명도 적용
  - 아이콘 크기 22→26 Bold, 설명 크기 14→18, 노드 영역 높이 70→80
  - 보상 팝업 아이콘도 emoji → TMP 호환 문자로 교체
- **이미지 리소스 플레이스홀더 시스템** (Y-59)
  - 14개 분홍색 64x64 PNG 플레이스홀더 생성 (`Assets/_Project/Art/UI/Icons/`)
    - 재능 화면: icon_gold, icon_gold_boost, icon_upgrade, icon_star, icon_check
    - 스탯: icon_atk, icon_hp, icon_def, icon_crit
    - 재화: icon_gold_currency, icon_gem, icon_stamina, icon_token, icon_ticket
  - SpriteDatabase에 `IconSpriteEntry[]` 배열 추가 — Inspector에서 id+sprite 매핑
  - SpriteManager에 `GetIcon(iconId)` 메서드 추가 — SpriteDatabase에 있으면 사용, 없으면 PlaceholderGenerator 분홍 박스 폴백
  - TalentScreen 아이콘: TextMeshProUGUI(emoji) → Image 컴포넌트 + SpriteManager.GetIcon() 전환
    - 노드 아이콘, 스탯 강화 카드 아이콘, 보상 팝업 아이콘 모두 Image 기반
  - `README_ICONS.txt` 매니페스트 생성 — 각 파일 용도 설명, 교체 방법 안내
- **딜그래프 누적 방식 수정** (Y-60)
  - BattleView의 OnTurnEntries 호출 방식 변경: 애니메이션 그룹별 분할 호출 → 턴 단위 `result.Entries` 일괄 호출 (TS 레퍼런스와 동일)
  - 기존 문제: 적 HP≤0 또는 플레이어 HP≤0일 때 남은 그룹의 break로 일부 엔트리가 그래프에 반영 안 됨
  - per-group `OnTurnEntries?.Invoke(group)` 3곳 삭제, `OnTurnEntries?.Invoke(result.Entries)` 턴 시작 시 1회 호출로 통합
  - 모험화면_기획서.md에 딜 그래프 섹션(§6) 추가 — 누적 모델, 분류 규칙(BattleLogCategorizer), 표시 형태, 토글 동작, 패배 시 추가 정보
- **리소스 아이콘 목록 문서** (Y-60)
  - `ICON_LIST.md` 생성 — 전체 스프라이트/아이콘 리소스 인벤토리
    - 재능 아이콘 5종, 스탯 아이콘 4종, 재화 아이콘 5종
    - 캐릭터 스프라이트 14종(idle+hit), 장비 아이콘 42조합(7슬롯×6등급)
    - 펫 아이콘 18종, 상태이상 아이콘 9종, UI 공통 스프라이트 8종
    - 파일명, 용도, 권장 크기, 교체 방법 포함
- **UI 품질 수정 + 아이콘 자동 로드** (Y-61)
  - **SpriteManager 아이콘 자동 로드**: `Resources.LoadAll<Sprite>("Icons")`로 `Resources/Icons/` 폴더 내 모든 스프라이트를 자동 등록. Inspector에서 SpriteDatabase에 수동 할당 불필요. 파일명(확장자 제외)이 아이콘 ID
  - 아이콘 파일을 `Art/UI/Icons/` → `Resources/Icons/`로 이동
  - **TalentScreen 마일스톤 아이콘**: preferredWidth=36 추가, preserveAspect 제거, SpriteManager.Instance null 체크
  - **ContentScreen 탭 아이콘**: 아이콘 텍스트 첫 글자만 표시(overflow 방지), wordWrap 해제
  - **EventScreen 출석부**: 셀 110x80→240x120, 열 7→4, Day fontSize 20→26 Bold, 보상 fontSize 14→22
  - **StatsDetailPopup**: RectMask2D→Image+Mask(클리핑 확실하게), ScrollRect Elastic 추가
- **아이콘 임포트 수정 + 마일스톤 노드 렌더링 수정** (Y-62)
  - **아이콘 spriteMode 불일치 수정**: PNG가 `spriteMode: 2`(Multiple)로 임포트되어 스프라이트 이름이 `icon_atk_0`으로 생성됨. SpriteManager에서 `_0` 접미사를 자동으로 벗겨서 원본 이름으로도 매핑
  - **Editor .asmdef 생성**: `CatCatGo.Editor.asmdef` 생성 (Editor 전용 어셈블리). IconImportSettings.cs가 컴파일 안 되던 문제 해결
  - **IconImportSettings 조건 수정**: textureType이 이미 Sprite여도 spriteMode가 Single이 아니면 수정하도록 변경. "Tools > Reimport All Icons" 메뉴로 기존 아이콘 일괄 수정
  - **TalentScreen 마일스톤 노드 null 버그**: `_nodeContainer = AddComponent<RectTransform>()`이 null 반환 (LayoutElement가 이미 RectTransform 자동 추가). `GetComponent<RectTransform>() ?? AddComponent<RectTransform>()`로 변경. 노드가 씬 루트로 빠져서 Canvas 밖에서 보이지 않던 문제 해결
  - **스킬 아이콘 목록**: 아이콘_리소스_목록.md에 §9 추가 — 액티브 스킬 29종 + 패시브 스킬 19종 현재 이모지 아이콘 + 스프라이트 교체 방법
- **장비 아이콘 자동 로드** (Y-63)
  - SpriteManager에 `Resources.LoadAll<Sprite>("Icons/equip")` 추가 — `equip_{slot}_{grade}.png` 파일을 `{SLOT}_{GRADE}` 키로 자동 매핑
  - `_256` 변형 자동 무시, `StripSpriteModeSuffix` 공통 메서드로 spriteMode 접미사 제거 통합
  - `GetEquipmentIcon`에서 리소스 로드 스프라이트 우선 확인 → SpriteDatabase → 플레이스홀더 순서
  - 아이콘_리소스_목록.md §5 장비 아이콘 섹션 업데이트 (자동 로드 경로 + 교체 방법)
- **RectTransform null 버그 전역 수정 + 스킬 아이콘 자동 로드** (Y-64)
  - **AddComponent<RectTransform>() null 버그 전역 수정**: Unity 6에서 UI 컴포넌트(Image, LayoutElement 등)가 RectTransform을 자동 생성하여 이후 AddComponent<RectTransform>()이 null 반환. 23개 파일(170+ 인스턴스)에서 `GetComponent<RectTransform>() ?? AddComponent<RectTransform>()` 패턴으로 일괄 교체
  - **영향받던 화면**: EquipmentScreen(탭 패널 null→양쪽 동시 표시), ChapterScreen(다수 패널), BattleView, ContentScreen, GachaScreen, PetScreen 등 전체 Presentation 레이어
  - **스킬 아이콘 자동 로드**: SpriteManager에 `Resources.LoadAll<Sprite>("Icons/skill")` 추가. `icon_skill_{id}.png` → skill ID로 매핑. `GetSkillIcon(skillId)` API 추가
  - **ChapterScreen 스킬 아이콘 스프라이트 전환**: 세션 스킬 아이콘(28x28), 엘리트 보상 스킬 카드(24x24), 설정 스킬 목록(22x22) 3곳에서 emoji 텍스트 → Image 스프라이트로 전환 (스프라이트 없으면 emoji 폴백)
- **장비/재능 UI 개선** (Y-65)
  - **EquipmentScreen 스프라이트 아이콘 적용**: 인벤토리 아이템, 페이퍼돌 장착 슬롯, 합성 탭 아이콘 3곳에서 SpriteManager.GetEquipmentIcon() 호출하여 실제 스프라이트 표시. 기존 텍스트 약어(무기/갑옷 등) 제거
  - **EquipmentScreen 탭 축소**: 탭 바 높이 44→36, 폰트 24→20, 패딩 조정
  - **EquipmentScreen 인벤토리 라벨 개선**: 강화 레벨이 있으면 "+N" 등급색 표시, 없으면 슬롯명. 합성 뱃지 폰트 22→14
  - **TalentScreen 마일스톤 노드 축소**: nodeArea 80→52, 아이콘 36→22, 설명 폰트 18→13, 설명 높이 26→16
  - **TalentScreen 스탯 카드 재배치**: 레퍼런스 이미지 기준 LV 상단→아이콘 40px→보너스(+값)→스탯명→비용 순서. 카드 배경 ButtonPrimary→CardLight
- **장비 UI 대규모 확대 + 상세 패널 분리** (Y-66)
  - 인벤토리 그리드 셀 2배 확대 (5열→4열, 120x160), 페이퍼돌 셀 96px, 높이 440px
  - 탭 바 최소화 (높이 22px, flexibleHeight=0)
  - 보관함 클릭 → 즉시 장착에서 상세 패널 경유로 변경
  - 상세 패널 모드 분리: 장착 모드(강화/해제) vs 보관함 모드(장착/판매/닫기)
  - 장비화면_기획서.md 동기화 (수치 제거, 개념만 기술)
- **전투 스킬 아이콘 스프라이트 전환** (Y-67)
  - BattleLogEntry/SkillDamageResult에 SkillId 필드 추가
  - SkillExecutionEngine(5곳) + Battle.cs(6곳)에서 SkillId 전달
  - DamagePopup: emoji 텍스트 → HorizontalLayoutGroup(Image+Text) 구조로 재작성
  - SpriteManager.GetSkillIcon(skillId)로 스프라이트 표시, 아이콘 페이드 아웃 애니메이션

---

## 2026-02-28 (Day 16)

### 완료 작업
- **GachaScreen 레이아웃 재설계** — 쇼케이스 영역(flexibleHeight=1) 추가, 탭/정보카드/버튼 고정 높이(flexibleHeight=0), 상자 아이콘+이름 중앙 배치
- **UI 테스트 에디터 윈도우** — UITestWindow (Ctrl+Shift+U): 12개 화면 전환, 팝업 테스트, 리소스 추가 버튼
- **ChapterScreen 헤더 2배 확대** — titleRow/설정/일차 표시 높이 및 폰트 2배 확대
- **ChapterTreasureScreen 2배 확대** — 전체 UI 요소 높이/폰트 2배, ContentSizeFitter 자동 높이
- **플레이어 캐릭터 스프라이트 애니메이션** (Y-68)
  - player.png 스프라이트 시트(1024×1536, 4열×2행) 런타임 슬라이싱: Sprite.Create()로 걷기 4프레임 + 공격 4프레임
  - SpriteManager: GetPlayerWalkFrames()/GetPlayerAttackFrames() 캐시 로딩
  - CharacterView: SetFrames() + 프레임 애니메이션 코루틴, SetPhase()에서 걷기↔공격 모션 자동 전환
  - BattleView: 전투 시작 시 플레이어에 스프라이트 프레임 자동 적용
  - player.png.meta: isReadable=1 설정 (런타임 Sprite.Create 필수)
- **적/스테이지 테마 변경: 판타지→실존 생명체** (Y-69)
  - 일반몹 5종: slime→일개미, goblin→참새, skeleton→거미, orc→다람쥐, dark_knight→고슴도치
  - 엘리트 2종: elite_wolf→왕사마귀, elite_mage→독수리
  - 챕터 보스 3종: boss_dragon→큰곰, boss_demon→왕뱀, boss_golem→코끼리
  - 던전 보스 3종: dungeon_dragon→여왕벌, world_tree→고목나무 정령, harpy→백호
  - 던전명 변경: 용의 둥지→거대 벌집, 세계수→수천년 고목, 하늘섬→호랑이 절벽
  - 변경 파일: enemy.data.json, encounter.data.json, dungeon.data.json, resource-labels.data.json, ContentScreen.cs, 05_스테이지던전시스템.md
  - 스탯/스킬/밸런스 수치 동일 유지, 260개 TS 테스트 통과
- **챕터 테마 시스템** (Y-70)
  - 10챕터 단위 5개 스테이지 테마: 미생물→곤충→소형동물→중형동물→대형동물
  - 50종 적 템플릿 추가 (테마당 일반 5 + 엘리트 2 + 보스 3)
  - `enemy.data.json`에 `chapterThemes` 배열 추가: 챕터 범위별 enemy/elite/boss 풀 + bossRotation
  - `EnemyTable.ts`에 `getEnemyPoolForChapter(id)`, `getBossAssignmentForChapter(id)` 추가
  - `Chapter.ts` createCombatBattle/createEliteBattle/createMidBossBattle/createBossBattle가 테마 인식 풀 사용
  - C# `EnemyTable.cs`, `Chapter.cs` 동일하게 반영, JSON 양쪽 복사
  - 260개 TS 테스트 통과

---

## 2026-03-01 (Day 17)

### 완료 작업
- **분노 시스템 리팩토링** — 플레이어/적 분노 시스템을 스킬 시스템으로 통합
  - `ragePerAttack`를 BattleUnit 캐릭터 스탯으로 추가 (rage_accumulate 티어 데이터에서 추출)
  - `ragePowerMultiplier` 제거 → rage_mastery를 SKILL_MODIFIER(RAGE 태그)로 전환, 기존 스킬 태그 보너스 시스템 활용
  - `AddRageEffect`에 `useSourceStat` 플래그 추가, rage_accumulate가 캐릭터의 ragePerAttack 스탯 사용
  - 적에게 빌트인 스킬(ilban_attack, bunno_attack, rage_accumulate) 자동 부여 (EnemyTemplate.buildEnemySkills)
  - Battle.ts: processPlayerTurn/processEnemyTurn → processUnitTurn으로 통합 (플레이어/적 동일 로직)
  - 적 분노 하드코딩 로직 제거 (ATK×1.2 고정 데미지 → 스킬 계수 기반 물리 공격)
  - battle.data.json에서 `playerRagePerAttack`, `attackMultiplier`, `skill` 섹션 삭제
  - 261개 테스트 통과
