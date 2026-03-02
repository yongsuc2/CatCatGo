---
name: sprite-generator
description: Use this skill when generating or regenerating character/monster sprites for CatCatGo. Triggered when user asks to create characters, remake sprites, fix sprite quality, or generate new monster art. Covers the full ComfyUI-based generation pipeline.
---

# CatCatGo Sprite Generator

ComfyUI API를 사용한 캐릭터 스프라이트 생성 파이프라인.

## 환경

| 항목 | 경로 |
|------|------|
| ComfyUI 서버 | `http://127.0.0.1:8188` |
| ComfyUI 설치 | `C:\Cat\ComfyUI\` |
| Python (venv) | `C:\Cat\ComfyUI\venv\Scripts\python.exe` |
| 생성 스크립트 | `C:\Cat\ComfyUI\scripts\generate_sprite.py` |
| 출력 경로 | `C:\Users\yongs\CatCatGo\Assets\_Project\Resources\Chars\{monster_id}\` |
| ComfyUI output | `C:\Cat\ComfyUI\output\` |
| ComfyUI input | `C:\Cat\ComfyUI\input\` (base 이미지 저장) |

## 모델 스택

| 타입 | 파일 | 강도 |
|------|------|------|
| Checkpoint | `sd_xl_base_1.0.safetensors` | - |
| LoRA | `chibi_style_xl.safetensors` | 0.7 |
| LoRA | `envy_cel_shaded_xl.safetensors` | 1.0 |

## 생성 파이프라인

```
1. txt2img → base 이미지 (512x512, idle 포즈)
2. img2img (low denoise 0.15~0.22) → idle_0~3.png (idle 변형 4프레임)
3. img2img (high denoise 0.55~0.62) → attack_0~3.png (attack 변형 4프레임)
4. rembg → 각 프레임 배경 제거
5. 출력: {monster_id}/idle_0~3.png, attack_0~3.png (개별 512x512 PNG)
```

## 스크립트 사용법

```bash
# ComfyUI venv 사용 필수
PYTHON="C:\Cat\ComfyUI\venv\Scripts\python.exe"

# 단일 몬스터 생성
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py <monster_id>

# 미생성 몬스터 전부
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

## 몬스터 추가 방법

`generate_sprite.py`의 `MONSTERS` dict에 추가:

```python
"new_monster_id": {
    "desc": "a cute [설명], [색상], [특징]",
    "attack": "[공격 모션 설명]",
    "seed": [고유 시드 번호],
},
```

`get_theme_monsters()`의 해당 테마 리스트에도 추가.

## 품질 문제 해결

### 여러 캐릭터가 나올 때
- seed 변경 (±100~1000 범위에서 시도)
- desc에 "a single" 강조 추가
- 기존 NEGATIVE_PROMPT에 이미 "multiple characters" 포함됨

### 배경 제거 실패 (캐릭터까지 제거)
- 밝은 색상 캐릭터 (흰색, 연노랑)에서 발생
- desc에 색상을 더 진하게 지정하거나 대비 색상 추가
- seed 변경으로 배경과 캐릭터의 대비가 더 뚜렷한 결과 유도

### 배경 잔상 (원형 배경 등)
- rembg가 불완전 제거한 경우
- seed 변경 → 단순 배경이 나오는 결과 유도
- 필요시 수동 후처리 (Pillow로 특정 색상 범위 투명화)

### 스타일 불일치 (리얼/비치비)
- seed 변경이 가장 효과적
- desc에 "chibi, super deformed, 2-head tall" 등 명시적 추가

## 재생성 절차

```bash
# 1. 기존 출력 삭제
rm -rf "C:\Users\yongs\CatCatGo\Assets\_Project\Resources\Chars\{monster_id}"

# 2. base 이미지도 삭제 (선택)
rm "C:\Cat\ComfyUI\input\{monster_id}_base.png"

# 3. seed 변경 후 재생성
$PYTHON C:\Cat\ComfyUI\scripts\generate_sprite.py {monster_id}
```

## 프롬프트 구조

### Positive
```
{STYLE_PREFIX}{description}{STYLE_SUFFIX}[standing idle pose | attacking action pose, {attack_desc}], no text, no labels, masterpiece, best quality
```

### Negative
```
realistic, photograph, 3d render, heavy gradient shading, strong specular highlights, complex details, thin outlines, realistic proportions, text, labels, numbers, grid lines, background scenery, multiple characters, off-center, cropped, character sheet, multiple views, turnaround, sprite sheet, collage, grid, reference sheet
```

## 아트 스타일 참조

상세 스타일 가이드: `C:\copybarago\copybarago\docs\리소스_제작_요청서\공통_스타일_가이드.md`
몬스터별 상세: `C:\copybarago\copybarago\docs\리소스_제작_요청서\몬스터_스프라이트.md`

## 테마 구성

| 테마 | 내용 |
|------|------|
| 1 | 미생물 (챕터 1~10) |
| 2 | 곤충 (챕터 11~20) |
| 3 | 소형 동물 (챕터 21~30) |
| 4 | 중형 동물 (챕터 31~40) |
| 5 | 대형 동물 (챕터 41~50) |
| 6 | 던전 보스 (3종) |
