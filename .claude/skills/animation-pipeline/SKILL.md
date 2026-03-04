---
name: animation-pipeline
description: Use this skill when creating frame-based animations (idle/walk/attack) for CatCatGo characters. Triggered when user asks to animate a character, generate animation frames, create side-view sprites, extract video frames, or deploy animation resources. Covers the full Grok SDK automated pipeline from image generation through frame extraction and deployment.
---

# CatCatGo Animation Pipeline

xAI Grok SDK를 사용한 캐릭터 프레임 애니메이션 자동 생성 파이프라인.

## 파이프라인 개요

```
ref.jpg (컨셉 이미지) → side.jpg (사이드뷰) → idle/walk/attack.mp4 (동영상) → frame_NNNN.png (투명 프레임)
```

1. **사이드뷰 이미지 생성** — Grok Image API (ref.jpg 참조 → side.jpg)
2. **동영상 생성** — Grok Video API (side.jpg → idle/walk/attack.mp4)
3. **프레임 추출 + 배경 투명화** — extract_frames.ps1 (ffmpeg + ImageMagick floodfill)
4. **검수** — 프레임 시각 확인
5. **배포** — Chars/{id}/{type}/ 으로 프레임 배치

## 환경

| 항목 | 경로/값 |
|------|--------|
| 자동화 스크립트 | `C:\Users\yongs\CatCatGo\scripts\generate_animation.py` |
| 프레임 추출 스크립트 | `C:\Users\yongs\CatCatGo\extract_frames.ps1` |
| 리소스 경로 | `Assets\_Project\Resources\Chars\{id}\` |
| 프롬프트 소스 | `docs\리소스_제작_요청서\몬스터_스프라이트.md` |
| 스타일 가이드 | `docs\리소스_제작_요청서\공통_스타일_가이드.md` |
| API 키 환경변수 | `XAI_API_KEY` |
| Python | 3.10+ (`pip install xai-sdk`) |

## 리소스 구조

```
Chars/{id}/
├── ref.jpg           (컨셉 참고 이미지, IP-Adapter용)
├── sprite.png        (기존 정적 스프라이트, 512x512)
├── side.jpg          (사이드뷰 이미지 — Step 1 생성물)
├── idle.mp4          (idle 동영상 소스)
├── walk.mp4          (walk 동영상 소스)
├── attack.mp4        (attack 동영상 소스)
├── idle/
│   ├── frame_0001.png  (투명 배경 프레임)
│   ├── frame_0002.png
│   └── ...
├── walk/
│   └── frame_NNNN.png
└── attack/
    └── frame_NNNN.png
