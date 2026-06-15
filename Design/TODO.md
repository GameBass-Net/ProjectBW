# TODO — 작업 체크리스트

> 살아있는 실행 체크리스트. 맥락은 [roadmap.md](roadmap.md), 구현 구조는 [tech/world-generation.md](tech/world-generation.md) §구현 구조.
> 현재 목표: **P1 첫 마일스톤 1+2** (생성 코어 → 단일 존, 에디터 검증).

---

## M1 — 생성 코어 (Unity 밖 독립 .NET 솔루션, 순수 C#)

> 위치 `ProjectBW/Core/` · 솔루션 `GameBassLib.sln` (모두 ns2.1;net10 멀티타깃)
> - **`Bass.Core`** = 컨텐츠 비종속 범용(MT·FastNoiseLite). 테스트 `Bass.Core.Test`(xUnit).
> - **`Bass.BW`** = 컨텐츠 종속(월드생성, ns `Bass.BW.WorldGeneration`). **`Bass.Core` 참조.** 테스트 `Bass.BW.Test`(xUnit).
> 빌드: `dotnet test`로 검증. Release/ns2.1 산출 DLL(`Bass.Core.dll` + `Bass.BW.dll`)을 **수동으로** `Client/Assets/Plugins/`에 복사·커밋. Unity는 Plugins DLL만 사용.

### T0. 솔루션 셋업 ✅
- [x] `Core/GameBassLib.sln` + `Bass.Core`(ns2.1;net10, ImplicitUsings 비활성, Nullable enable) + `Bass.Core.Test`(xUnit).
- [x] Unity 쪽 worldgen asmdef/소스/테스트 롤백(테스트 코드 Unity에서 제거).
- [x] Release/ns2.1 DLL → `Client/Assets/Plugins/Bass.Core.dll` 배치, Unity 로드 에러 0.

### T1. MT (Mersenne Twister) ✅
- [x] `Bass.Core.MT19937`(std::mt19937 32비트 호환) + `Bass.Core.MT19937_64`(std::mt19937_64 64비트 호환). API: Seed/Next(U)Int·ULong/NextFloat01/NextDouble01/NextInt/NextRange(관대 동작: 동일→반환, 역전→스왑).
- [x] 레이어별 스트림 분리 → T4 `NoiseField.DeriveLayerSeed(worldSeed, layer)` = `MT19937(worldSeed+layer).NextUInt()`(순수 함수, 레이어 독립 파생).
- [x] 결정성 확인(xUnit): 32비트 10000번째=4123659995, 64비트 10000번째=9981545732273789042 (둘 다 C++ 표준 적합) + 같은시드 동일.

### T2. FastNoiseLite 이식 ✅
- [x] 벤더링: 공식 원본(Auburn/FastNoiseLite, MIT 헤더 유지) → `Bass.Core`. 전역 `FastNoiseLite` 클래스(그대로).
- [x] 래퍼 → T4 `NoiseField`(단일 래퍼) + `NoiseLayerSettings`(타입/주파수/옥타브 등 캡슐화).
- [x] 결정성 확인: 같은 시드·좌표 동일 + [-1,1] 범위 + 다른시드 분기 (xUnit 통과).

### T3. 데이터 구조 / Config (`Bass.BW`, ns `Bass.BW.WorldGeneration`) ✅
- [x] `EBiomeId`(초원/설산/사막/암석/Ocean), `BiomeWeights`(readonly struct, 고정필드+인덱서+Dominant/Sum, 무할당), `HeightField`(정규화[0,1] row-major).
- [x] `WorldGenConfig`: WorldSeed, ZoneSize=128, HeightmapResolution=129, SeaLevel, MaxHeight. (레이어 노이즈·섬마스크 파라미터는 T4~T6에서 추가)
- [x] `Bass.BW.Test` 8/8 통과(BiomeWeights 4, HeightField 4).

### T4. 필드 샘플러 (전역좌표 입력) ✅
- [x] `Continentalness / Elevation / Temperature / Moisture`(`EFieldLayer`) — 단일 래퍼 `NoiseField`(레이어 시드 파생), `Sample(worldX, worldZ)`→[0,1] 정규화. 파라미터는 `WorldGenConfig.NoiseLayers[]`(레이어당 `NoiseLayerSettings`). (Erosion P1 생략) `Bass.BW.Test` 8개 추가.

