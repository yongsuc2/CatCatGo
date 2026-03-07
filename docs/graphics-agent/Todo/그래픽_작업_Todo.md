# 그래픽 작업 Todo 리스트

최종 갱신: 2026-03-08

---

## 우선순위 1: 펫 아이콘 18종 제작

- **상태**: 미착수
- **영향**: PetScreen에서 이니셜 텍스트로 대체 중. 다른 화면(몬스터/장비/스킬)은 모두 전용 아이콘이 있어 시각적 불균형 발생
- **규격**: 64x64 PNG, RGBA, 투명 배경
- **저장 경로**: `Assets/_Project/Resources/Icons/pet/pet_{id}.png`
- **제작 요청서**: `docs/graphics-agent/리소스_제작_요청서/펫_아이콘.md`
- **작업 후 필요 사항**:
  - SpriteDatabase Inspector에서 펫 아이콘 할당
  - PetScreen 코드에서 `SpriteManager.GetPetIcon()` 호출로 전환 (dev-agent 협업 필요)

### 대상 목록

| 티어 | ID | 이름 |
|------|-----|------|
| S | elsa | 엘사 |
| S | piggy | 피기 |
| S | freya | 프레야 |
| S | slime_king | 슬라임 킹 |
| S | flash | 플래시 |
| S | unicorn | 유니콘 |
| S | ice_wind_fox | 얼음바람여우 |
| S | little_elle | 리틀 엘 |
| S | cleopatra | 클레오파트라 |
| A | purple_demon_fox | 보라귀신여우 |
| A | baby_dragon | 아기 드래곤 |
| A | monopoly | 모노폴리 |
| A | glazed_shroom | 윤기버섯 |
| A | flame_fox | 불꽃여우 |
| A | cactus_fighter | 선인장 파이터 |
| B | brown_bunny | 갈색토끼 |
| B | blue_bird | 파랑새 |
| B | green_frog | 초록개구리 |

---

## 우선순위 2: UI 공통 스프라이트 4종 제작

- **상태**: 미착수
- **영향**: 전체 UI가 단색 사각형으로 구성되어 프로토타입 수준. 둥근 모서리/그림자/테두리가 없음
- **규격**: 9-slice 가능한 PNG
- **저장 경로**: `Assets/_Project/Resources/Icons/`
- **코드 참조**: `SpriteDatabase`의 `buttonSprite`, `panelSprite`, `frameSprite`, `circleSprite` 필드
- **작업 후 필요 사항**: SpriteDatabase Inspector에서 할당하면 자동 적용 (코드 변경 불필요)

### 대상 목록

| 파일명 | 용도 | 비고 |
|--------|------|------|
| `buttonSprite.png` | 버튼 배경 | 9-slice, 둥근 모서리 |
| `panelSprite.png` | 패널 배경 | 9-slice, 반투명/그림자 |
| `frameSprite.png` | 테두리 프레임 | 9-slice |
| `circleSprite.png` | 원형 범용 | 64x64 |

---

## 우선순위 3: player/wolf sprite.png 생성

- **상태**: 미착수
- **영향**: 낮음. 프레임 애니메이션으로 전투 화면은 정상 동작. 단, 다른 시스템(프로필, 도감 등)에서 단일 스프라이트 참조 시 null 반환 가능
- **규격**: 512x512 PNG, RGBA, 투명 배경
- **저장 경로**: `Assets/_Project/Resources/Chars/{id}/sprite.png`
- **방법**: idle 애니메이션의 첫 프레임을 512x512로 리사이즈, 또는 ComfyUI로 재생성

### 대상 목록

| ID | 캐릭터 | 현재 상태 |
|----|--------|----------|
| `player` | 플레이어 고양이 | idle/walk/attack 프레임만 존재 |
| `wolf` | 늑대 | idle/walk/attack 프레임만 존재 |

---

## 우선순위 4: 가챠 상자 아이콘 검토

- **상태**: 확인 필요
- **영향**: 가챠 시스템(06_가챠시스템)이 추가되었으나, 상자 3종(일반/고급/전설)에 대한 아이콘이 별도로 존재하지 않음
- **현재 동작**: `GachaRewardPopup`에서 재화/장비 아이콘을 직접 참조하여 표시. 상자 자체 아이콘은 사용하지 않는 것으로 보임
- **조치**: 기획서 확인 후 상자 아이콘이 필요한지 판단. 필요하다면 제작 요청서 작성
