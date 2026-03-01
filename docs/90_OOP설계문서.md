# 카피바라고! OOP 설계 문서

## 설계 원칙

- 기술 스택에 독립적인 도메인 모델 설계
- 게임 로직과 렌더링/UI 완전 분리
- 모든 시스템은 인터페이스 기반으로 정의하여 Web/Unity 이식 용이

## 아키텍처 레이어

```
┌─────────────────────────────────────────────┐
│              Presentation Layer              │
│   (Web: HTML/Canvas/React  |  Unity: Scene)  │
├─────────────────────────────────────────────┤
│              Application Layer               │
│   GameManager, SessionManager, RoutineManager│
├─────────────────────────────────────────────┤
│               Domain Layer                   │
│   Player, Battle, Chapter, Equipment, Pet... │
├─────────────────────────────────────────────┤
│            Infrastructure Layer              │
│   Repository, EventBus, Timer, Random, Save  │
└─────────────────────────────────────────────┘
```

## 도메인 모델

### 핵심 엔티티

```
Player
├── stats: Stats { hp, maxHp, atk, def, crit }
├── talent: Talent { atkLevel, hpLevel, defLevel, grade }
├── heritage: Heritage { route, level }
├── equipmentSlots: EquipmentSlot[6]
├── activePet: Pet
├── ownedPets: Pet[]
├── inventory: Inventory
└── resources: Resources

Stats
├── hp: int
├── maxHp: int
├── atk: int
├── def: int
├── crit: float
└── computedFrom(talent, equipment, heritage, pets): Stats

Talent
├── atkLevel: int
├── hpLevel: int
├── defLevel: int
├── grade: TalentGrade (제자 ~ 영웅)
├── passives: TalentPassive[]
├── upgrade(statType, gold): Result
└── getUpgradeCost(statType): int

Heritage
├── route: HeritageRoute (해골/기사/유협/유령)
├── level: int
├── passives: HeritagePassive[]
├── upgrade(book): Result
└── getSkillMultiplier(skill): float
```

### 전투 관련

```
Battle
├── player: BattleUnit
├── enemy: BattleUnit
├── turnCount: int
├── state: BattleState (진행중/승리/패배)
├── log: BattleLog[]
├── executeTurn(): TurnResult
├── applySkillEffects(): void
└── checkResult(): BattleState

BattleUnit
├── stats: Stats
├── activeSkills: Skill[]
├── statusEffects: StatusEffect[]
├── takeDamage(amount): void
├── heal(amount): void
└── isAlive(): bool

Skill
├── id: string
├── name: string
├── grade: SkillGrade (일반/전설/신화/불멸)
├── category: SkillCategory (공격/마스터리/생존/디버프/버프)
├── heritageSynergy: HeritageRoute[]
├── effect: SkillEffect
└── triggerCondition: TriggerCondition (턴시작/공격시/피격시/항상)

SkillEffect
├── type: EffectType (데미지/힐/버프/디버프/DoT/부활)
├── value: float
├── duration: int
├── scalingStat: StatType
└── apply(source, target): void

StatusEffect
├── type: StatusEffectType
├── remainingTurns: int
├── value: float
└── tick(): void
```

### 챕터/스테이지 관련

```
Chapter
├── id: int
├── type: ChapterType (60일/30일/5일)
├── totalDays: int
├── currentDay: int
├── encounters: Encounter[]
├── boss: EnemyTemplate
├── generateEncounter(day): Encounter
└── isCompleted(): bool

Encounter
├── type: EncounterType (천사/악마/찬스/전투/상인/룰렛)
├── options: EncounterOption[]
├── resolve(choice): EncounterResult
└── isAvailable(): bool

EncounterOption
├── label: string
├── cost: Cost (HP/골드/없음)
├── reward: Reward (스킬/회복/재화/장비)
└── successRate: float

EnemyTemplate
├── baseStats: Stats
├── skills: Skill[]
├── scalingFactor: float
└── createInstance(chapterLevel): BattleUnit
```

### 장비 관련

```
Equipment
├── id: string
├── name: string
├── slot: SlotType (무기/의상/반지/장신구)
├── grade: EquipmentGrade (일반 ~ 신화)
├── isS: bool
├── level: int
├── upgradeCount: int (합성 레벨 +1~+4)
├── uniqueEffect: UniqueEffect
├── getStats(): Stats
├── upgrade(stone): Result
├── promote(powerStone): Result
└── canPromote(): bool

EquipmentSlot
├── type: SlotType
├── maxCount: int (무기1/의상1/반지2/장신구2)
├── equipped: Equipment[]
├── equip(equipment): Result
└── unequip(index): Equipment

Forge (합성 시스템)
├── merge(equipments: Equipment[]): Equipment
├── canMerge(equipments): bool
├── getMergeCost(equipments): Cost
└── getResultGrade(inputGrade): EquipmentGrade
```

### 펫 관련

```
Pet
├── id: string
├── name: string
├── tier: PetTier (S/A/B)
├── grade: PetGrade (일반 ~ 불멸)
├── level: int
├── passiveBonus: PassiveBonus
├── feed(food): void
├── upgrade(duplicate): Result
└── getGlobalBonus(): Stats

PetManager
├── ownedPets: Pet[]
├── activePet: Pet
├── setActive(pet): void
├── hatchEgg(egg): Pet
├── getTotalPassiveBonus(): Stats
└── feedPet(pet, food): void
```

### 재화 관련

```
Resources
├── gold: int
├── gems: int
├── stamina: int { current, max, regenRate }
├── challengeTokens: int
├── arenaTickets: int
├── pickaxes: int
├── heritageBooks: Map<HeritageRoute, int>
├── equipmentStones: int
├── powerStones: int
├── spend(type, amount): Result
├── add(type, amount): void
├── canAfford(type, amount): bool
└── tick(deltaTime): void (스태미나 자동 회복)
```

