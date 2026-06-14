# Digger Pro — Claude 소비자 문서

> 작성 규칙·섹션 의미는 [_guide.md](_guide.md) 참조. **소비·검증·함정 중심**(내부 구조 재서술 X).
> 마커: `[확인됨: MCP/샘플/테스트]` · `[확인됨: 소스]` · `[확인됨: 벤더문서]` · `[가정]`

- **확인 날짜**: 2026-06-14 (2차: runtime-scene 샘플 + 소스 실측). **Unity**: 6000.4.9f1. Digger 에셋 버전: TODO.
- **개조가 필요할 때 참고(벤더 문서, 내부 구조/확장)**: [Client/Assets/Digger/CLAUDE.md](../../Client/Assets/Digger/CLAUDE.md) · [Documentation/](../../Client/Assets/Digger/Documentation/)
- ⚠ 벤더 CLAUDE.md의 BrushType/ActionType 목록은 **실제 enum과 다름**(아래 3 참조). 소스 기준을 따를 것.

## 1. 오리엔테이션
- Unity 터레인에 동굴·오버행을 만드는 복셀 툴. **Pro = 런타임 편집 포함**(우리가 쓰는 핵심). [확인됨: 벤더문서]
- 위치: `Client/Assets/Digger/`, 영속 데이터 `Client/Assets/DiggerData/`. 공식: https://ofux.github.io/Digger-Documentation/
- **우리 용도(D3)**: 월드생성 코어가 만든 지형을 런타임에 카빙(동굴) + 영속화. [확인됨: 벤더문서]

## 2. 에셋 맵 / 쓸만한 리소스 위치
| 경로 | 무엇 | 쓸모 |
|---|---|---|
| `Modules/Runtime/Sources/DiggerMasterRuntime.cs` | **런타임 편집 API 본체** | ★우리가 호출 [확인됨: 소스] |
| `Modules/Runtime/Sources/` | `DiggerNavMeshRuntime`, `DiggerSystemExtensions` | 네브메시 갱신 등 [가정] |
| `Modules/Core/Sources/` | `DiggerMaster`/`DiggerSystem`, `ModificationParameters`, `ActionType`/`BrushType` | 진입점·타입 [확인됨: 소스] |
| `Demo/Runtime Scene/runtime-scene.unity` + `Demo/.../DiggerRuntimeUsageExample.cs` | **런타임 사용 예제(실측 출처)** | ★API 사용법 [확인됨: 샘플] |
| `DiggerData/Scenes/{scene}/` | 에디터 베이크 복셀 데이터 | 규약(7) [확인됨: 벤더문서] |
| `Modules/AdvancedOperations/`, `Shaders/`, `Misc/`, `Documentation/` | 스플라인·셰이더·문서 | 대부분 무시(문서만 링크) |

## 3. 우리가 호출하는 공개 API ★
인스턴스 획득(데모 패턴): `FindAnyObjectByType<DiggerMasterRuntime>()`. [확인됨: 샘플]

`DiggerMasterRuntime` (`Modules/Runtime/Sources/DiggerMasterRuntime.cs`) — 모두 [확인됨: 소스]:
- `void Modify(ModificationParameters p)` — 동기 변형.
- `void Modify(Vector3 position, BrushType brush, ActionType action, int textureIndex, float opacity, float size, float stalagmiteHeight=8, bool stalagmiteUpsideDown=false, bool opacityIsTarget=false, bool bypassDestructability=false, bool paintWhileDigging=true)`
- `Awaitable ModifyAsync(ModificationParameters p)` (+오버로드) — 비동기, 프레임 부담↓.
- `bool ModifyAsyncBuffured(ModificationParameters p)` — **버퍼링 비동기(연속 편집용)**. 데모는 마우스 누름 동안 이걸 호출. (메서드명 오타 "Buffured" 그대로)
- `void PersistAll()` / `void DeleteAllPersistedData()` / `void ClearScene()`
- `void SetupRuntimeTerrain(Terrain terrain, string guid = null)` — **런타임에 터레인을 Digger에 등록**. ★우리 통합 진입점.
- `void SetPersistenceDataPathPrefix(string pathPrefix)` / `void RefreshTerrainList()`
- field `public bool enablePersistence` — `PersistAll` 동작 전제.

