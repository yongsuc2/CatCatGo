---
name: sprite-generator
description: Use this skill when generating or regenerating character/monster sprites for CatCatGo. Triggered when user asks to create characters, remake sprites, fix sprite quality, or generate new monster art. Covers the full ComfyUI-based generation pipeline.
user-invocable: true
disable-model-invocation: true
---

# CatCatGo Sprite Generator

ComfyUI API를 사용한 캐릭터 단일 스프라이트 생성 파이프라인.

## 환경

| 항목 | 경로 |
|------|------|
| ComfyUI 서버 | `http://127.0.0.1:8188` |
| ComfyUI 설치 | `C:\Cat\ComfyUI\` |
| Python (venv) | `C:\Cat\ComfyUI\venv\Scripts\python.exe` |
| 생성 스크립트 | `C:\Cat\ComfyUI\scripts\generate_sprite.py` |
| 출력 경로 | `C:\Users\yongs\CatCatGo\Assets\_Project\Resources\Chars\{id}\sprite.png` |
| ComfyUI output | `C:\Cat\ComfyUI\output\` |
| ComfyUI input | `C:\Cat\ComfyUI\input\` |

## 모델 스택

| 타입 | 파일 | 강도 |
|------|------|------|
| Checkpoint | `sd_xl_base_1.0.safetensors` | - |
| LoRA | `chibi_style_xl.safetensors` | 0.2 |
| LoRA | `envy_cel_shaded_xl.safetensors` | 0.5 |
| IP-Adapter | `ip-adapter_sdxl.safetensors` | 0.45 (ref 이미지 있을 때만) |
| CLIP Vision | `clip_vision_g.safetensors` | - |

## 생성 파이프라인

```
0. [선택] ref 이미지 감지 → Chars/{id}/ref.* 있으면 IP-Adapter로 컨셉 참조
1. txt2img → 512x512 idle 포즈 1장 [+ IP-Adapter if ref]
2. rembg → 배경 제거 + 내부 투명 구멍 원본 색으로 채우기
3. 출력: {id}/sprite.png (512x512 PNG, RGBA, 투명 배경)
```

## IP-Adapter 레퍼런스 이미지

캐릭터 컨셉의 일관성을 위해 참조 이미지를 사용할 수 있음.

### 사용법
1. `Chars/{id}/ref.png` (또는 `ref.jpg`, `ref.jpeg`, `ref.webp`)에 참조 이미지 배치
2. 스크립트 실행 시 자동 감지 → `ComfyUI/input/{id}_ref.{ext}`로 복사
3. txt2img 단계에 IP-Adapter weight 0.45로 적용
4. 해상도 무관 (CLIP Vision이 내부 리사이즈, 224x224 이상 권장)

### 주의사항
- ref 이미지가 없으면 기존 파이프라인 그대로 동작
- weight 0.45는 컨셉을 참조하되 프롬프트를 따르도록 하는 균형점

## 스크립트 사용법

```bash
PYTHON="C:\Cat\ComfyUI\venv\Scripts\python.exe"

# 단일 캐릭터 생성
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py <id>

# 미생성 캐릭터 전부
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py --all

# 테마별 생성
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py --theme <1-6>

# 목록 확인
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py --list
```

## 실행 전 체크리스트

1. **ComfyUI 서버 실행 확인**: `curl -s http://127.0.0.1:8188/system_stats` → 200이면 OK
2. **모델 파일 확인**: checkpoint + 2개 LoRA가 모델 폴더에 있는지
3. **venv Python 사용**: 시스템 Python에는 rembg가 없음

## 캐릭터 추가 방법

`generate_sprite.py`의 `MONSTERS` dict에 추가:

```python
"new_id": {
    "desc": "a cute [설명], [색상], [특징]",
    "seed": [고유 시드 번호],
    "extra_negative": "[선택] 추가 네거티브 키워드",
},
```

`get_theme_monsters()`의 해당 테마 리스트에도 추가.

## 품질 문제 해결

### 여러 캐릭터가 나올 때
- seed 변경 (±100~1000 범위에서 시도)
- desc에 "a single" 강조 추가

### 배경 제거 실패 (캐릭터까지 제거)
- 밝은 색상 캐릭터에서 발생
- desc에 색상을 더 진하게 지정하거나 대비 색상 추가
- seed 변경으로 배경과 캐릭터의 대비가 더 뚜렷한 결과 유도

### 스타일 불일치
- seed 변경이 가장 효과적
- desc에 "chibi, super deformed, 2-head tall" 등 명시적 추가

## 재생성 절차

```bash
# 1. 기존 sprite.png 삭제
rm "C:\Users\yongs\CatCatGo\Assets\_Project\Resources\Chars\{id}\sprite.png"

# 2. seed 변경 후 재생성
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py {id}
```

## 프롬프트 구조

### Positive
```
{STYLE_PREFIX}{description}, {STYLE_SUFFIX}no text, no labels, masterpiece, best quality
```

- STYLE_PREFIX: `chibi, cel shaded, thick black outlines, flat cel-shading, solid pure white background, centered composition, solo, `
- STYLE_SUFFIX: `2d illustration, simple design, one character only, full body, standing still, relaxed idle pose, `

### Negative
```
realistic, photograph, 3d render, heavy gradient shading, strong specular highlights, complex details, thin outlines, realistic proportions, text, labels, numbers, grid lines, background scenery, multiple characters, ..., [extra_negative]
```

## 아트 스타일 참조

상세 스타일 가이드: `docs/리소스_제작_요청서/공통_스타일_가이드.md`
캐릭터별 상세: `docs/리소스_제작_요청서/플레이어_캐릭터.md`, `docs/리소스_제작_요청서/몬스터_스프라이트.md`

## 테마 구성

| 테마 | 내용 |
|------|------|
| 1 | 미생물 (챕터 1~10) |
| 2 | 곤충 (챕터 11~20) |
| 3 | 소형 동물 (챕터 21~30) |
| 4 | 중형 동물 (챕터 31~40) |
| 5 | 대형 동물 (챕터 41~50) |
| 6 | 던전 보스 (3종) |

## 모션 (Unity Transform 애니메이션)

스프라이트 자체는 1장의 정지 이미지이며, 모션은 Unity 코드에서 처리:
- **idle**: bob (4px, 2.5Hz) + breathe (scale 97~103%, 2Hz)
- **attack**: 접근 → squash-stretch 히트 임팩트 → 후퇴
- 코드 위치: `CharacterView.cs` (`IdleBobAndBreathe`, `ApproachTo`, `HitImpact`, `RetreatTo`)
