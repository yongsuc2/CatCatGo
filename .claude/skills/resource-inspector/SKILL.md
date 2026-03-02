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

## 정상 포맷 (방식 A: 개별 프레임)

```
Chars/{monster_id}/
├── idle_0.png ~ idle_3.png     (대기 모션 4프레임)
└── attack_0.png ~ attack_3.png (공격 모션 4프레임)
```

- 각 프레임: 512x512 PNG, RGBA, 투명 배경
- 캐릭터 1마리만 표시
- 45도 측면 뷰, 오른쪽 향함

## 레거시 포맷 (방식 B: 스프라이트시트)

```
Chars/{monster_id}.png  (1024x1536, 4열x3행 그리드)
```

레거시 포맷은 개별 프레임으로 재생성 필요.

## 검수 체크리스트

### 1. 구조 검사 (자동화 가능)

```bash
# Python 스크립트로 검사
python -c "
from PIL import Image
import os, glob

chars = 'C:/Users/yongs/CatCatGo/Assets/_Project/Resources/Chars'

# 레거시 스프라이트시트 검출
for f in sorted(glob.glob(os.path.join(chars, '*.png'))):
    name = os.path.basename(f)
    img = Image.open(f)
    if img.size != (512, 512):
        print(f'[LEGACY] {name}: {img.size[0]}x{img.size[1]}')

# 개별 프레임 누락 검출
for d in sorted(os.listdir(chars)):
    dpath = os.path.join(chars, d)
    if not os.path.isdir(dpath): continue
    expected = ['idle_0','idle_1','idle_2','idle_3','attack_0','attack_1','attack_2','attack_3']
    for e in expected:
        if not os.path.exists(os.path.join(dpath, f'{e}.png')):
            print(f'[MISSING] {d}/{e}.png')
"
```

### 2. 시각 검사 (수동 — Read tool로 확인)

각 캐릭터의 `idle_0.png`를 Read tool로 열어서 확인:

| 검사 항목 | 합격 기준 | 불합격 예시 |
|-----------|-----------|------------|
| **캐릭터 수** | 정확히 1마리 | 2마리 이상, 그리드/시트 |
| **투명 배경** | 배경 완전 투명 | 원형 배경 잔상, 흰색 사각형 |
| **아트 스타일** | 치비(2등신), 굵은 외곽선, 평면 색면 | 리얼 스타일, 3D, 세밀 묘사 |
| **캐릭터 식별** | 해당 동물로 식별 가능 | 다른 동물처럼 보임, 형태 불명 |
| **귀여움** | 큰 눈, 둥근 형태, 장난감 느낌 | 무섭게 생김, 어두운 색 위주 |
| **크기/위치** | 캔버스 중앙~하단, 적절한 크기 | 너무 작음, 잘림, 한쪽 치우침 |
| **프레임 일관성** | idle 4장 + attack 4장 스타일 동일 | 프레임마다 다른 캐릭터 |

### 3. 프레임 일관성 검사

idle_0~3과 attack_0~3을 모두 열어서 확인:
- 같은 캐릭터인지
- 크기/위치가 비슷한지
- attack 프레임이 실제 공격 모션인지

## 품질 등급

| 등급 | 기준 | 조치 |
|------|------|------|
| **CRITICAL** | 여러 마리, 빈 이미지, 레거시 포맷 | 즉시 재생성 |
| **BAD** | 스타일 불일치, 배경 잔상, 귀엽지 않음 | 재생성 권장 |
| **OK** | 약간의 아티팩트, 색상 아쉬움 | 선택적 재생성 |
| **GOOD** | 모든 기준 충족 | 유지 |

## 검수 결과 보고 형식

```markdown
## 리소스 검수 결과

### CRITICAL (즉시 재생성)
| 캐릭터 | 문제 |
|--------|------|
| beetle | idle_0에 4마리 무당벌레 |

### BAD (재생성 권장)
| 캐릭터 | 문제 |
|--------|------|
| boss_gorilla | 리얼 스타일, 치비 아님 |

### GOOD (유지)
bacteria, paramecium, virus, ...
```

## 재생성 방법

품질 문제 발견 시 sprite-generator skill 사용:

1. 기존 출력 디렉토리 삭제
2. `generate_sprite.py`에서 해당 몬스터의 seed 변경
3. 재생성 실행

상세: sprite-generator skill 참조.
