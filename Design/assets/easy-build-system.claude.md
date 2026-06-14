# Easy Build System — Claude 소비자 문서

> 작성 규칙·섹션 의미는 [_guide.md](_guide.md) 참조. **소비·검증·함정 중심**(내부 구조 재서술 X).
> 마커: `[확인됨: MCP/샘플/테스트]` · `[확인됨: 소스]` · `[가정]`

- **확인 날짜**: 2026-06-14 (2차: `Sample – Standalone Terrain Building Example` + 소스 실측). **Unity**: 6000.4.9f1. EBS 버전: TODO.
- **벤더 로컬 문서 없음** — 공식(gitbook)이 1차 출처: https://mindcodeinteractive.gitbook.io/easy-build-system
- 위치: `Client/Assets/Mind Code Interactive/Easy Build System/`. asmdef 루트 NS: `MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime`. [확인됨: MCP]

## 1. 오리엔테이션
- 그리드/소켓 기반 건축. 파츠 프리팹을 배치/조정/철거/저장. **우리 용도(추후)**: 생성 지형 위 건축. [확인됨: 소스]
- 두 사용 모드: **인터랙티브**(플레이어 입력 → `BuildingController`) / **프로그램적**(`BuildingManager.Instance.PlacePart(...)` 직접). 우리 용도엔 후자가 직결. [확인됨: 소스]

## 2. 에셋 맵 / 쓸만한 리소스 위치
| 경로 | 무엇 | 쓸모 |
|---|---|---|
| `Framework/Code/Runtime/Systems/Managers/BuildingManager.cs` | **매니저(싱글톤)**: 배치/철거/조회 API + 설정·서브시스템(Terrain/Save/Grid/Physics/Batching/Grouping) | ★우리가 호출 [확인됨: 소스] |
| `.../Systems/Controllers/BuildingController.cs` | **인터랙티브 빌드 컨트롤러**(모드/입력/뷰) | 플레이어 주도 빌드 [확인됨: 소스] |
| `.../Systems/Parts/BuildingPart.cs`, `BuildingPartRegistry.cs` | 파츠 컴포넌트 / `Resources/BuildingPartRegistry` 등록소 | 파츠 정의·조회 [확인됨: 소스] |
| `Framework/Resources/Building Parts/`, `Building Menus/` | 기본 파츠·메뉴 리소스 | 참고 [확인됨: MCP] |
| `Packages/Samples/Sample – Standalone Terrain Building Example.unity` + `Packages/Samples/Shared/Code/Runtime/Example/` | **터레인 빌드 샘플 + 예제 스크립트** | ★셋업·사용 실측 [확인됨: MCP] |
| `Packages/Integrations/`(FishNet/Mirror/PUN2/…), `Packages/Extensions/`, `Framework/Art/`, `Framework/Code/Editor/` | 네트워크 연동·확장·아트·에디터 | 현재 무시 |

## 3. 우리가 호출하는 공개 API ★
**`BuildingManager`** (`Instance` 싱글톤, Awake에서 설정) — 모두 [확인됨: 소스]:
- `BuildingPart PlacePart(BuildingPart part, Vector3 position, Quaternion rotation, Vector3 scale, BuildingSocket socket = null)` — ★직접 배치.
- `void DestroyPart(BuildingPart part)` / `void DestroyAllPlacedParts(bool includePreplaced = true)`
- `BuildingPart CreatePreview(BuildingPart part)` / `void DestroyPreview(BuildingPart previewPart)`
- `void AdjustPart(part, Vector3 newPos, Quaternion newRot)` / `void UpgradePart(part, int upgradeIndex)`
- 조회: `GetPartByPrefabId(string)`, `GetPartByUniqueId(string)`, `GetPartByIndex(int)`, `GetPartsByCategory(string)`, `GetPartReferences()`, `GetPartCount()`
- 소켓: `HashSet<BuildingSocket> DetectSockets(Vector3, float radius)`, `BuildingSocket GetNearestSocket(Vector3, float radius)`
- 규칙/상태: `ConditionResult ValidateRules(part, BuildingMode)`, `GetPartsByState(BuildingPart.BuildingState)`, `GetAreaForPart(part)`
- 등록: `bool Register<T>(T)` / `Unregister<T>` / `GetRegistered<T>` where `T : IRegisterable`

