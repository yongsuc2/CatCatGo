# 카피바라고! 개발일지 — kkwndud

---

## 2026-02-25

### 일일 던전 전투 시스템 전면 재설계 (K-17)
- 즉시 클리어 방식 → 실시간 전투 애니메이션으로 변경
- 3종 던전별 고유 몬스터 추가: Dungeon Dragon(에메랄드 용), World Tree(나무 몬스터), Harpy(하피)
- `dungeon.data.json` + `DungeonDataTable.ts` 데이터 테이블 신규 생성
- `enemy.data.json`에 3종 적 템플릿 추가
- `DailyDungeon.ts` 전면 재설계: 스테이지 기반 난이도(clearedStage 영구 누적), createBattle/onBattleVictory/getSweepReward
- `DailyDungeonManager`: 개별 던전 3회 → 전체 공유 3회 일일 제한으로 변경
- `GameManager`: enterDungeon 삭제 → challengeDungeon/onDungeonBattleResult/sweepDungeon 신규
- `SaveSerializer`: 던전 세이브 구조 변경 + 기존 세이브 역호환 마이그레이션
- `CharacterSprite.tsx`: 3종 SVG 스프라이트 + NAME_TO_TYPE 매핑 추가
- `ContentScreen.tsx`: 던전 탭 전면 재설계 — DungeonPhase 상태 머신(select/battling/result/sweep-result), BattleArena 기반 1:1 전투 애니메이션 루프, 도전/소탕 버튼
- `DailyDungeon.test.ts`: 새 API에 맞게 테스트 전면 재작성 (10개 테스트 통과)

---

## 2026-02-23

### 장비 패시브 시스템 전체 삭제 (K-15)
- `EquipmentPassiveTable.ts`, `equipment-passive.data.json` 파일 삭제
- `Equipment.getPassive()` 메서드 제거
- `Player.getCombatPassives()`에서 장비 패시브 필드 8개 제거 (shieldPercent, multiHitChance, regenPercent, atkUpBuff, critUpBuff, defUpBuff, magicBoost, rageBoost)
- `BattleManager`에서 `getEquipmentPassiveSkills()`, `passiveToPassiveSkill()`, `mergePercentageStatModifiers()` 제거
- `EquipmentScreen`에서 `getPassiveTypeName()`, `renderPassiveInfo()`, 패시브 표시 UI 전부 제거
- `MainScreen` 상세 스탯 팝업에서 "장비 패시브" 섹션 제거
- `ChapterScreen`, `ContentScreen`에서 전투 생성 시 장비 패시브 수집 로직 제거

### 챕터 결과 화면 개선 (K-16)
- 패배 시 실패 일차 표시: "챕터 N (현재일/총일일차)" 형태
- 패배 결과의 딜그래프 아래에 적 남은 HP를 별도 카드로 표시 (프로그래스바 + 수치/퍼센트)
- 모험 중 실시간 딜그래프에는 적 HP 미표시 (결과 화면 패배 시에만)

---

## 2026-02-20

### UI 비주얼 개선 — ChapterScreen + CSS 디자인 토큰 (K-7 1단계)
- `:root`에 CSS 디자인 토큰 추가: 색상 18종(bg/border/accent/text/gold/green/purple/danger), 폰트 5종, 간격 4종, 반경 3종
- 유틸리티 클래스 추가: `.flex-row/col/between/center`, `.gap-xs/sm/md/lg`, `.text-xs/sm/base/secondary/muted/gold/danger/green/purple/bold/center`, `.w-full`
- `.modal-overlay` 공통 모달 클래스 추가
- ChapterScreen 전용 CSS 클래스 추가: `.chapter-card-header`, `.chapter-settings-btn`, `.chapter-start-row`, `.chapter-gold-label`, `.chapter-battle-controls`, `.chapter-result-graph`, `.chapter-result-buttons`, `.chapter-result-btn`
- 설정 모달 CSS 클래스: `.settings-modal-title`, `.settings-skill-count/list/item/icon/name/desc`, `.settings-empty`, `.settings-actions`
- ChapterScreen.tsx: 인라인 스타일 ~40건을 CSS 클래스로 교체
- index.css 기존 하드코딩 → CSS 변수 치환: `#e94560`→accent, `#aaa`→text-secondary, `#ddd`→text-bright, `#ffd700`→gold, `#ff5252`→danger, `#4caf50`→green, `#c73a52`→accent-hover, `#444`→border-light, `#333`→border, `#e0e0e0`→text-primary 등 대량 치환

