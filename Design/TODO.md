# TODO — 작업 체크리스트

> 살아있는 실행 체크리스트. 맥락은 [roadmap.md](roadmap.md), 구현 구조는 [tech/world-generation.md](tech/world-generation.md) §구현 구조.
> 현재 목표: **P1 첫 마일스톤 1+2** (생성 코어 → 단일 존, 에디터 검증).

---

## M1 — 생성 코어 `Bass.Core` (Unity 밖 독립 .NET 솔루션, 순수 C#)

> 위치 `ProjectBW/Core/` · 솔루션 `GameBassLib.sln` · 코어 `Bass.Core`(ns2.1;net10 멀티타깃) · 테스트 `Bass.Core.Test`(xUnit/net10).
> 빌드: `dotnet test`로 검증. Release/ns2.1 빌드 산출 DLL을 **수동으로** `Client/Assets/Plugins/Bass.Core.dll`에 복사·커밋. Unity는 Plugins DLL만 사용.

### T0. 솔루션 셋업 ✅
- [x] `Core/GameBassLib.sln` + `Bass.Core`(ns2.1;net10, ImplicitUsings 비활성, Nullable enable) + `Bass.Core.Test`(xUnit).
- [x] Unity 쪽 worldgen asmdef/소스/테스트 롤백(테스트 코드 Unity에서 제거).
- [x] Release/ns2.1 DLL → `Client/Assets/Plugins/Bass.Core.dll` 배치, Unity 로드 에러 0.

### T1. MT (Mersenne Twister) ✅
- [x] `Bass.Core.MersenneTwister`(std::mt19937 32비트 호환). API: `Seed(uint)`/`NextUInt`/`NextFloat01`/`NextDouble01`/`NextInt`/`NextRange`.
- [ ] 레이어별 스트림 분리 → **T4 필드 생성 시** worldSeed+레이어 상수로 시드 파생(MT 인스턴스 분리). MT측 준비 완료.
- [x] 결정성 확인: 시드 5489 10000번째=4123659995(std 적합) + 같은시드 동일 (xUnit 통과).

### T2. FastNoiseLite 이식 ✅
- [x] 벤더링: 공식 원본(Auburn/FastNoiseLite, MIT 헤더 유지) → `Bass.Core`. 전역 `FastNoiseLite` 클래스(그대로).
- [ ] 래퍼 → **T4 필드 샘플러**에서 타입/주파수/옥타브/시드 캡슐화 예정.
- [x] 결정성 확인: 같은 시드·좌표 동일 + [-1,1] 범위 + 다른시드 분기 (xUnit 통과).

### T3. 데이터 구조 / Config (ns `Bass.Core.WorldGeneration`)
- [ ] `EBiomeId` enum(초원/설산/사막/암석/Ocean — `E` 접두사 컨벤션), `BiomeWeights`, `HeightField`(w·h·float[]).
- [ ] `WorldGenConfig`(plain C#): worldSeed, zoneSize=128, 하이트맵 해상도, seaLevel, 레이어별 노이즈 파라미터.

### T4. 필드 샘플러 (전역좌표 입력)
- [ ] `Continentalness / Elevation / Temperature / Moisture` — 각 FastNoiseLite(레이어 시드), `Sample(worldX, worldZ)`. (Erosion P1 생략)

### T5. BiomeClassifier
- [ ] `BiomeAt(worldX, worldZ) → BiomeWeights`: ① 대륙도→바다/육지, ② 육지면 고도+온도+습도→연속 블렌딩.

### T6. 높이 합성
- [ ] `GenerateHeightField(rect, res)`: 베이스 고도 → 바이옴 가중 셰이핑 → P1 섬 마스크(반경 감쇠) → 최종 높이.

### T7. 검증 덤프 (엔진 없이)
- [ ] 그리드 샘플 → PNG/CSV(하이트맵 그레이스케일 + 바이옴맵 컬러). 콘솔 앱 또는 테스트로.
- [ ] 시드 바꿔가며 그럴싸한지 확인 → **"절차 생성 작동" 1차 증명.**

### T8. 테스트 (xUnit, `Bass.Core.Test`) ✅(진행 중)
- [x] `Bass.Core.Test` 생성 + MT 4 + FastNoiseLite 3 테스트, `dotnet test` 7/7 통과.
- [ ] 노이즈/`BiomeAt`/높이합성 테스트는 해당 단계에서 추가.

---

## M2 — 단일 존 (Unity 통합 레이어, 에디터 타임)
- [ ] **T2.1** Unity 통합 asmdef(`Assets/_ProjectBW/Scripts/...`), **`Bass.Core.dll`(Plugins) + Digger 참조**. (asmdef/ns명 M2에서 확정)
- [ ] **T2.2** `ZoneBuilder` 에디터 메뉴: TerrainData + `SetHeights`(코어 HeightField) → Terrain GO.
- [ ] **T2.3** Terrain layers 7장 등록 + `SetAlphamaps`(바이옴 가중치 + 경사 규칙).
- [ ] **T2.4** Digger 셋업(템플릿) + 단순 동굴 1개 Modify 카빙.
- [ ] **T2.5** 검증: 지형/바이옴 텍스처/동굴 + 캐릭터 보행(Suriyun/FlyCamera).

---

## 이후 (P1 나머지 / P2)
- P1: 스트리밍(정적 전체 로드) → 해수면(SW2) → 캐릭터 → 스폰/상호작용(나무·바위·동물). (roadmap R3~R6)
- P2: 동적 스트리밍 + 세이브/로드, 규모 확대, 인벤토리 등. (roadmap)