**`BuildingController`** (인터랙티브) — [확인됨: 소스]: `SetMode(BuildingMode)`, `SelectPart(BuildingPart)`, `ValidAction()`, `CancelAction()`, `RotateAction(int)`, `SetView(BuildingViewType)`, `ActiveMode`. UI 훅: `OnEnterPlacementMode/OnEnterDestructionMode/OnEnterAdjustmentMode/OnExitMode/OnValidateButton/...`

`enum BuildingMode { None=0, Placement=1, Adjustment=2, Destruction=3, Upgrade=4 }` · `BuildingPart.BuildingState { None, Placed, Queue, Placement, Adjusting, Destruction }` [확인됨: 소스]
파츠 등록소: `BuildingPartRegistry.Instance` = `Resources.Load<BuildingPartRegistry>("BuildingPartRegistry")`(ScriptableObject 싱글톤). [확인됨: 소스]

**서브시스템 접근** (`BuildingManager` 프로퍼티) — [확인됨: 소스]:
- 저장: `SaveSystem.SaveBuildings()` / `LoadBuildings()` / `DeleteSave()` / `HasSaveData()` / `GetSaveMetaData()`
- 그리드: `GridSystem.GetCell(int x, int z)` / `GetCell(Vector3 pos)` / `RefreshGrid()` / `CurrentStage`
- 소켓: `DetectSockets(pos, radius)` / `GetNearestSocket(pos, radius)`, 정적 `BuildingSocket.GetSocketById(id)`

## 4. 작업 레시피 표 (task → API 호출) ★
| 하고 싶은 것 | 호출 |
|---|---|
| 프로그램적으로 파츠 1개 배치 | `BuildingManager.Instance.PlacePart(part, pos, rot, Vector3.one)` [확인됨: 소스] |
| 배치할 파츠 얻기 | `BuildingManager.Instance.GetPartByPrefabId(id)` / `GetPartByIndex(i)` [확인됨: 소스] |
| 파츠 철거 | `DestroyPart(part)` / 전체 `DestroyAllPlacedParts()` [확인됨: 소스] |
| 플레이어 주도 빌드 시작 | `BuildingController.SetMode(BuildingMode.Placement)` + `SelectPart(part)` [확인됨: 소스] |
| 미리보기(고스트) | `CreatePreview(part)` → 확정 시 `PlacePart(...)` [가정] |
| 배치물 저장 / 로드 | `BuildingManager.Instance.SaveSystem.SaveBuildings()` / `LoadBuildings()` [확인됨: 소스] |
| 인접 소켓 찾기(스냅) | `DetectSockets(pos, r)` / `GetNearestSocket(pos, r)` [확인됨: 소스] |

## 5. 씬/프리팹 셋업 요건 ★ [확인됨: MCP(샘플 씬)]
- 씬에 **"Building Manager"(`BuildingManager`) 1개** 필수(싱글톤). 샘플 설정: Physics=on / Save·Grid·Batching·Grouping=off(기본). `SocketLayer`=레이어 64(비트). 그리드 CellSize 기본 1.
- **파츠**: `BuildingPart` 컴포넌트를 가진 프리팹 → `Resources/BuildingPartRegistry`(ScriptableObject)에 등록되어 있어야 런타임 조회 가능(에디터 타임 자동 등록). [확인됨: 소스]
- 인터랙티브 빌드 시: 플레이어에 **`BuildingController`** + `BuildingView`(Orbital/FirstPerson/ThirdPerson) + 입력 + 카메라. 샘플은 `Example_OrbitCameraController` 사용. [확인됨: MCP]
- 우리(프로그램적) 용도엔 `BuildingController` 없이 `BuildingManager.Instance.PlacePart`만으로 충분(추정). [가정]