---

## 2026-02-19

### 장비 강화 비용 구간별 기울기 시스템
- 기존 강화석 1개 고정 비용 → 구간별 복리 배율 시스템으로 변경
- `equipment-constants.data.json`에 `upgradeCostTiers` 배열 추가 (6구간)
- `EquipmentTable.getUpgradeCost(level)`: 레벨별 누적 복리 비용 계산
- `EquipmentTable.getTotalUpgradeCost(level)`: 0→level까지 총 비용 합산 (환불용)
- `Equipment.getUpgradeCost()`, `Equipment.getTotalUpgradeCost()` 메서드 추가

### 장비 강등 기능
- 장착 장비의 강화 레벨을 0으로 초기화, 투입 강화석 100% 반환
- `Equipment.demote()` 메서드 추가 (레벨/승급 초기화 + 총비용 환불)
- EquipmentScreen: RotateCcw 아이콘 + 확인 패널 UI (반환 수량 표시 + 강등 버튼)

### 일괄 강화 기능
- 보유 강화석이 허용하는 최대 레벨까지 한 번에 강화
- EquipmentScreen: ChevronsUp 아이콘 + `bulkUpgradeEquipment()` 함수
- 승급 필요 시 자동 중단

### 뽑기 화면 용어 변경
- GachaScreen: "천장" → "신화 확정", "천장까지" → "신화 확정 뽑기까지"

---

## 2026-02-17

### 장비 화면 RPG 페이퍼돌 UI 재설계
- 기존 슬롯별 7탭 → 2탭(장비/합성) 구조로 변경
- 장비 탭: 페이퍼돌 레이아웃 (4행×3열 CSS Grid, 캐릭터 중앙 span 2)
- 8슬롯 배치: 좌(목걸이/반지×2) / 중앙(모자/캐릭터/신발) / 우(무기/방어구/장갑)
- 슬롯 셀: 빈 슬롯(투명 아이콘 윤곽, 클릭→해당 슬롯 보관함 필터) / 장착(등급 색상 테두리) / 선택(빨간 강조)
- 장비 상세 패널: 슬롯 클릭 시 스탯+패시브+강화/해제, 보관함 클릭 시 스탯+패시브+장착/판매
- 보관함 아이콘 그리드: 5열, 필터 선택 시에만 표시 (초기 미표시)
- 슬롯 클릭 시 보관함 자동 필터 (모자→모자만, 무기→무기만)
- CharacterSprite 컴포넌트를 페이퍼돌 중앙에 배치
- `index.css`: `.paper-doll`, `.paper-doll-slot`, `.inv-grid`, `.equip-detail-panel` 스타일 추가

### 플레이어 능력치 표시 + 변화 알림
- 신발 아래 플레이어 총 능력치 요약 (ATK/HP/DEF/CRIT) 한 줄 표시
- 장착/해제/강화 시 능력치 변화량 화면 중앙 토스트 (0.7초, 초록=증가/빨강=감소)
- `index.css`: `.equip-stats-bar`, `.stat-toast`, `@keyframes stat-toast-pop` 추가

### 합성 탭 UI 리디자인: 아이콘 그리드 + 일괄 합성
- 합성 탭 전면 재설계: 리스트 카드 → 아이콘 그리드 + 미리보기 레이아웃
- 일괄 합성 기능: 가능한 모든 그룹을 한 번에 합성, 결과 카드에 EquipmentIcon + 등급 색상 이름 표시 (4초 표시)
- 상단 미리보기: 선택 장비의 재료 아이콘 다이어그램 + 결과 등급/스탯/패시브
- 하단 아이콘 그리드: 5열 CSS Grid, 등급 테두리/수량 뱃지/합성 가능 glow
- `index.css`: `.forge-grid`, `.forge-grid-item`, `.forge-badge`, `.forge-preview` 스타일 추가

### 장비 강화 스탯 고정값 변경
- 강화 시 능력치 증가 방식 변경: 곱연산(레벨당 5%) → 고정값 가산(레벨당 +8)
- `EquipmentTable.UPGRADE_FLAT_PER_LEVEL = 8`, 기본 스탯이 있는 항목에만 적용
- 고등급 장비의 과도한 능력치 스케일링 해소

