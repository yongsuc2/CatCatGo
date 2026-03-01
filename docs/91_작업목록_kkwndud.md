# kkwndud 작업 목록

## 작업 분배 원칙
- 기존 패턴을 따르는 UI 화면 + 게임 시스템 구현
- 비교적 쉬운 난이도의 시스템 개선/신규 구현
- 공유 파일 수정 시 yongsuc2에게 알림

---

## 작업 목록

| ID | 작업 | 난이도 | 상태 | 상세 | 참고 패턴 |
|----|------|--------|------|------|----------|
| K-1 | 전승 화면 + 연동 | 중 | 대기 | HeritageScreen: 4개 계승 루트 탭, 레벨/패시브 표시, 강화. NavBar/App.tsx 추가 | TalentScreen 참고 |
| K-2 | 장비 합성 화면 + 연동 | 중 | 대기 | ForgeScreen: 인벤토리에서 재료 선택, 미리보기, 합성 실행. GameManager 메서드 추가 | EquipmentScreen 보관함 탭 참고 |
| K-3 | 소장품 도감 시스템 | 하 | 대기 | CollectionScreen: 도감 목록, 보너스 표시, 수령. Collection 도메인 이미 구현됨 | PetScreen 참고 |
| K-4 | 이벤트 화면 | 하 | 완료 | EventScreen: 7일 출석체크 이벤트 (AttendanceSystem + AttendanceDataTable) | QuestScreen 참고 |
| K-5 | 지하감옥 개선 | 중 | 대기 | CatacombDungeon 연속 전투 UI. 기존 전투 로직 활용하여 5연전+보스 구현 | ChapterScreen 전투 참고 |
| K-6 | 화면 기획서 작성 | 하 | 대기 | 미작성 화면별 상세 기획서 (docs/화면기획문서/) | 모험화면_기획서.md 참고 |
| K-7 | UI 비주얼 개선 | 하 | 진행중 | CSS 디자인 토큰 + 유틸리티 클래스 + ChapterScreen 인라인→CSS 완료, 변수 치환 완료 | index.css 참고 |
| K-8 | 밸런스 데이터 시트 | 하 | 완료 | 전체 밸런스 수치 정리 문서 (docs/13_밸런스데이터시트.md) | 기존 데이터 테이블 참고 |
| K-9 | ~~장비 패시브~~ + 무기 종류 | 중 | 완료 | WeaponSubType(검/지팡이/활) 유지, **장비 패시브 전체 삭제됨** | — |
| K-10 | 합성 시스템 간소화 | 중 | 완료 | upgradeCount 제거, 전 등급 3개 합성 통일, 합성 UI 아이콘 다이어그램 | Forge, EquipmentScreen 참고 |
| K-11 | 강화 레벨 슬롯 귀속 | 중 | 완료 | 강화 레벨이 슬롯에 귀속, 장비 교체/해제 시 슬롯 레벨 유지 | EquipmentSlot, SaveSerializer 참고 |
| K-12 | 강화 비용 구간별 기울기 | 중 | 완료 | 6구간 복리 배율 시스템, upgradeCostTiers 데이터 테이블화 | EquipmentTable, equipment-constants 참고 |
| K-13 | 장비 강등 + 일괄 강화 | 중 | 완료 | 강등(100% 환불), 일괄 강화(최대치 한번에), UI 아이콘 | Equipment, EquipmentScreen 참고 |
| K-14 | 뽑기 용어 변경 | 하 | 완료 | "천장" → "신화 확정" 용어 통일 (GachaScreen + 기획서) | GachaScreen 참고 |
| K-15 | 장비 패시브 삭제 | 중 | 완료 | EquipmentPassiveTable + equipment-passive.data.json 삭제, 전투/UI/상세스탯에서 패시브 제거 | — |
| K-16 | 챕터 결과 개선 | 하 | 완료 | 패배 시 실패 일차 표시, 딜그래프 아래 적 남은 HP 별도 표시 | ChapterScreen 참고 |
| K-17 | 일일 던전 전투 시스템 재설계 | 상 | 완료 | 즉시 클리어→전투 애니메이션, 스테이지 기반 난이도, 도전/소탕, 3종 몬스터 | DailyDungeon, ContentScreen 참고 |

---

## 의존성

- K-1, K-3, K-4는 독립적으로 바로 시작 가능 (도메인 서비스 이미 존재)
- K-2는 Forge 서비스 확인 후 진행
- K-5는 전투 시스템 이해 후 진행 (K-1이나 K-3 먼저 해보면 좋음)
- K-6, K-7, K-8은 언제든 진행 가능

---

## 작업 시 참고 사항

### 새 화면 생성 패턴
```
1. src/presentation/screens/[Name]Screen.tsx 생성
2. src/App.tsx에 import + case 추가
3. src/index.css에 스타일 추가
4. (필요시) src/presentation/components/NavBar.tsx에 탭 추가
```

### 핵심 훅
```typescript
const { game, refresh, setScreen } = useGame();
```
- `game`: GameManager 인스턴스 (모든 게임 데이터 접근)
- `refresh()`: UI 갱신 (게임 상태 변경 후 반드시 호출)
- `setScreen('name')`: 화면 전환
