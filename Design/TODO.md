# TODO — 작업 체크리스트

> 살아있는 실행 체크리스트. 맥락은 [roadmap.md](roadmap.md), 구현 구조는 [tech/world-generation.md](tech/world-generation.md) §구현 구조.
> 현재 목표: **P1 첫 마일스톤 1+2** (생성 코어 → 단일 존, 에디터 검증).

---

## M1 — 생성 코어 `BW.WorldGen.Core` (유니티 전, 순수 C#)

### T0. 어셈블리 셋업
- [ ] `Assets/_Core/WorldGen/` + `BW.WorldGen.Core.asmdef` 생성.
- [ ] asmdef 참조: `Unity.Mathematics`만 허용, **UnityEngine 참조 금지**(No Engine References).

### T1. MT (Mersenne Twister)
- [ ] 기존 C# MT 구현 가져오기(or 신규). API: `Seed(ulong)`, `NextUInt`, `NextFloat01`, `NextRange(min,max)`.
- [ ] 레이어별 스트림 분리(레이어마다 시드 오프셋).
- [ ] 결정성 확인: 같은 시드 → 같은 시퀀스.

### T2. FastNoiseLite 이식
- [ ] `FastNoiseLite.cs` 벤더링(MIT, 단일 파일, 라이선스 헤더 유지).
- [ ] 래퍼: 노이즈 타입/주파수/옥타브/시드 설정.
- [ ] 결정성 확인: 같은 시드·좌표 → 같은 값.

### T3. 데이터 구조 / Config
- [ ] `BiomeId` enum(초원/설산/사막/암석/Ocean), `BiomeWeights`, `HeightField`(w·h·float[]).
- [ ] `WorldGenConfig`(plain C#): worldSeed, zoneSize=128, 하이트맵 해상도, seaLevel, 레이어별 노이즈 파라미터.

### T4. 필드 샘플러 (전역좌표 입력)
- [ ] `Continentalness / Elevation / Temperature / Moisture` — 각 FastNoiseLite(레이어 시드), `Sample(worldX, worldZ)`. (Erosion P1 생략)

### T5. BiomeClassifier
- [ ] `BiomeAt(worldX, worldZ) → BiomeWeights`: ① 대륙도→바다/육지, ② 육지면 고도+온도+습도→연속 블렌딩.

### T6. 높이 합성
- [ ] `GenerateHeightField(rect, res)`: 베이스 고도 → 바이옴 가중 셰이핑 → P1 섬 마스크(반경 감쇠) → 최종 높이.

### T7. 검증 덤프 (엔진 없이)
- [ ] 그리드 샘플 → PNG/CSV(하이트맵 그레이스케일 + 바이옴맵 컬러).
- [ ] 시드 바꿔가며 그럴싸한지 확인 → **"절차 생성 작동" 1차 증명.**

### (선택) T8. 테스트 asmdef
- [ ] MT/노이즈 결정성·BiomeAt 경계값 단위 테스트.

---

## M2 — 단일 존 `BW.WorldGen.Unity` (에디터 타임)
- [ ] **T2.1** asmdef `BW.WorldGen.Unity`(`Assets/_ProjectBW/Scripts/WorldGen`), Core+Digger 참조.
- [ ] **T2.2** `ZoneBuilder` 에디터 메뉴: TerrainData + `SetHeights`(코어 HeightField) → Terrain GO.
- [ ] **T2.3** Terrain layers 7장 등록 + `SetAlphamaps`(바이옴 가중치 + 경사 규칙).
- [ ] **T2.4** Digger 셋업(템플릿) + 단순 동굴 1개 Modify 카빙.
- [ ] **T2.5** 검증: 지형/바이옴 텍스처/동굴 + 캐릭터 보행(Suriyun/FlyCamera).

---

## 이후 (P1 나머지 / P2)
- P1: 스트리밍(정적 전체 로드) → 해수면(SW2) → 캐릭터 → 스폰/상호작용(나무·바위·동물). (roadmap R3~R6)
- P2: 동적 스트리밍 + 세이브/로드, 규모 확대, 인벤토리 등. (roadmap)