### 에픽 이상 합성 mergeLevel 단계 시스템
- 에픽/전설 합성: 3개 → 2개 변경, mergeLevel(+0/+1/+2) 단계 도입
- 에픽+0×2→에픽+1, 에픽+1×2→에픽+2, 에픽+2×2→전설 (전설→신화도 동일)
- Equipment 엔티티에 `mergeLevel` 프로퍼티 추가
- EquipmentTable: `isHighGradeMerge()`, `getMergeEnhanceMax()` 메서드 추가
- Forge: canMerge/merge/findMergeCandidates에 mergeLevel 로직 반영
- SaveSerializer: mergeLevel 직렬화/역직렬화 + 하위호환
- EquipmentScreen: 장비 이름에 "+N" 표시, 합성 그룹 mergeLevel별 분리, 결과 미리보기

### 펫 경험치 프로그레스 바
- PetScreen: 현재 EXP/필요 EXP 프로그레스 바 + "먹이 N개→레벨업" 표시 추가

---

## 2026-02-15

### 전투 배속 기능 (1x / 2x)
- ChapterScreen에 `battleSpeed` state + ref 추가, delay/PHASE_DURATION을 배속으로 나눠 적용
- BattleArena에 `speedMultiplier` prop 추가, `--battle-speed` CSS 변수로 애니메이션 속도 제어, 데미지 팝업 타이머 조정
- AdventureStage에 `speedMultiplier` prop 전달 인터페이스 추가
- index.css: `.ba-character`, `.ba-hit`, `.ba-hp-fill`, `.ba-damage-popup`, `.ba-rage-fill`, `.ba-shield-fill` transition/animation에 `calc(... / var(--battle-speed, 1))` 적용
- 전투 중 딜 그래프 버튼 왼쪽에 배속 토글 버튼 배치

### 장비 화면 RPG식 탭 구조 변경
- 기존 "장착/보관함/합성" 3탭 → "무기/방어구/반지/장신구/합성" 5탭으로 변경
- 각 슬롯 탭에서 장착 중 아이템 + 보관함 아이템을 한 화면에서 확인/장착/해제 가능
- 보관함 아이템은 등급 높은 순 → S등급 우선 → 레벨 높은 순 정렬
- 장착 슬롯에 빨간 테두리 강조 추가

### 장비 슬롯 7종 확장 + 장비 아이콘 SVG
- 기존 4종(무기/방어구/반지/장신구) → 7종(무기/방어구/반지/목걸이/신발/장갑/모자)으로 확장
- ACCESSORY(장신구) → NECKLACE(목걸이)로 마이그레이션 (SaveSerializer에 변환 로직 추가)
- 새 슬롯별 스탯: 목걸이(ATK+HP 혼합), 신발(HP), 장갑(ATK), 모자(HP)
- 반지만 2개 장착, 나머지 전부 1개
- EquipmentIcon SVG 컴포넌트 생성: 슬롯+등급별 색상/형태 차별화
- 무기 아이콘: 등급에 따라 검(일반~우수) → 지팡이(희귀~에픽) → 활(전설~신화) 형태 변화
- 장비 화면/뽑기 화면에 아이콘 적용, 뽑기 결과에 슬롯 한글 라벨 표시

## 2026-02-16

### 장비 등급별 패시브 능력 + 무기 종류(검/지팡이/활)
- WeaponSubType enum(SWORD/STAFF/BOW) 추가, AOE_DAMAGE EffectType 추가
- EquipmentPassiveTable 데이터 테이블 생성: 무기(검=ATK_UP, 지팡이=AOE_DAMAGE, 활=RAGE_BOOST) + 비무기 6종(방어구=SHIELD, 반지=CRIT_UP, 목걸이=ATK_UP, 신발=REGEN, 장갑=MULTI_HIT, 모자=DEF_UP) × 6등급
- Equipment 엔티티에 weaponSubType 프로퍼티 + getPassive() 메서드 추가
- TreasureChest: 무기 생성 시 랜덤 subType 배정, 장비 이름에 종류 반영
- Forge: 같은 weaponSubType만 합성 가능, 결과에 subType 상속
- SaveSerializer: weaponSubType 직렬화 + 기존 무기 id hash 기반 결정적 마이그레이션
- BattleManager: getEquipmentPassiveSkills(player) 메서드 추가, 장비 패시브를 Skill 객체로 변환
- Battle.ts: AOE_DAMAGE 처리 (주 타겟 외 나머지 적에게 ATK% 범위 피해)
- ChapterScreen 4곳 + ContentScreen 2곳: BattleUnit 생성 시 장비 패시브 스킬 포함
- EquipmentIcon: 무기 아이콘을 weaponSubType 기반으로 변경 (검/지팡이/활 형태)
- EquipmentScreen: 슬롯탭 장비 클릭 → 패시브 능력 표시, 합성탭 장비 클릭 → 합성 정보 표시
- DebugPanel: 디버그 장비 생성 시 무기에 랜덤 subType 배정
- EquipmentDataTable: WEAPON_SUB_TYPE_LABELS(검/지팡이/활) 추가