`ModificationParameters` (struct) 필드: `Position`(world), `Brush`, `CustomBrush`, `Action`, `TextureIndex`, `Opacity`, `Size`, `StalagmiteUpsideDown`, `OpacityIsTarget`, `PaintWhileDigging`, `BypassDestructability`, `Callback`(`Action<ModificationResult>`). [확인됨: 샘플]
`ModificationResult`: `RemovedMatterQuantity`, `AddedMatterQuantity`, `TotalModifiedVoxels`, `AverageChangePerVoxel`. [확인됨: 샘플]
`enum ActionType { Dig, Add, Paint, PaintHoles, Reset, Smooth, BETA_Sharpen }` [확인됨: 소스]
`enum BrushType { Sphere, HalfSphere, RoundedCube, Stalagmite, Custom }` [확인됨: 소스]

## 4. 작업 레시피 표 (task → API 호출) ★
| 하고 싶은 것 | 호출 |
|---|---|
| 런타임에 구멍 파기 | `ModifyAsyncBuffured(params{ Action=Dig, Brush=Sphere, Position=worldPos, Size, Opacity })` [확인됨: 샘플] |
| 메우기/추가 | 위에서 `Action=Add` [확인됨: 소스] |
| 텍스처 페인트 | `Action=Paint`, `TextureIndex` 지정 [확인됨: 소스] |
| 변형 영속화 | `PersistAll()` (단, `enablePersistence=true`) [확인됨: 소스] |
| 영속 데이터 삭제 / 변형 초기화 | `DeleteAllPersistedData()` / `ClearScene()` [확인됨: 소스] |
| 새(생성된) 터레인 런타임 등록 | `SetupRuntimeTerrain(terrain, guid)` [확인됨: 소스] |

## 5. 최소 동작 스니펫
데모(`DiggerRuntimeUsageExample`) 발췌 — 레이캐스트 지점을 구체 브러시로 파기. [확인됨: 샘플]
```csharp
var dmr = FindAnyObjectByType<DiggerMasterRuntime>();
if (Physics.Raycast(transform.position, transform.forward, out var hit, 2000f)) {
    dmr.ModifyAsyncBuffured(new ModificationParameters {
        Position = hit.point, Brush = BrushType.Sphere, Action = ActionType.Dig,
        TextureIndex = 0, Opacity = 0.5f, Size = 4f, PaintWhileDigging = true,
        Callback = r => Debug.Log(r.TotalModifiedVoxels),
    });
}
```

## 6. 씬/프리팹 셋업 요건 ★
**런타임 디깅을 쓰려면 에디터에서 먼저 메뉴 셋업을 돌려야 한다**(안 하면 씬에 Digger 오브젝트가 없어 예제/런타임이 동작 안 함). 메뉴 `Tools → Digger`, 3개가 누적 호출됨 — [확인됨: 소스]:
| 메뉴 | 하는 일 | 비고 |
|---|---|---|
| `Setup terrains` (1) | "Digger Master"(+`DiggerMaster`, `CreateDirs`) 생성, 셰이더 임포트, 각 `Terrain`에 자식 "Digger"(+`DiggerSystem`) 추가 | 에디터 베이크 최소 |
| `Setup for runtime` (2) | **위 `Setup terrains` 먼저 실행** + "Digger Master Runtime"(+`DiggerMasterRuntime`) 생성 | **런타임 디깅 최소 요건** |
| `Setup NavMeshComponents` (3) | **위 `Setup for runtime` 먼저 실행** + `DiggerNavMeshRuntime` 추가 | 런타임 네브메시 갱신 필요 시 |

