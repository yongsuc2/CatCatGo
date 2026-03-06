# 개발일지 - dev-agent

## 2026-03-07

### 리소스 참조 검증 Editor 도구 추가

**파일**: `Assets/_Project/Scripts/Editor/ResourceValidator.cs`

Unity Editor 메뉴 `Tools/Resource Validator`에서 실행 가능한 리소스 검증 도구 개발.

**검증 항목**:
- Character Resources: enemy.data.json 기반 Chars 폴더/애니메이션 프레임 존재 및 연속성
- Status Effect Icons: 9종 상태이펙트 아이콘 존재
- Equipment Icons: 7 슬롯 x 6 등급 장비 아이콘 존재
- Skill Icons: active-skill-tier.data.json 기반 스킬 아이콘 존재
- JSON Data Files: Data/Json 원본과 Resources 복사본 존재/동기화
- Unused Character Resources: 데이터에서 참조하지 않는 Chars 폴더 탐지

**발견 사항**:
- `spider` 캐릭터 폴더가 Chars에 존재하나 enemy.data.json에 참조 없음