### 합성 시스템 간소화 + 합성 UI 아이콘화
- upgradeCount(승급 시스템) 완전 제거: Equipment 엔티티, Forge, SaveSerializer, EquipmentScreen, 테스트
- 에픽/전설 합성 수량 2개 → 3개로 통일 (모든 등급 3개 합성)
- 합성 탭 아이콘 다이어그램: 보유 장비=실선 테두리 불투명, 부족=점선 테두리 반투명
- 합성 결과/패시브 변화 미리보기 유지

### 장비 강화 레벨 슬롯 귀속
- 강화 레벨이 장비가 아닌 **슬롯에 귀속**되도록 변경
- EquipmentSlot에 `slotLevels[]`, `slotPromoteCounts[]` 배열 추가
- `equip()`: 새 장비에 슬롯 레벨 적용, 교체된 장비는 현재 레벨 유지한 채 보관함으로
- `unequip()`: 해제된 장비 현재 레벨 유지한 채 보관함으로, 슬롯 레벨 유지
- `syncLevel()`: 강화 후 슬롯 레벨 동기화
- `initFromEquipped()`: 기존 세이브 호환 (slotLevels 없으면 장착 장비에서 추출)
- SaveSerializer: slotLevels/slotPromoteCounts 직렬화/역직렬화 + 하위호환
- EquipmentScreen: 강화 성공 후 `slot.syncLevel(index)` 호출

### 지팡이 패시브 MAGIC_BOOST로 변경
- 지팡이 패시브를 AOE_DAMAGE → MAGIC_BOOST(마법 스킬 계수 증가)로 변경
- EffectType.MAGIC_BOOST enum 추가
- PassiveSkill StatModifierEffect에 'MAGIC_COEFFICIENT' stat 타입 추가
- BattleUnit.applyPassiveSkill에서 MAGIC_COEFFICIENT 처리 (base 0.5에 가산)
- BattleManager.passiveToPassiveSkill에 MAGIC_BOOST 변환 추가

### 7일 출석체크 이벤트 시스템 (K-4)
- AttendanceDataTable: 7일 보상 데이터 테이블 (보석/에픽펫/장비뽑기/골드/장비석/펫알+사료/보석200)
- AttendanceSystem: 출석 상태 관리 (checkedDays, cycleStartDate, lastCheckDate, canCheckIn, resetCycle)
- GameManager: claimAttendance() 메서드 (RESOURCE/PET/EQUIPMENT_GACHA 보상 분기 처리), checkDailyReset에 사이클 리셋 연동
- SaveSerializer: attendance 상태 직렬화/역직렬화 + 기존 세이브 하위호환
- EventScreen: 출석체크 탭 UI (4열 그리드, 수령됨/수령가능/잠금 상태, 보상 결과 표시)
- NavBar: CalendarDays 아이콘으로 이벤트 탭 추가
- App.tsx: event 라우트 추가

### 디버그 패널 출석 Day+1
- DebugPanel에 "출석 Day+1" 버튼 추가 (lastCheckDate 초기화로 canCheckIn 활성화)
- 7일 완료 시 자동 resetCycle() 호출

### 펫 형상(PetIcon SVG) + 특수능력
- PetIcon SVG 컴포넌트: 18종 펫별 고유 형태, 티어별 색상 (S=금, A=보라, B=초록)
- PetTable에 ability 필드 추가: 펫별 고유 전투 패시브 (방어막/흡혈/공격력/방어력/연타/재생/반격/부활/치명타)
- 등급별 능력 스케일: COMMON=1x → IMMORTAL=5x (gradeScale 가산)
- BattleManager.getPetAbilitySkill(): 활성 펫 능력을 PassiveSkill로 변환
- ChapterScreen 4곳 + ContentScreen 2곳 + BattleManager.createPlayerUnit: 펫 능력 전투 적용
- PetScreen: PetIcon + 특수능력 설명 표시, 출전 중 상태 표시