- 셋은 누적이라 **NavMesh 하나만 눌러도 3개가 다 적용**됨. 런타임 디깅만이면 `Setup for runtime`로 충분.
- **에디터 타임 디깅**(런타임 아님): `Setup terrains` 후 씬에서 "Digger Master" 선택 → 터레인 클릭으로 디깅, 브러시/불투명도/텍스처는 Digger Master 인스펙터에서 조정. (씬의 `README!` 오브젝트 인스펙터에 동일 안내) [확인됨: 벤더문서]
- 결과 셋업: "Digger Master"(`DiggerMaster`) + 각 터레인 자식 "Digger"(`DiggerSystem`) + "Digger Master Runtime"(`DiggerMasterRuntime`). 터레인엔 `Terrain` + `TerrainCollider`(레이캐스트용). [확인됨: 소스]
- 데모 예제는 `FindAnyObjectByType<DiggerMasterRuntime>()`로 참조 → **없으면 자기 비활성화**(= 셋업 누락 시 "예제가 안 돎"의 원인). [확인됨: 샘플]
- ⚠ 현재 열린 `runtime-scene`(dirty)엔 `DiggerMasterRuntime`가 없었음(검색 0건) — 셋업 전이거나 `Remove Digger from the scene`로 제거된 상태. [확인됨: MCP]
- **런타임 생성(우리 월드젠) 터레인은 메뉴를 못 쓰므로** `DiggerMasterRuntime.SetupRuntimeTerrain(terrain, guid)` API로 등록(메뉴의 런타임 등가물). [확인됨: 소스]

## 7. 데이터·좌표·단위 규약
- `Modify`의 `Position`은 **world 좌표**(데모는 `hit.point`). [확인됨: 샘플]
- 복셀 = SDF(음수 내부 / 양수 외부 / 0 표면). [확인됨: 벤더문서]
- ⚠ **영속 데이터 위치가 에디터/런타임 다름**:
  - 에디터 베이크 → `Assets/DiggerData/Scenes/{scene}/` (`.vox3`/`.vom`/`.labels`/`.ver`). [확인됨: 벤더문서]
  - 런타임 `PersistAll` → `Application.persistentDataPath/DiggerData/[pathPrefix/]` (`SetPersistenceDataPathPrefix`로 prefix 지정). [확인됨: 소스]

## 8. 런타임 vs 에디터 제약 · 함정 ★
- ⚠ **"런타임 예제가 안 돈다" 1순위 원인 = 메뉴 셋업 누락**. 씬에 `DiggerMasterRuntime`가 있어야 함 → 6 참조(`Tools → Digger → Setup for runtime`). [확인됨: 소스/사용자]
- 런타임 편집은 **Pro 전용** 모듈. [확인됨: 벤더문서]
- 연속/실시간 편집은 동기 `Modify`보다 **`ModifyAsyncBuffured`** 권장(프레임 히치 방지). [확인됨: 샘플 주석]
- `PersistAll`은 `enablePersistence`가 켜져 있어야 동작. [확인됨: 소스]
- ⚠ 벤더 CLAUDE.md의 Brush(`Cube/Cylinder`)/Action 목록은 **틀림** — 위 3의 enum이 정답. [확인됨: 소스]

## 9. 우리 프로젝트 통합 지점
- 월드생성 코어(`Bass.BW.WorldGeneration`)가 만든 `Terrain`을 런타임에 **`SetupRuntimeTerrain(terrain, guid)`** 로 등록 → `Modify*`로 동굴 카빙 → `PersistAll`로 영속화(델타 저장, D7). guid가 영속 데이터 매핑 키로 추정. [가정]
- 결정 D3(동굴 = Digger 첫생성 카빙 + 영속화)와 연결. 상세 설계: `Design/tech/world-generation.md`.
