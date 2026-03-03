---
name: resource-inspector
description: Use this skill when inspecting, auditing, or validating CatCatGo character sprite resources for quality issues. Triggered when user asks to check resources, find broken sprites, audit character art quality, or verify sprite consistency. Covers visual inspection criteria and automated checks.
---

# CatCatGo Resource Inspector

캐릭터 스프라이트 리소스의 품질 검수 가이드.

## 리소스 경로

```
C:\Users\yongs\CatCatGo\Assets\_Project\Resources\Chars\
```

## 정상 포맷

```
Chars/{monster_id}/
├── sprite.png        (단일 스프라이트, 512x512 PNG, RGBA, 투명 배경)
└── ref.png/jpg (선택, IP-Adapter 참조 이미지)
```

## 레거시 포맷

| 포맷 | 구조 | 조치 |
|------|------|------|
| 개별 프레임 | `idle_0~3.png` + `walk_0~3.png` + `attack_0~3.png` | `sprite.png`로 재생성 필요 |
| 스프라이트시트 | `{id}.png` (1024x1536, 4x3 그리드) | `sprite.png`로 재생성 필요 |

Unity 코드(`SpriteManager.LoadBattleSprite`)는 레거시 포맷도 호환 로딩하지만, 새 포맷으로 전환 권장.

## 검수 체크리스트

### 1. 구조 검사 (자동화 가능)

```bash
# Python 스크립트로 검사
python -c "
from PIL import Image
import os, glob

chars = 'C:/Users/yongs/CatCatGo/Assets/_Project/Resources/Chars'

# sprite.png 누락 검출
for d in sorted(os.listdir(chars)):
    dpath = os.path.join(chars, d)
    if not os.path.isdir(dpath): continue
    sprite = os.path.join(dpath, 'sprite.png')
    if not os.path.exists(sprite):
        print(f'[MISSING] {d}/sprite.png')
    else:
        img = Image.open(sprite)
        if img.size != (512, 512):
            print(f'[SIZE] {d}/sprite.png: {img.size[0]}x{img.size[1]}')

# 레거시 스프라이트시트 검출
for f in sorted(glob.glob(os.path.join(chars, '*.png'))):
    name = os.path.basename(f)
    print(f'[LEGACY] {name} (root-level spritesheet)')
"
```

### 2. 시각 검사 (수동 — Read tool로 확인)

각 캐릭터의 `sprite.png`를 Read tool로 열어서 확인:

| 검사 항목 | 합격 기준 | 불합격 예시 |
|-----------|-----------|------------|
| **캐릭터 수** | 정확히 1마리 | 2마리 이상, 그리드/시트 |
| **투명 배경** | 배경 완전 투명 | 원형 배경 잔상, 흰색 사각형 |
| **내부 투명 없음** | 캐릭터 안쪽에 투명 구멍 없음 | 피부/옷 안에 투명 영역 |
| **아트 스타일** | 치비(2등신), 굵은 외곽선, 평면 색면 | 리얼 스타일, 3D, 세밀 묘사 |
| **캐릭터 식별** | 해당 동물로 식별 가능 | 다른 동물처럼 보임, 형태 불명 |
| **귀여움** | 큰 눈, 둥근 형태, 장난감 느낌 | 무섭게 생김, 어두운 색 위주 |
| **크기/위치** | 캔버스 중앙~하단, 적절한 크기 | 너무 작음, 잘림, 한쪽 치우침 |

## 품질 등급

| 등급 | 기준 | 조치 |
|------|------|------|
| **CRITICAL** | 여러 마리, 빈 이미지, sprite.png 누락 | 즉시 재생성 |
| **BAD** | 스타일 불일치, 배경 잔상, 귀엽지 않음 | 재생성 권장 |
| **OK** | 약간의 아티팩트, 색상 아쉬움 | 선택적 재생성 |
| **GOOD** | 모든 기준 충족 | 유지 |

## 검수 결과 보고 형식

```markdown
## 리소스 검수 결과

### CRITICAL (즉시 재생성)
| 캐릭터 | 문제 |
|--------|------|
| beetle | sprite.png 누락 |

### BAD (재생성 권장)
| 캐릭터 | 문제 |
|--------|------|
| boss_gorilla | 리얼 스타일, 치비 아님 |

### GOOD (유지)
bacteria, paramecium, virus, ...
```

## 재생성 방법

품질 문제 발견 시 sprite-generator skill 사용:

1. 기존 `sprite.png` 삭제
2. `generate_sprite.py`에서 해당 몬스터의 seed 변경
3. 재생성 실행

상세: sprite-generator skill 참조.
