# BUG-014: CollectionDataTable.EnsureLoaded()에서 null safety 미흡

## 상태
Closed

## 심각도
Minor

## 발견일
2026-03-07

## 발견 경위
클라이언트 코드 검수 리팩토링 결과 검증 (Task #3) 중 CollectionDataTable.cs 코드 리뷰에서 발견

## 증상
`CollectionDataTable.EnsureLoaded()` 메서드에서:
1. `JsonDataLoader.LoadJObject("collection.data.json")`가 null을 반환하면 `_entries`가 null인 채로 남아, 이후 `GetAllEntries()`가 null을 반환함
2. `data["entries"]`가 null이면 NullReferenceException 발생 가능

```csharp
private static void EnsureLoaded()
{
    if (_entries != null) return;
    var data = JsonDataLoader.LoadJObject("collection.data.json");
    if (data == null) return;  // _entries 여전히 null → GetAllEntries()도 null 반환
    _entries = new List<CollectionEntryData>();
    foreach (var e in data["entries"])  // data["entries"]가 null이면 NRE
    {
        ...
    }
}
```

`Collection` 생성자가 `CollectionDataTable.GetAllEntries()`를 호출하여 foreach를 돌리므로, 반환값이 null이면 NullReferenceException 발생.

## 원인
JSON 파일 로드 실패 시 `_entries`를 빈 리스트로 초기화하지 않는 방어 코드 누락

## 영향 범위
- JSON 파일이 정상적으로 포함되어 있으면 실제로는 발생하지 않음
- JSON 파일이 누락/손상된 경우에만 발생하므로 심각도 Minor

## 관련 파일
- 코드: `Assets/_Project/Scripts/Domain/Data/CollectionDataTable.cs:19-27`
- 코드: `Assets/_Project/Scripts/Domain/Economy/Collection.cs:23`
- 데이터: `Assets/_Project/Data/Json/collection.data.json`

## 수정 이력
| 날짜 | 담당 Agent | 내용 | 커밋 |
|------|-----------|------|------|
| 2026-03-07 | qa-agent | 버그 발견 및 등록 | - |
| 2026-03-08 | dev-agent | data?["entries"] null 체크 추가, null 시 빈 리스트 초기화 | 3fc8f412 |
| 2026-03-08 | qa-agent | 수정 검증 완료 — data null, entries null 두 경우 모두 빈 리스트 반환 확인 | - |
