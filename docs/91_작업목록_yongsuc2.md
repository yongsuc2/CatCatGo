# yongsuc2 작업 목록

## 작업 분배 원칙
- 난이도 높은 작업: 저장/불러오기, 밸런스 설계, 아키텍처 변경, 시스템 통합
- 공유 파일 수정 시 kkwndud에게 알림

---

## 작업 목록

| ID | 작업 | 난이도 | 상태 | 상세 |
|----|------|--------|------|------|
| Y-1 | 저장/불러오기 통합 | 상 | 대기 | SaveManager → GameManager 연결, Player 전체 상태 직렬화/역직렬화, 자동 저장 |
| Y-2 | 설정 화면 | 중 | 대기 | SettingsScreen: 저장/불러오기/삭제 버튼, App.tsx 라우팅 |
| Y-3 | 방치 보상 통합 | 중 | 대기 | OfflineRewardCalculator를 앱 시작 시 실행, 보상 팝업 |
| Y-4 | 밸런스 시뮬레이터 | 상 | 대기 | 전투 N회 시뮬레이션, 경제 N일 시뮬레이션 → 통계 출력 |
| Y-5 | 챕터 난이도 곡선 | 상 | 대기 | 챕터별 클리어율 목표 설정, 적 스케일링 공식 재조정 |
| Y-6 | 스킬 선택 UI 개선 | 중 | 대기 | 챕터 내 스킬 획득 시 카드 형태 선택지 (등급/시너지 표시) |
| Y-7 | 퀘스트 진행도 연동 확장 | 중 | 대기 | 미연결된 액션들의 퀘스트 자동 추적 보완 |
| Y-8 | 통합 테스트 | 중 | 대기 | 챕터 풀 진행, 일일 루틴 시나리오 테스트 코드 |
| Y-9 | 스킬 시스템 전면 재설계 | 상 | 완료 | 4단계 계층, CompoundTrigger, 물리/마법/고정 공격 타입, INJECT_EFFECT, 패시브 분리 |
| Y-10 | 밸런스 조정 (패시브/인카운터/전투) | 중 | 완료 | 패시브 병합, 반격 변경, 인카운터 확률/보상, 전투 골드, 몬스터 스케일링 |
| Y-11 | 스킬 비주얼 이펙트 | 중 | 완료 | 투사체 애니메이션, 크리티컬 강화 표시, 데미지 팝업 스킬 아이콘 |
| Y-12 | 상태 효과 아이콘 + DoT 중첩 규칙 | 중 | 완료 | 캐릭터 하단 버프/디버프 아이콘, 같은 스킬 DoT 덮어쓰기, 다른 스킬 DoT 공존 |
| Y-13 | 뽑기 시스템 통합 | 중 | 완료 | 브론즈/실버/골드 → 단일 장비 상자, 합성 비율 기반 확률, 천장 신화 확정 |
| Y-14 | 데이터 테이블 JSON 분리 | 상 | 완료 | 15개 데이터 테이블을 JSON 파일로 분리, TS 래퍼 패턴, 소비자 코드 변경 없음 |
| Y-15 | 전투 연출/밸런스 개선 | 중 | 완료 | 상태효과 틱 애니메이션 분리, 엘리트/보스 dayProgress 적용 + 챕터별 고정 배치, 전투 연출 순서도 기획서, 챕터 획득 골드 UI |
| Y-16 | PROFICIENCY 삭제 + 밸런스 하향 + 회복 시스템 % 전환 | 중 | 완료 | PROFICIENCY 카테고리/스킬 전체 삭제, lifesteal/multi_hit/crit/shrink 밸런스 하향, REGEN/HEAL_HP 고정값→최대체력 비례(%) 전환, 분노 즉시 발동 |
| Y-17 | 속성 마스터리 + 전투 공식 정리 | 중 | 완료 | SKILL_MODIFIER 전투 적용 구현, 번개/수리검/광창 마스터리 3종, 크리티컬 물리 전용, 분산 제거, 죽은 코드 삭제 |
| Y-18 | 전투 밸런스 대규모 조정 | 중 | 완료 | 연타 일반 공격 전용+UPPER 연계, 뇌우 삭제, 소환 횟수 1/2/3/4, 몬스터 스케일링 곡선 개선(1.25/0.3), 몬스터 컨셉 리밸런스, 골드 보상 UI 버그 수정 |
| Y-19 | 전투 연출 개선 | 중 | 완료 | 스킬 투사체 타이밍 수정, 같은 종류 스킬 연출 그룹화(reorderBySkillType), 기획서 동기화 |
| Y-20 | 로비 상세 스탯 팝업 | 중 | 완료 | Player.getStatsBreakdown()/getCombatPassives() 추가, 출처별 기본 스탯 분해 + 전투 스탯 + 장비 패시브 팝업 UI |
| Y-21 | 전투 시스템 확장 (패시브/AoE/방어공식/세션HP) | 상 | 완료 | hp_fortify·magic_mastery 패시브, 검기 AoE, 방어 퍼센트 감소 공식, 세션 maxHp 연동, 연속 연출 속도 조정, 전투 시작 함수 통합 |
| Y-22 | 스킬 특징(traits) 필드 추가 | 하 | 완료 | ActiveSkillFamilyDef·PassiveSkillFamilyDef에 traits 필드, 기획/개발 참고용(UI 미표시), 04_스킬시스템.md 문서화 |
| Y-23 | 인카운터 UI 세션 스탯 반영 | 하 | 완료 | PlayerStatsBar에 세션 패시브 스킬 STAT_MODIFIER 버프 반영 (effectiveAtk/effectiveDef 계산) |
| Y-24 | 40·50일차 확률적 엘리트 전투 | 하 | 완료 | optionalEliteDays 데이터 테이블, Chapter.rollOptionalElite() 확률 판정, 30% 확률 |
| Y-25 | 재능 UI 레벨당 증가분 표시 | 하 | 완료 | TalentScreen에 레벨당 스탯 증가량(+3/+15/+2) 표시 |
| Y-26 | 스킬 교환 인카운터 | 중 | 완료 | CHANCE 하위 이벤트로 T1 스킬 교환, T1 보유 스킬 1개 → 새 T1 스킬 3개 중 선택 교환, skillIdsToRemove 메카닉 |
| Y-27 | 스킬 리롤 기능 | 하 | 완료 | 스킬 선택 인카운터에서 리롤 버튼, 세션당 2회 제한, 같은 타입 재생성 |
| Y-28 | 장비 부스탯 시스템 | 중 | 완료 | 등급별 0~5개 랜덤 부스탯, 슬롯별 후보 풀, 뽑기/합성 시 생성, 데이터 테이블 분리, UI 표시, 세이브 호환 |
| Y-29 | 공격 강화 패시브 스킬 추가 | 하 | 완료 | atk_fortify T1~T4 (6%~18%), STAT_MODIFIER ATK % |
| Y-30 | 재능 등급업 보상 시스템 | 중 | 완료 | 등급별 스탯 % 보너스 (ATK%/DEF%/HP%), Player.computeStats() 누적 적용, 등급업 팝업 UI, StatsBreakdown 등급 보너스 항목 |
| Y-31 | 스킬 티어 스케일링 가속 곡선 | 중 | 완료 | 전 스킬 1:2.3:4:6 비율 재조정, 확률 캡 스킬 T1 역산, duration/count 별도 처리 |
| Y-32 | 분노 초과분 유지 | 하 | 완료 | 분노 100 소모 후 초과분 버리지 않고 유지, Math.min 캡 제거 |
| Y-33 | 수리검 강타 스킬 추가 | 하 | 완료 | 매 일반 공격 시 수리검 소환, count 1→2→4→6, SHURIKEN 태그 |
| Y-34 | 방어 강화 패시브 스킬 추가 | 하 | 완료 | def_fortify T1~T4 (8%~48%), STAT_MODIFIER DEF %, KNIGHT 시너지 |
| Y-35 | 독 수리검 스킬 추가 | 하 | 완료 | 2턴마다 수리검+확률적 독 주입, INJECT_EFFECT poison_inject, SHURIKEN+POISON 태그 |
| Y-36 | 인카운터 정리: 천사 제거 + 확률 조정 | 중 | 완료 | ANGEL 타입 제거→우연 통합, T2 제한 해제, 악마 7%, 우연 53%, 하위 가중치 재조정, 중/대박 카운터 증가 비활성화 |
| Y-37 | 체력 비례 공격/방어 패시브 스킬 | 중 | 완료 | 배수진(low_hp_atk), 불굴(low_hp_def) — LOW_HP_MODIFIER 타입, getEffectiveAtk/Def 실시간 HP 비례 보정 |
| Y-38 | 최대 HP 비례 물리 추가 데미지 패시브 | 중 | 완료 | 압도(max_hp_damage) — MAX_HP_DAMAGE 타입, 물리 공격 시 maxHP×coefficient 추가 ATK, 방어 적용 |
| Y-39 | 적 HP 비례 물리 공격 + 기절 스킬 | 중 | 완료 | 분쇄(hp_crush): 적 maxHP 비례 물리 데미지, 강타(stun_strike): TRIGGER_SKILL→stun_apply, 기절(stun_apply): LOWEST SkillEffectType.STUN, STUN 상태효과 신규 |
| Y-40 | 재능 등급 골드 보상 | 하 | 완료 | gradeRewards에 goldPercent 추가, 챕터 내 전투/인카운터/클리어 골드에만 적용, 퀘스트/아레나/여행 등 미적용 |
| Y-41 | 재능 등급 보상 마일스톤 재설계 | 중 | 완료 | 등급 스탯% 보너스 → 등급업 ATK/DEF 교대 보너스 + 레벨 간격 마일스톤(골드/골드획득량%), 통합 프로그레스 바 UI, 수동 수령+소급 가능 |
| Y-42 | 재능 서브 등급(단) 시스템 | 중 | 완료 | 67개 서브 등급(30레벨/단 = 공10+방10+체10), 스탯 10 도달 시 서브 등급 진급+리셋, 마일스톤 10레벨 간격, 총 레벨 max 2010 |
| Y-43 | [Unity] 도메인 로직 포팅 | 상 | 완료 | TS→C# 전체 포팅 (67파일+22테스트), 250개 테스트 통과, 크로스 검증 완료 |
| Y-44 | [Unity] 전투 연출 시스템 | 상 | 완료 | BattleView: 접근→타격→후퇴 Coroutine/DOTween 페이즈 시퀀스, CharacterView, DamagePopup 풀, ProjectileView, 배속 1x/2x |
| Y-45 | [Unity] 전투 UI | 중 | 완료 | HP/분노/쉴드 바, 상태이상 아이콘(StatusEffectIconView), 캐릭터 스프라이트, 복수 적 렌더링 |
| Y-46 | [Unity] 챕터 화면 | 상 | 완료 | ChapterScreen: 인카운터 선택, 전투 진입/결과, 스킬 획득 카드(3택+리롤), 일차 진행, 중박/대박 카운터, 설정 오버레이(세션 스킬 목록+포기), 데미지 그래프 |
| Y-47 | [Unity] 메인/로비 화면 | 중 | 완료 | MainScreen: 플레이어 스탯 요약, 메뉴 카드 4개(모험/컨텐츠/재능/가챠), StatsDetailPopup(출처별 스탯 분해) |
| Y-48 | [Unity] 장비 화면 | 중 | 완료 | EquipmentScreen: 페이퍼 돌(7슬롯) + 인벤토리 그리드, 장착/해제, 강화(골드), 합성(Forge) 탭, 부스탯 표시, 등급별 색상 |
| Y-49 | [Unity] 재능 화면 | 중 | 완료 | TalentScreen: 3열 스탯 강화 버튼, 슬라이딩 윈도우 마일스톤 프로그레스 바, 서브 등급(단) 진행, 유산 카드 |
| Y-50 | [Unity] 펫 화면 | 중 | 완료 | PetScreen: 리소스 카드, 쇼케이스+스탯, 5열 그리드, 먹이주기/등급업/장착 |
| Y-51 | [Unity] 가챠/상자 화면 | 중 | 완료 | GachaScreen: 장비/펫/보석 상자 탭, 1회/10회 뽑기, 천장 진행도, 결과 카드 |
| Y-52 | [Unity] 컨텐츠 화면 | 중 | 완료 | ContentScreen: 던전/탑/아레나/여행/고블린/카타콤 6개 메뉴 카드, 각 서브패널 |
| Y-53 | [Unity] 퀘스트/이벤트 화면 | 중 | 완료 | QuestScreen(일일/주간 탭, 미션 진행도 바, 보상 수령) + EventScreen(출석 캘린더 그리드) |
| Y-54 | [Unity] 설정/디버그 화면 | 하 | 완료 | SettingsScreen(저장/불러오기/삭제/내보내기/가져오기) + DebugScreen(리소스/챕터/퀘스트 치트) |
| Y-55 | [Unity] 공통 컴포넌트 | 중 | 완료 | NavBar, ResourceBar, PlayerStatsBar, ProgressBar, DamageGraph, TabBar + Core(UIManager, BaseScreen/Popup, SpriteManager, PlaceholderGenerator, ColorPalette) |
| Y-56 | [Unity] 챕터 보물/출석 화면 | 하 | 완료 | ChapterTreasureScreen(챕터별 마일스톤 보상, 진행도 바) |
| Y-57 | [Unity] UI 레이아웃 품질 수정 | 중 | 완료 | 12개 화면 스크롤 콘텐츠 offset 초기화, 폰트 크기 증가(6개 화면), ResourceType 한글 라벨(NumberFormatter.FormatResourceType), MainScreen 그리드/팝업 레이아웃 수정, ProgressBarView sizeDelta 제거 |
| Y-58 | 딜그래프 분류 버그 수정 + 테스트 | 중 | 완료 | BattleLogCategorizer 유틸 추출(TS+C#), 스킬 크리티컬 분류 버그 수정(CRIT+SkillName→스킬명), HEAL 타입 힐맵 누락 수정, 14개 유닛+통합 테스트(260개 전체 통과), 딜그래프 연출 동기화(그룹별 OnTurnEntries), DamageGraphView UI(총합/비율%/라벨Bold) |
| Y-59 | 이미지 리소스 플레이스홀더 시스템 | 중 | 완료 | 14개 분홍 PNG 플레이스홀더, SpriteDatabase IconSpriteEntry 추가, SpriteManager.GetIcon() 폴백 시스템, TalentScreen emoji→Image 컴포넌트 전환, README_ICONS.txt 매니페스트 |
| Y-60 | 딜그래프 누적 수정 + 아이콘 리소스 문서 | 중 | 완료 | BattleView OnTurnEntries 턴 단위 일괄 누적으로 변경(그룹별→턴별), 모험화면_기획서.md 딜그래프 섹션 추가(6장), ICON_LIST.md 전체 리소스 아이콘 목록 문서 |
| Y-61 | UI 품질 수정 + 아이콘 자동 로드 | 중 | 완료 | SpriteManager Resources/Icons 자동 로드(Inspector 불필요), TalentScreen 아이콘 preferredWidth+null체크, ContentScreen 탭 아이콘 1글자 제한, EventScreen 출석부 셀 240x120+폰트 증가, StatsDetailPopup Mask 클리핑 |
| Y-62 | 아이콘 임포트 수정 + 마일스톤 노드 렌더링 수정 | 중 | 완료 | spriteMode Multiple→Single 이름 불일치 해결(SpriteManager _0 접미사 정규화), Editor .asmdef 생성+IconImportSettings 수정, TalentScreen _nodeContainer null 버그 수정(AddComponent→GetComponent), 스킬 아이콘 목록 문서 추가 |
| Y-63 | 장비 아이콘 자동 로드 | 하 | 완료 | SpriteManager에서 Resources/Icons/equip/ 자동 로드, equip_{slot}_{grade}→{SLOT}_{GRADE} 키 매핑, _256 변형 자동 무시, 아이콘 리소스 목록 문서 업데이트 |
| Y-64 | RectTransform null 버그 전역 수정 + 스킬 아이콘 자동 로드 | 중 | 완료 | AddComponent<RectTransform>() null 반환 버그 23개 파일 전역 수정(GetComponent ?? AddComponent 패턴), SpriteManager 스킬 아이콘 자동 로드(Resources/Icons/skill/), ChapterScreen 3곳 emoji→스프라이트 아이콘 전환 |
| Y-65 | 장비/재능 UI 개선 | 하 | 완료 | EquipmentScreen: 인벤토리/페이퍼돌/합성에 SpriteManager.GetEquipmentIcon() 적용, 탭 축소. TalentScreen: 마일스톤 노드 축소, 스탯 카드 레이아웃 재배치(LV→아이콘→보너스→이름→비용) |
| Y-66 | 장비 UI 대규모 확대 + 상세 패널 분리 | 중 | 완료 | 인벤토리 셀 2배 확대(4열), 페이퍼돌 셀 확대, 탭 바 최소화, 보관함 클릭→상세 패널(장착/판매/닫기) 분리, 기획서 동기화 |
| Y-67 | 전투 스킬 아이콘 스프라이트 전환 | 중 | 완료 | BattleLogEntry/SkillDamageResult에 SkillId 추가, DamagePopup emoji→Image 스프라이트 전환, 아이콘 페이드 아웃 애니메이션 |
| Y-68 | 플레이어 캐릭터 스프라이트 애니메이션 | 중 | 완료 | player.png 스프라이트 시트(4×3, 걷기+공격) 런타임 슬라이싱, CharacterView 프레임 애니메이션(걷기/공격 모션 전환), BattleView 플레이어에 자동 적용 |
| Y-69 | 적/스테이지 테마 변경: 판타지→실존 생명체 | 중 | 완료 | 일반몹(개미/참새/거미/다람쥐/고슴도치), 엘리트(왕사마귀/독수리), 보스(큰곰/왕뱀/코끼리), 던전보스(여왕벌/고목정령/백호), 던전명 변경, 260 테스트 통과 |
| Y-70 | 챕터 테마 시스템 (5테마×10챕터) | 상 | 완료 | 미생물→곤충→소형동물→중형동물→대형동물 5테마, 50종 적 템플릿, chapterThemes 풀/보스로테이션, EnemyTable 테마 인식 풀 선택, Chapter.ts/C# 동시 적용, 260 테스트 통과 |
| Y-71 | 분노 시스템 리팩토링 | 중 | 완료 | ragePerAttack 캐릭터 스탯화, ragePowerMultiplier→SKILL_MODIFIER(RAGE), 적 빌트인 스킬 자동 부여, processUnitTurn 통합, 하드코딩 분노 로직 제거, battle.data.json 정리, 261 테스트 통과 |

---

## 의존성

- Y-2는 Y-1 완료 후 진행 (설정 화면에서 저장/불러오기 사용)
- Y-3는 Y-1 완료 후 진행 (방치 보상 계산 후 저장 필요)
- Y-4, Y-5는 독립적으로 진행 가능
- Y-6, Y-7, Y-8은 독립적으로 진행 가능