## 6. 데이터·좌표·단위 규약
- `PlacePart`의 position은 world 좌표, scale은 `Vector3`(보통 `Vector3.one`). [확인됨: 소스]
- 소켓은 전용 레이어(샘플 64)로 감지, 감지방식 `SocketDetectionType.Physics_Based`(콜라이더 기반) → 그 레이어 비워두지 말 것. [확인됨: MCP]
- 파츠 식별 = `prefabId`(레지스트리 매핑). [확인됨: 소스]
- 저장 위치 = `SaveProviderType`: `PlayerPrefs` / `LocalFileData`(프로젝트 폴더) / `LocalFilePersistent`(`Application.persistentDataPath`). Save 샘플은 **LocalFilePersistent + Automatic**(런타임 안전). [확인됨: 소스/MCP]

## 7. 런타임 vs 에디터 제약 · 함정 ★
- `BuildingManager.Instance`는 **Awake에서 설정** → 다른 Awake에서 접근 시 순서 주의. [확인됨: 소스]
- 파츠 프리팹이 **`Resources/BuildingPartRegistry`에 등록 안 돼 있으면** `GetPartByPrefabId` 등으로 못 찾음. [확인됨: 소스]
- 저장/그리드는 기본 **비활성** — 필요 시 BuildingManager 설정에서 켜야 함. [확인됨: MCP]
- `PlacePart`는 매니저 레벨 배치 — 소켓/규칙 검증(`ValidateRules`)은 호출자가 별도로(자동 아님 추정). [가정]
- 저장은 기능별 토글(아래 8) — 단순 `SaveBuildings()` 호출 전 `SaveSettings.EnableSaving=true` 필요. [확인됨: 소스]

## 8. 서브시스템 토글 & UI 카탈로그 (샘플 실측)
각 기능은 `BuildingManager`의 설정 플래그로 켜짐(샘플 씬에서 해당 플래그만 on). [확인됨: MCP] :

| 기능 | 켜는 설정 | 샘플 씬 / 실측값 |
|---|---|---|
| **그리드** 스냅 | `GridSettings.EnableGrid` | `Feature – Building Grid Example`: on, Stage 15×15, CellSize 1 |
| **저장/로드** | `SaveSettings.EnableSaving` | `Feature – Building Save Example`: on, Automatic + LocalFilePersistent |
| **소켓** 스냅 | (파츠에 `BuildingSocket` 부착) | `Feature – Building Socket Example`: 소켓 8개/파츠 2개, Physics_Based 감지 |
| **물리(중력/붕괴)** | `PhysicsSettings.EnablePhysics`(매니저) + 파츠에 `BuildingCollapseCondition` | `Feature – Building Collapse Example`. 기본 on |
| **영역(Area)** | `BuildingArea` 컴포넌트(씬 배치) | `Feature – Building Area Example`. 건축 허용 구역+규칙 |
| **배칭** | `BatchingSettings.EnableBatching` | `Feature – Building Batching Example`. 기본 off, 성능용 |
| 그룹 | `GroupingSettings.EnableGrouping` | 기본 off (미검토) |

