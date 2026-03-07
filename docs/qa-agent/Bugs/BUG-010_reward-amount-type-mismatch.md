# BUG-010: 서버 RewardData.Amount(double)와 클라이언트 RewardResourceData.Amount(int) 타입 불일치

## 상태
Closed

## 심각도
Major

## 발견일
2026-03-07

## 발견 경위
서버-클라이언트 비즈니스 로직 연동 검증(Task #5) 수행 중, ContentService 반환 모델과 클라이언트 ServerResponseTypes.cs의 대응 모델을 비교하여 발견.

## 증상
서버 `RewardData.Amount`는 `double` 타입이고, 클라이언트 `RewardResourceData.Amount`는 `int` 타입이다. 서버가 소수점 값이 포함된 보상 금액을 보내면 클라이언트에서 소수점이 잘린다.

**서버** (`Server/src/CatCatGo.Server.Core/Services/ContentService.cs` line 308):
```csharp
public class RewardData
{
    public string Type { get; set; } = string.Empty;
    public double Amount { get; set; }
}
```

**클라이언트** (`Assets/_Project/Scripts/Network/ServerResponseTypes.cs` line 92-96):
```csharp
public class RewardResourceData
{
    public string Type;
    public int Amount;
}
```

**실제 발생 사례:**
- GoblinCartAsync: `reward.Amount = goldReward` 여기서 `goldReward = progress.HighestStage * 50.0` (double 연산)
- CatacombBattleAsync: `reward.Amount = goldReward` 여기서 `goldReward = currentFloor * 200.0`
- CatacombEndAsync: `reward.Amount = goldReward` 여기서 `goldReward = progress.HighestStage * 200.0`

## 원인
서버의 보상 모델(`RewardData`)이 `double`로 정의된 반면, 클라이언트의 대응 모델(`RewardResourceData`)이 `int`로 정의됨.

## 영향 범위
- 보상 금액 표시가 부정확할 수 있음 (소수점 절삭)
- 현재는 서버에서 정수값만 사용하므로 당장 문제가 되진 않지만, 서버에서 소수점 보상을 지급하면 클라이언트에서 금액이 잘릴 수 있음
- 또한 클래스 이름 자체가 다름: 서버 `RewardData` vs 클라이언트 `RewardResourceData` (내부에 Resources 리스트로 감싼 구조)

## 추가 발견: 서버-클라이언트 RewardData 구조 불일치
서버의 `RewardData`는 단일 보상(`Type` + `Amount`), 클라이언트의 `RewardData`는 `List<RewardResourceData> Resources` 리스트를 포함하는 래퍼 클래스. 역직렬화 시 서버가 `{"type":"GOLD","amount":1000}`을 보내면 클라이언트 `RewardData`는 이를 매핑하지 못한다.

## 관련 파일
- 코드(서버): `Server/src/CatCatGo.Server.Core/Services/ContentService.cs` (line 305-309, RewardData 클래스)
- 코드(클라이언트): `Assets/_Project/Scripts/Network/ServerResponseTypes.cs` (line 87-96, RewardData/RewardResourceData 클래스)

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | dev-server-agent | 서버 RewardData를 클라이언트 구조(래퍼+int)에 맞춰 수정 | - |
| 2026-03-07 | dev-agent | 클라이언트 확인 - 서버 수정으로 구조 일치, 추가 수정 불필요 | - |