```

## 해상도 스펙

### 이미지 (grok-imagine-image)

| 항목 | 값 |
|------|-----|
| aspect_ratio | 3:4 |
| resolution | 1k |
| 출력 크기 | 896x1280 px |
| 비용 | $0.02/장 |

- 지원 aspect_ratio: `1:1`, `16:9`, `9:16`, `4:3`, `3:4`, `3:2`, `2:3`, `2:1`, `1:2`, `19.5:9`, `9:19.5`, `20:9`, `9:20`, `auto`
- 지원 resolution: `1k`, `2k` (최소 1k, 임의 픽셀 지정 불가)

### 동영상 (grok-imagine-video)

| 타입 | 목표 프레임 | 추출 fps | duration | aspect_ratio | resolution | 비용 |
|------|-----------|---------|----------|-------------|-----------|------|
| idle | 4 | 2 | 2초 | 9:16 | 480p | $0.10 |
| walk | 6 | 2 | 3초 | 9:16 | 480p | $0.15 |
| attack | 6 | 2 | 3초 | 9:16 | 480p | $0.15 |

- 지원 resolution: `480p` ($0.05/s), `720p` ($0.07/s)
- 캐릭터당 총 비용: 이미지 $0.02 + 동영상 $0.40 = **$0.42**

## 스크립트 사용법

```bash
python scripts/generate_animation.py <id>                   # 전체 파이프라인
python scripts/generate_animation.py <id> --step image      # 사이드뷰 이미지만
python scripts/generate_animation.py <id> --step video      # 동영상만 (side.jpg 필요)
python scripts/generate_animation.py <id> --step frames     # 프레임 추출만 (mp4 필요)
python scripts/generate_animation.py <id> --step deploy     # 프레임 배포만
python scripts/generate_animation.py --list                 # 전체 상태 확인
```

## 캐릭터 방향

| 캐릭터 | 바라보는 방향 | 이유 |
|--------|-------------|------|
| 적 | 왼쪽 | 오른쪽에서 등장, 왼쪽(플레이어)을 향함 |
| 플레이어 | 오른쪽 | 왼쪽에서 등장, 오른쪽(적)을 향함 |

3/4 사이드뷰: 완전 90도가 아닌, 카메라에 정면이 살짝 보여서 캐릭터 외형이 식별 가능한 각도.

## 프롬프트 템플릿

### 사이드뷰 이미지 생성 (적)

```
Side view of {desc}, facing left, 3/4 angle side view with features slightly visible,
chibi 2-head-tall SD proportions, cel-shaded, thick black outlines, flat solid colors,
toy-like matte appearance, minimal detail, full body, single character only,
on a solid bright green (#00FF00) background,
no text, no labels, clean illustration, masterpiece, best quality
```

ref.jpg가 있으면 image_url로 전달하여 캐릭터 컨셉 참조.

### idle 동영상

```
Animate this exact character with a gentle idle breathing animation.
Subtle rhythmic breathing - body slightly expands and contracts, small gentle bobbing.
Keep the character exactly identical to the reference image in every detail.
Maintain the solid bright green background throughout. Smooth looping animation. Side view maintained. No audio, no sound, no music.
```

### walk 동영상

```
Animate this exact character walking in place (treadmill style), facing the same direction.
Arms and legs move in a natural walking rhythm.
Keep the character exactly identical to the reference image in every detail.
Maintain the solid bright green background throughout. Smooth looping animation. Side view maintained. No audio, no sound, no music.
```

### attack 동영상

```
Animate this exact character performing a quick attack motion.
Wind up, then strike forward with force, then return to ready pose.
Keep the character exactly identical to the reference image in every detail.
Maintain the solid bright green background throughout. Side view maintained. No audio, no sound, no music.
```

## 검수 체크리스트

프레임 추출 후 Read tool로 샘플 확인 (첫/중간/마지막 프레임):

| 검사 항목 | 합격 기준 | 불합격 시 조치 |
|-----------|---------|-------------|
| 배경 투명 | 초록 잔여 없음, 완전 투명 | FuzzPercent 조정 후 재추출 |
| 캐릭터 보존 | 외곽 잘림/구멍/손상 없음 | FuzzPercent 낮추거나 동영상 재생성 |
| 초록 프린지 | 캐릭터 외곽에 초록 잔상 없음 | FuzzPercent 올리기 (30~35) |
| 프레임 일관성 | 캐릭터 외형이 프레임 간 유지 | 동영상 프롬프트 강화 후 재생성 |
| 프레임 수 | 목표 수량 달성 (idle 4, walk 6, attack 6) | fps 조정 |
| 동작 품질 | 자연스러운 모션, 텔레포트 없음 | 동영상 재생성 |
| 스타일 일치 | 치비/셀셰이딩 스타일 유지 | 이미지 프롬프트 강화 후 재시작 |

### 품질 등급

| 등급 | 기준 | 조치 |
|------|------|------|
| CRITICAL | 빈 프레임, 캐릭터 심각 손상 | 동영상 재생성 |
| BAD | 초록 프린지, 스타일 불일치 | 재추출 또는 재생성 |
| OK | 경미한 아티팩트 | 수용 가능 |
| GOOD | 모든 기준 충족 | 배포 |

## 배경색 참고

- 원본: `#00FF00` (표준 크로마키 그린)
- 동영상 인코딩 후: 색상 변화됨 (예: `#46C45C` 부근)
- extract_frames.ps1의 floodfill + fuzz 25%가 이 변화를 처리함
- 캐릭터 자체가 녹색인 경우: `#0000FF` (블루) 배경 사용 고려

## 트러블슈팅

| 문제 | 원인 | 해결 |
|------|------|------|
| 초록 프린지 | FuzzPercent 부족 | `--fuzz 30` 또는 `35`로 재추출 |
| 캐릭터 외곽 잘림 | FuzzPercent 과다 | `--fuzz 15` 또는 `20`으로 재추출 |
| 프레임 수 부족/초과 | fps 설정 불일치 | fps 조정 (2=12프레임, 3=18프레임 @6초) |
| 캐릭터 외형 변형 | 동영상 생성 시 참조 이미지 미반영 | 프롬프트에 "exactly identical" 강조 추가 |
| side.jpg 스타일 불일치 | text-to-image 한계 | ref.jpg를 image_url로 전달하여 스타일 참조 |
| API 타임아웃 | 동영상 생성 지연 | 폴링 간격/최대 대기시간 늘리기 |
| ffmpeg/magick 미설치 | PATH 미등록 | ffmpeg, ImageMagick 설치 + PATH 등록 |

## 아트 스타일 참조

상세 스타일 가이드: `docs/리소스_제작_요청서/공통_스타일_가이드.md`
캐릭터별 상세: `docs/리소스_제작_요청서/몬스터_스프라이트.md`