- **소켓**: `BuildingSocket`은 파츠 프리팹에 자식으로 붙어 스냅 지점 제공. `MatchType{Reference, Category}`로 어떤 파츠가 붙을지 결정. [확인됨: 소스]
- **스냅은 별도 기능/씬이 아님** = 소켓 스냅 ∪ 그리드 스냅 + 파츠별 배치 설정 조합. 인터랙티브 미리보기 시 `BuildingPlacementSettings`(파츠별): `PreviewSnappingPositionThreshold`(0.1)·`PreviewSnappingRotationThreshold`(5)·`PreviewSnappedRotationStep`(45°)·`PreviewUseGridSnapping`. 컨트롤러 `PlacementBuildingState.SnapOnlyIfValid`(기본 true), 뷰 `BuildingView.SnapSettings`(장애물 체크). **프로그램적 `PlacePart`는 이 스냅 보정을 안 거치므로 호출자가 위치를 직접 줘야 함**(스냅 원하면 `DetectSockets`/`GetCell`로 좌표 보정). [확인됨: 소스]
- **파츠별 중력/붕괴**: 파츠 프리팹에 **`BuildingCollapseCondition`**(Condition 컴포넌트)을 붙이면 지지 상실 시 무너짐. 핵심 설정 — `RequireStablePlacement`(true), 지지 검사 bounds(0.5³)+`SupportLayerMask`, `RequireBuildingPart`/`RequireAnyColliderSupport`(무엇을 지지로 볼지), 낙하 물리 `FallPhysicsMass`(50)·`FallPhysicsTime`(5s)·`FallPhysicsDrag`. `BuildingPhysicsSystem`이 `CheckStability`로 구동(`EnablePhysics` 필요). 잔해/파괴는 `BuildingDebrisBehavior`(`DestructionTrigger{OnDestroyed, OnFallingImpact, OnStabilityLost, …}`). API: `PhysicsSystem.Pause()/Resume()/GetFallingParts()`. **이 컴포넌트가 없는 파츠는 중력 영향 없음**(프로그램적 `PlacePart`도 동일 — 프리팹에 붙어 있어야 함). [확인됨: 소스]
- **영역(Area)**: `BuildingArea`는 씬에 두는 **건축 구역** 컴포넌트 — `ShapeType{Sphere(반경 5)/Bounds}`, `InclusionMode{Partial, Full}`(파츠가 일부/완전히 안에 들어와야), 구역별 `BuildingRule` 리스트. `ValidateRules(part, mode)`/`ContainsPart(part)`. 용도 = 빌드 허용/금지 구역·플롯 지정. 우리: 거점/건축 가능 구역 제한에 쓸 여지(사용 미정). [확인됨: 소스]
- **배칭**: 배치된 파츠 메시를 묶어 **드로우콜↓**(순수 성능 최적화, 게임플레이 영향 X). `EnableBatching`(기본 off)/`AutoBatching`(on)/`BatchingDistance`(10), `BatchingSystem.BatchAllGroups()`/`UnbatchAllGroups()`. 묶인 파츠 수정하려면 먼저 unbatch. **저사양 NFR상 대규모 건축 시 도입 검토 가치**. [확인됨: 소스]
- **UI 카탈로그**: `Sample – Standalone UI Building Catalog Menu Example`. 프리팹 `Resources/Building Menus/UI_BuildingCatalogMenu.prefab`의 `BuildingCatalogMenuUI`가 **`BuildingPartRegistry`의 파츠(샘플 8개)를 카테고리/슬롯으로 자동 구성** → 슬롯 클릭 시 `BuildingController.SelectPart`. 우리 게임 UI는 이 프리팹을 재사용하거나 `BuildingCatalogMenuUI`를 참고. [확인됨: MCP]

## 9. 우리 프로젝트 통합 지점
- 생성 지형 위 건축: BuildingManager의 **TerrainSystem(`BuildingTerrainSystem`)** 이 터레인 위 배치/백업·복원 지원 → 우리 월드젠 `Terrain`과 연계. [확인됨: MCP]
- 우리 흐름(추정): 생성 지형 준비 → 파츠 프리팹을 레지스트리 등록 → `BuildingManager.Instance.PlacePart`로 구조물 배치(자동 생성) 또는 `BuildingController`로 플레이어 건축. [가정]
- 건축은 후순위 기능 — 상세 설계 시 `Design/tech/`에 별도 문서화.
