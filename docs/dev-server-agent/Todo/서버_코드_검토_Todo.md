# 서버 코드 검토 Todo

검토일: 2026-03-08

## 버그 수정 (우선순위: 높음)

### 1. ShopService.ConsumeAsync DB 업데이트 누락
- **파일**: `Server/src/CatCatGo.Server.Core/Services/ShopService.cs:97-108`
- **문제**: `purchase.Status = "CONSUMED"` 설정 후 `_purchaseRepo`를 통한 DB 업데이트 호출이 없음
- **영향**: Consume 호출 후 서버 응답은 성공이지만, 실제로 DB에 상태가 반영되지 않아 중복 소비 가능
- **수정 방안**: `purchase.Status = "CONSUMED"` 후 `await _purchaseRepo.UpdateAsync(purchase)` 추가 필요

### 2. BattleVerifier의 인메모리 세션 저장소가 Scoped DI와 불일치
- **파일**: `Server/src/CatCatGo.Server.Core/Services/BattleVerifier.cs:11`
- **문제**: `_activeSessions`가 인메모리 `Dictionary`이고, DI 등록이 `Scoped`라서 매 요청마다 새 인스턴스 생성 -> StartBattle 세션을 Report에서 찾을 수 없음
- **수정 방안**:
  - 방법 A: Redis/DB 기반 세션 저장소로 교체
  - 방법 B: DI를 Singleton으로 변경 + ConcurrentDictionary 사용 (단일 서버 한정)

## 보안 개선 (우선순위: 중간)

### 3. ResourceController Spend API 보안 검토
- **파일**: `Server/src/CatCatGo.Server.Api/Controllers/ResourceController.cs:31-43`
- **문제**: 클라이언트가 임의의 Type, Amount, Source로 리소스 소비를 직접 요청 가능
- **검토 필요**: 이 API가 클라이언트에 노출되어야 하는지, 아니면 서버 내부 전용인지 결정 필요
- **수정 방안**: 불필요하면 API 제거, 필요하면 허용 Type/Amount 화이트리스트 추가

### 4. Webhook 서명 검증 추가 (구현 시)
- **파일**: `Server/src/CatCatGo.Server.Api/Controllers/ShopController.cs:61-75`
- **문제**: RTDN/S2S Notification 엔드포인트에 서명 검증 없음
- **수정 방안**: Google RTDN, Apple S2S 서명 검증 로직 추가

### 5. ResourceService.SpendMultipleAsync Race Condition
- **파일**: `Server/src/CatCatGo.Server.Core/Services/ResourceService.cs:52-66`
- **문제**: 잔액 확인과 차감이 별도 루프에서 실행되어, 동시 요청 시 이중 차감 가능
- **수정 방안**: DB 트랜잭션으로 감싸거나, Optimistic Concurrency 적용

## 코드 품질 개선 (우선순위: 낮음)

### 6. GetAccountId() 중복 제거
- **영향 범위**: 14개 컨트롤러
- **수정 방안**: `ControllerBase`를 상속하는 공통 베이스 컨트롤러 `ApiControllerBase` 생성

### 7. ToActionResult<T> 중복 제거
- **영향 범위**: 9개 컨트롤러
- **수정 방안**: `ApiControllerBase`에 함께 포함

### 8. ToEquipmentDeltaData 중복 제거
- **파일**: `EquipmentService.cs:272-295` / `GachaService.cs:99-110`
- **수정 방안**: `EquipmentEntry` 모델 또는 별도 매퍼 클래스에 통합

### 9. ContentService dungeonType->rewardType 매핑 중복 제거
- **파일**: `ContentService.cs:92-98` / `ContentService.cs:135-141`
- **수정 방안**: private 메서드로 추출