### T5. BiomeClassifier ✅
- [x] `BiomeClassifier(config).BiomeAt(worldX, worldZ) → BiomeWeights`: ① 대륙도 SmoothStep→바다/육지 연속 전이, ② 육지면 (고도·온도·습도) 기후 이상점 가우시안 거리 가중→정규화 연속 블렌딩. 튜닝값 `BiomeClassifierSettings`(이상점·해안대·sharpness). `Bass.BW.Test` 7개 추가(합=1/비음/바다·육지 분기/결정성). 수치는 시작값(만들어보고 조정).

### T6. 높이 합성 ✅
- [x] `HeightSynthesizer(config)`: `HeightAt(x,z)` / `GenerateHeightField(originX, originZ, worldSize, res)`. 베이스 고도(elevation 노이즈) → 바이옴 가중 셰이핑(접근 A, `HeightSynthesisSettings` 가감폭) → P1 섬 마스크(반경 감쇠) → clamp[0,1]. 대양 재판정은 후보정(보류). `Bass.BW.Test` 7개 추가.

### T7. 검증 덤프 (엔진 없이) ✅
- [x] `Tools/WorldGenDump`(net10 콘솔, Core 참조 + ImageSharp NuGet, `GameBassLib.sln` 밖). `dotnet run -- [출력폴더] [시드...]` → 시드별 하이트맵 그레이스케일 + 바이옴맵 컬러 PNG(512²). 코어에 이미지 의존성 없음.
- [x] 시드 1·2·3 확인 → 반경 감쇠 섬 높이맵 + 연속 바이옴 블렌딩 확인. **"절차 생성 작동" 1차 증명 완료.**
- [x] **(후보정 완료)** 대양 높이 기반 재판정 + T5 정리: `BiomeClassifier.BiomeAt`→`LandBlendAt`(육지 블렌드만), `HeightSynthesizer.BiomeAt`이 높이<SeaLevel로 대양 합성. 바이옴맵 대양↔섬 형태 일치 확인. `BlendSharpness` 8→25.

### T8. 테스트 (xUnit) ✅(진행 중)
- [x] `Bass.Core.Test`: MT19937 7 + MT19937_64 6 + FastNoiseLite 3 = 16, `dotnet test` 통과.
- [x] `Bass.BW.Test` 프로젝트 생성(→ Bass.BW 참조). 솔루션 4프로젝트 빌드 0경고.
- [ ] `BiomeAt`/높이합성 등 월드생성 테스트는 `Bass.BW.Test`에 해당 단계에서 추가.

---

## M2 — 단일 존 (Unity 통합 레이어, 에디터 타임)
- [ ] **T2.1** Unity 통합 asmdef(`Assets/_ProjectBW/Scripts/...`), **`Bass.Core.dll`(Plugins) + Digger 참조**. (asmdef/ns명 M2에서 확정)
- [ ] **T2.2** `ZoneBuilder` 에디터 메뉴: TerrainData + `SetHeights`(코어 HeightField) → Terrain GO.
- [ ] **T2.3** Terrain layers 7장 등록 + `SetAlphamaps`(바이옴 가중치 + 경사 규칙).
- [ ] **T2.4** Digger 셋업(템플릿) + 단순 동굴 1개 Modify 카빙.
- [ ] **T2.5** 검증: 지형/바이옴 텍스처/동굴 + 캐릭터 보행(Suriyun/FlyCamera).

---

## 후보정 / 튜닝 보류 (기록)
> "지금 대충, 나중에 보정" 합의 항목. 그림(T7) 본 뒤 조정.
- ~~대양 판정 모델 전환~~ ✅ 완료: 높이<SeaLevel 기반 재판정으로 통일(`HeightSynthesizer.BiomeAt`), `BiomeClassifier`는 육지 블렌드(`LandBlendAt`)만. 대륙도 레이어는 enum/Config에 보존하되 P1 분류 미사용.
- **튜닝값(전부 시작값)**: `NoiseLayerSettings`(레이어별 주파수/옥타브 등), `BiomeClassifierSettings`(이상점·sharpness·OceanCoastBand), `HeightSynthesisSettings`(바이옴 셰이핑 가감폭·섬 마스크 중심/반경/감쇠). 그림 보며 조정. **미해결: 사막이 거의 안 나옴**(온도/습도·이상점 튜닝 필요).
- **바이옴 셰이핑 A→B 재검토**: 현재 A(베이스 고도+바이옴 가중 가감). 산이 충분히 안 솟으면 B(바이옴별 목표 고도 곡선)로 전환 검토.

## 이후 (P1 나머지 / P2)
- P1: 스트리밍(정적 전체 로드) → 해수면(SW2) → 캐릭터 → 스폰/상호작용(나무·바위·동물). (roadmap R3~R6)
- P2: 동적 스트리밍 + 세이브/로드, 규모 확대, 인벤토리 등. (roadmap)
