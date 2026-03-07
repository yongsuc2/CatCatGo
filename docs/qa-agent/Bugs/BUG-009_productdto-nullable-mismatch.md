# BUG-009: ProductDto StartAt/EndAt nullable 불일치

## 상태
Fixed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
서버-클라이언트 Shared 모델 일치 여부 검증(Task #3) 수행 중, ShopCatalogResponse의 ProductDto 필드 비교에서 발견.

## 증상
서버 `ProductDto`의 `StartAt`, `EndAt` 필드가 `long?` (nullable)인 반면, 클라이언트 `ProductDto`는 `long` (non-nullable)으로 정의되어 있다.

**서버** (`Server/src/CatCatGo.Shared/Responses/ShopCatalogResponse.cs` line 19-20):
```csharp
public long? StartAt { get; set; }
public long? EndAt { get; set; }
```

**클라이언트** (`Assets/_Project/Scripts/Shared/Responses/ShopResponses.cs` line 24-25):
```csharp
public long StartAt;
public long EndAt;
```

## 원인
서버에서 기간 한정 상품이 아닌 경우 `StartAt`/`EndAt`를 null로 보내는데, 클라이언트에서는 non-nullable `long`이므로 null이 기본값 0으로 역직렬화된다.

## 영향 범위
- 기간 한정이 아닌 상품의 StartAt/EndAt가 0(1970-01-01)으로 해석됨
- 클라이언트에서 "기간 한정 상품인지" 판별 로직이 있다면, 0인 경우를 "기간 없음"으로 처리해야 하는 추가 로직 필요
- 현재 클라이언트 코드에서 StartAt/EndAt를 사용하는 곳이 없으면 영향 없음

## 관련 파일
- 코드(서버): `Server/src/CatCatGo.Shared/Responses/ShopCatalogResponse.cs` (line 19-20)
- 코드(클라이언트): `Assets/_Project/Scripts/Shared/Responses/ShopResponses.cs` (line 24-25)

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-agent | 클라이언트 ProductDto StartAt/EndAt를 long? (nullable)로 변경 | - |
