# JSON 데이터 파일 Truncation (닫는 중괄호 누락)

## 발생일
2026-03-08

## 환경
- Unity 클라이언트 (CatCatGo)
- 관련 커밋: 6455da43 (수정), 6e37d42d (검증 테스트 추가)

## 증상
`battle.data.json`, `dungeon.data.json`, `pet.data.json` 3개 JSON 데이터 파일에서 파싱 에러가 발생할 수 있다. 파일 끝에 루트 JSON 객체의 닫는 중괄호(`}`)가 누락되어 유효하지 않은 JSON 형식이었다.

**영향 범위:**
- 전투 관련 데이터 로드 실패 가능 (`battle.data.json` - 전투 계수, 적 스탯, 일일 리셋 등)
- 던전 데이터 로드 실패 가능 (`dungeon.data.json` - 던전 설정, 일일 제한 등)
- 펫 데이터 로드 실패 가능 (`pet.data.json` - 펫 스탯, 등급 등)

각 파일은 소스 디렉토리(`Assets/_Project/Data/Json/`)와 빌드용 복사본 디렉토리(`Assets/Resources/_Project/Data/Json/`) 양쪽에 동일하게 존재하므로, 총 6개 파일이 영향을 받았다.

## 원인
JSON 데이터 파일 편집 또는 자동 생성 과정에서 파일 끝부분이 잘려(truncated) 루트 객체의 닫는 중괄호가 누락되었다. 3개 파일 모두 동일한 증상이므로 일괄 편집 시 발생한 것으로 추정된다.

## 해결 방법
6개 파일 모두에 누락된 닫는 중괄호(`}`)를 추가하여 유효한 JSON 형식으로 복원했다.

**수정 파일 목록:**

| 디렉토리 | 파일 |
|----------|------|
| `Assets/_Project/Data/Json/` | `battle.data.json` |
| `Assets/_Project/Data/Json/` | `dungeon.data.json` |
| `Assets/_Project/Data/Json/` | `pet.data.json` |
| `Assets/Resources/_Project/Data/Json/` | `battle.data.json` |
| `Assets/Resources/_Project/Data/Json/` | `dungeon.data.json` |
| `Assets/Resources/_Project/Data/Json/` | `pet.data.json` |

**후속 조치 (커밋 6e37d42d):**
JSON 데이터 검증 에디터 테스트가 추가되어, 모든 JSON 데이터 파일의 구조적 유효성을 자동 검증하도록 구현됨.

## 예방 방법
- JSON 데이터 파일 수정 후 JSON 유효성 검증(validation) 수행 필수
- 에디터 테스트(커밋 6e37d42d)를 통한 자동 검증 활용
- 소스(`Assets/_Project/Data/Json/`)와 빌드용 복사본(`Assets/Resources/_Project/Data/Json/`) 간 동기화 시 양쪽 모두 검증
- 대량 JSON 파일 편집 시 편집 도구의 파일 저장 완결성 확인

## 관련 문서
- `Assets/_Project/Data/Json/battle.data.json` (전투 밸런스 데이터)
- `Assets/_Project/Data/Json/dungeon.data.json` (던전 설정 데이터)
- `Assets/_Project/Data/Json/pet.data.json` (펫 데이터)
- `docs/planning-agent/SystemDesign/13_밸런스데이터시트.md` (데이터 테이블 색인)