### 콘텐츠 관련

```
Tower
├── currentFloor: int
├── currentStage: int
├── maxFloor: int
├── challenge(player): BattleResult
├── getReward(floor, stage): Reward
└── getEnemyTemplate(floor, stage): EnemyTemplate

Dungeon
├── type: DungeonType (드래곤/아스트랄/환검)
├── dailyLimit: int
├── todayCount: int
├── isAvailable(): bool
├── enter(player): DungeonSession
└── getReward(): Reward

Arena
├── tier: ArenaTier (브론즈 ~ 마스터)
├── points: int
├── todayEntries: int
├── matchOpponents(): BattleUnit[]
├── fight(player): ArenaResult
└── updateTier(result): void

Travel
├── maxChapter: int
├── multiplier: int (3x ~ 50x)
├── run(stamina): TravelResult
└── getGoldReward(chapter, multiplier): int

GoblinMiner
├── pickaxes: int
├── oreCount: int
├── mine(): MineResult
├── canUseCart(): bool
└── useCart(): Reward
```

### 가챠 관련

```
TreasureChest
├── type: ChestType (브론즈/실버/골드/펫/보석)
├── cost: int (보석)
├── pityCount: int (천장 카운터)
├── pityThreshold: int (180)
├── pull(): PullResult
├── pull10(): PullResult[]
├── getPull10Cost(): int
└── isPityReached(): bool

LootTable
├── entries: LootEntry[]
├── guaranteedAtPity: LootEntry
└── roll(): LootEntry

LootEntry
├── item: Equipment | Pet | Resource
├── weight: float
└── grade: ItemGrade
```

### 이벤트 관련

```
GameEvent
├── id: string
├── type: EventType (수집형/미션형/한정가챠)
├── startDate: DateTime
├── endDate: DateTime
├── missions: EventMission[]
├── shop: EventShop
├── isActive(): bool
└── getProgress(): float

EventMission
├── description: string
├── target: int
├── current: int
├── reward: Reward
├── isCompleted(): bool
└── updateProgress(action): void

EventShop
├── items: EventShopItem[]
├── currency: EventCurrency
└── buy(item): Result
```

## 서비스 레이어

```
GameManager
├── player: Player
├── currentChapter: Chapter
├── contentManager: ContentManager
├── eventManager: EventManager
├── saveManager: SaveManager
├── startNewChapter(chapterId): void
├── processEncounter(choice): void
├── runDailyRoutine(): void
└── tick(deltaTime): void

ContentManager
├── tower: Tower
├── dungeons: Dungeon[]
├── arena: Arena
├── travel: Travel
├── goblinMiner: GoblinMiner
├── getAvailableContent(): Content[]
└── enterContent(type): ContentSession

BattleManager
├── createBattle(player, enemy): Battle
├── simulateBattle(battle): BattleResult
├── applyRewards(result): void
└── calculateDamage(attacker, defender): int

SkillSelector
├── getSkillPriority(skill, heritage): int
├── rankSkills(options, heritage, currentSkills): Skill[]
├── isCoreSynergySkill(skill, heritage): bool
└── isSupportSynergySkill(skill, heritage): bool

EquipmentManager
├── compareEquipment(a, b): int
├── autoEquipBest(player): void
├── findMergeCandidates(inventory): Equipment[][]
└── autoMerge(inventory): Equipment[]

ResourceAllocator
├── allocateGold(player): AllocationPlan
├── shouldSpendGems(player, purpose): bool
├── getStaminaPriority(): ContentType
└── generateDailyPlan(player): DailyPlan
```

## 이벤트 버스 (시스템 간 통신)

```
EventBus
├── subscribe(eventType, handler): void
├── publish(event): void
└── unsubscribe(eventType, handler): void

GameEvents:
├── BATTLE_START { player, enemy }
├── BATTLE_END { result, rewards }
├── CHAPTER_CLEAR { chapterId, rewards }
├── LEVEL_UP { statType, newLevel }
├── EQUIPMENT_CHANGE { slot, oldEquip, newEquip }
├── RESOURCE_CHANGE { type, oldAmount, newAmount }
├── PET_CHANGE { pet, action }
├── TALENT_UPGRADE { stat, level }
├── HERITAGE_UPGRADE { route, level }
├── ENCOUNTER_RESOLVED { type, choice, result }
├── DAILY_RESET { }
└── EVENT_START { eventId }
```

## 데이터 테이블 (밸런스 시트)

```
TalentTable
├── upgradeCost[grade][level]: int (골드)
├── statPerLevel[statType]: float
├── gradeThresholds[grade]: int (레벨)
└── passiveUnlocks[grade]: TalentPassive

EquipmentTable
├── baseStats[grade][slot]: Stats
├── upgradeMultiplier[level]: float
├── mergeCost[grade]: int
├── mergeCount[grade]: int (필요 개수)
└── sEquipmentList[]: EquipmentTemplate

SkillTable
├── allSkills[]: SkillTemplate
├── synergyMap[heritage][]: SkillId
├── tierList: Map<SkillId, Tier>
└── encounterSkillPool[chapterType][]: SkillId

EnemyTable
├── chapterEnemies[chapterId][]: EnemyTemplate
├── towerEnemies[floor][stage]: EnemyTemplate
├── dungeonEnemies[type][level]: EnemyTemplate
└── arenaOpponents[tier]: StatRange

ChapterTable
├── staminaCost: int (5)
├── encounterWeights[chapterType][encounterType]: float
├── bossScaling[chapterId]: float
└── rewards[chapterId]: RewardTable
```
