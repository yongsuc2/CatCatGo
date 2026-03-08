# CollectionDataTable NRE (BUG-014)

## 증상

`CollectionDataTable.GetAllEntries()` 호출 시 NullReferenceException 발생 가능.

## 원인

`EnsureLoaded()`에서 두 가지 경로로 NRE 발생:

1. `JsonDataLoader.LoadJObject("collection.data.json")` 실패(null 반환) 시 `_entries`를 초기화하지 않고 return. 이후 `GetAllEntries()`가 null을 반환하여 호출부에서 NRE.
2. JSON 로드 성공해도 `data["entries"]`가 null이면 foreach에서 NRE.

## 수정

```csharp
var entriesToken = data?["entries"];
if (entriesToken == null)
{
    _entries = new List<CollectionEntryData>();
    return;
}
_entries = new List<CollectionEntryData>();
foreach (var e in entriesToken) { ... }
```

- `data?["entries"]`로 null 전파 차단
- 실패 시에도 빈 리스트로 초기화하여 재시도 방지 + null 반환 방지

## 영향 범위

동일한 패턴(`data == null` 시 필드 미초기화)이 ChapterTreasureTable, DungeonDataTable, EncounterDataTable, EnemyTable 등 여러 DataTable에 존재. 현재는 CollectionDataTable만 수정.

## 커밋

3fc8f412
