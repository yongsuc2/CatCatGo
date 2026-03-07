# Unity 컴파일에러 검증방법

## 작성일
2026-03-07

## 최종 수정일
2026-03-07

## 요약
Unity batch mode로 프로젝트를 열어 컴파일 에러를 검증하고, Editor.log에서 에러를 수집한다.
asmdef 참조 누락, 타입 멤버 불일치 등 자주 발생하는 패턴별 해결 방법을 정리한다.

## 상세 내용

### Unity.exe 경로

```
C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe
```

프로젝트 Unity 버전: `6000.3.10f1` (`ProjectSettings/ProjectVersion.txt` 참조)

### Batch Mode 컴파일 검증 명령어

```bash
"/c/Program Files/Unity/Hub/Editor/6000.3.10f1/Editor/Unity.exe" \
  -batchmode \
  -nographics \
  -projectPath "C:/Users/yongs/CatCatGo" \
  -logFile - \
  -quit
```

| 옵션 | 설명 |
|------|------|
| `-batchmode` | GUI 없이 실행 |
| `-nographics` | GPU 초기화 건너뜀 |
| `-projectPath` | 프로젝트 루트 경로 |
| `-logFile -` | 로그를 stdout으로 출력 (`-`를 생략하면 Editor.log에 기록) |
| `-quit` | 스크립트 컴파일 완료 후 종료 |

### 성공/실패 판단 기준

| 조건 | 판단 |
|------|------|
| 종료코드 `0` | 성공 (컴파일 에러 없음) |
| 종료코드 `1` 이상 | 실패 (컴파일 에러 또는 기타 에러 존재) |
| 로그에 `Compilation failed` 포함 | 컴파일 실패 확정 |
| 로그에 `compilationhadfailure: True` 포함 | 컴파일 실패 확정 |

### Editor.log 경로 및 에러 수집

**Editor.log 위치**:
```
%LOCALAPPDATA%\Unity\Editor\Editor.log
```
bash에서:
```bash
"$LOCALAPPDATA/Unity/Editor/Editor.log"
```
절대 경로: `C:\Users\yongs\AppData\Local\Unity\Editor\Editor.log`

**에러 수집 grep 패턴**:

```bash
grep -E "(error CS[0-9]+|Compilation failed|compilationhadfailure)" "$LOCALAPPDATA/Unity/Editor/Editor.log"
```

주요 에러 코드:
| 패턴 | 의미 |
|------|------|
| `error CS0234` | 네임스페이스에 타입이 없음 (참조 누락) |
| `error CS0246` | 타입을 찾을 수 없음 (using 누락 또는 asmdef 참조 누락) |
| `error CS1061` | 타입에 해당 멤버가 없음 (예: `.Length` vs `.Count`) |
| `error CS0103` | 이름이 현재 컨텍스트에 없음 |
| `error CS0117` | 타입에 해당 정적 멤버가 없음 |

### 자주 발생하는 에러 유형과 해결 패턴

#### 1. asmdef 참조 누락 (CS0234, CS0246)

**증상**: 다른 어셈블리의 타입을 사용하는 코드에서 `error CS0246: The type or namespace name 'Xxx' could not be found`

**원인**: `.asmdef` 파일의 `references` 배열에 대상 어셈블리가 빠져 있음

**해결**:
```json
{
    "references": [
        "CatCatGo.Domain",
        "com.unity.nuget.newtonsoft-json"  // 추가
    ]
}
```

#### 2. overrideReferences + precompiledReferences 누락 (CS0246)

**증상**: `overrideReferences: true`인 asmdef에서 NuGet 패키지 DLL을 사용할 때 타입을 찾지 못함

**원인**: `overrideReferences: true`이면 `precompiledReferences`에 명시적으로 DLL을 나열해야 함. `references`에 패키지를 추가하는 것만으로는 부족하다.

**해결**: `references`와 `precompiledReferences` 모두 추가
```json
{
    "overrideReferences": true,
    "references": [
        "com.unity.nuget.newtonsoft-json"
    ],
    "precompiledReferences": [
        "nunit.framework.dll",
        "Newtonsoft.Json.dll"
    ]
}
```

**주의**: `overrideReferences: false`인 어셈블리는 `precompiledReferences` 불필요 (자동 참조됨)

#### 3. List.Length vs List.Count (CS1061)

**증상**: `error CS1061: 'List<T>' does not contain a definition for 'Length'`

**원인**: 배열(`T[]`)은 `.Length`, `List<T>`는 `.Count` 사용. 배열에서 List로 타입이 변경된 경우 발생.

**해결**: `.Length` -> `.Count` 변경. `IReadOnlyList<T>`도 `.Count` 사용.

#### 4. Unity .meta 파일 누락

**증상**: Unity Editor에서 "Meta file not found" 경고 또는 GUID 충돌

**원인**: .cs 파일을 Unity 외부에서 추가하면 .meta 파일이 자동 생성되지만, 커밋 시 누락되는 경우 발생

**해결**: Unity Editor를 한 번 열어 .meta 파일을 자동 생성시킨 후 함께 커밋. 또는 기존 .meta 파일이 이미 생성되어 있으면 untracked 상태에서 추가 커밋.

#### 5. Newtonsoft.Json 참조 체인

CatCatGo 프로젝트의 asmdef 참조 구조:
```
CatCatGo.Domain       -> (Newtonsoft.Json 직접 사용: DataTable 클래스들)
CatCatGo.Infrastructure -> CatCatGo.Domain
CatCatGo.Services     -> CatCatGo.Domain, CatCatGo.Infrastructure, com.unity.nuget.newtonsoft-json
CatCatGo.Tests.Editor -> 모든 어셈블리 + nunit.framework.dll + Newtonsoft.Json.dll
```

Services에서 Newtonsoft.Json 타입을 직접 사용하거나 Domain의 JSON 로딩 코드를 호출하는 경우 `com.unity.nuget.newtonsoft-json` 참조가 필요하다.

### 컴파일 검증 워크플로우

```
1. git diff로 변경된 .cs / .asmdef 파일 확인
2. asmdef 변경 시: references, precompiledReferences 검토
3. .cs 변경 시: using 문, 타입 멤버 호출 검토
4. Unity batch mode 실행으로 전체 컴파일 검증
5. 실패 시: Editor.log에서 error CS 패턴 grep
6. 에러별 해결 패턴 적용 후 재검증
```

## 출처
- Unity.exe 경로: `C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe` (실제 파일 존재 확인)
- Editor.log 경로: `C:\Users\yongs\AppData\Local\Unity\Editor\Editor.log` (실제 파일 존재 확인, 45KB, 최종 수정 2026-03-07 09:13)
- Unity 버전: `ProjectSettings/ProjectVersion.txt` -> `6000.3.10f1`
- asmdef 에러 해결: `CatCatGo.Services.asmdef`, `CatCatGo.Tests.Editor.asmdef` 실제 수정 경험 (2026-03-07)
- List.Count 에러: `BattleManagerTests.cs` 실제 수정 경험 (2026-03-07)
