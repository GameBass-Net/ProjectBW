# Easy Build System — Claude 소비자 문서

> 작성 규칙·섹션 의미는 [_guide.md](_guide.md) 참조. **소비·검증·함정 중심**(내부 구조 재서술 X).
> 마커: `[확인됨: MCP/샘플/테스트]` · `[확인됨: 벤더문서]` · `[가정]`

- **설치 버전**: TODO(확인 필요) · **확인 날짜**: 2026-06-14 (스켈레톤)
- **벤더 로컬 문서 없음** — 공식 문서(gitbook)가 1차 출처: https://mindcodeinteractive.gitbook.io/easy-build-system
- 리포 내 위치: `Client/Assets/Mind Code Interactive/Easy Build System/`
- asmdef 루트 네임스페이스: `MindCodeInteractive.EasyBuildSystem.Runtime`. [확인됨: MCP/샘플]

## 1. 오리엔테이션
- 그리드/소켓 기반 건축 시스템. 파츠 배치·저장·터레인 위 건축 지원. [가정]
- **우리 용도**: 생성된 지형 위에 건축(추후) — `Design/assets.md`. [가정]

## 2. 에셋 맵 / 쓸만한 리소스 위치
> raw 스캔 초안. "쓸만함" 큐레이션은 2차 샘플 교차확인으로 확정.

| 경로 | 무엇 | 쓸모 |
|---|---|---|
| `Framework/Code/Runtime/Systems/Managers/` | **핵심 매니저**(`BuildingManager` 및 서브시스템: Terrain/Save/Batching/Physics/Grouping) | ★우리가 호출 [가정] |
| `Framework/Code/Runtime/Systems/Parts/` | **파츠 시스템**(`BuildingPart`, `BuildingPartRegistry`, Placement/Renderer/Condition) | ★파츠 정의·배치 [가정] |
| `Framework/Code/Runtime/Systems/Commands/` | 명령(`BuildingCommandManager`) | 배치/철거 명령 [가정] |
| `Framework/Code/Editor/` | 에디터 툴/메뉴 | 무시(런타임 아님) |
| `Framework/Resources/Building Parts/` | 빌딩 파츠 리소스 | 참고 [가정] |
| `Packages/Samples/Sample – Standalone Terrain Building Example.unity` | **터레인 건축 샘플** | ★우리 용도에 가장 근접, 셋업 실측 출처 [가정] |
| `Packages/Samples/*.unity` | 시점별/기능별 샘플 다수 | 참고 [가정] |
| `Packages/Integrations/` | FishNet/Mirror/PUN2/Playmaker/GC2 연동 | 우리 미사용(무시) |
| `Packages/Extensions/` | Survival/Interior/Buggy 등 확장 | 현재 무시 |
| `Framework/Art/`, `Common/` | 모델·셰이더·UI·서드파티 | 대부분 무시 |

## 3. 우리가 호출하는 공개 API ★
> TODO(2차): 샘플에서 실제 호출 확인 후 시그니처+경로로 채움.
- `BuildingManager` (`.../Systems/Managers/BuildingManager.cs`) — 시스템 진입점(싱글톤 추정). [가정]
- `BuildingPart` / `BuildingPartRegistry` — 파츠 정의·등록. [가정]
- `BuildingPlacementSystem` — 배치 로직. [가정]

## 4. 작업 레시피 표 (task → API 호출) ★
> TODO(2차).
| 하고 싶은 것 | 호출 |
|---|---|
| 파츠 1개 런타임 배치 | TODO [가정] |
| 파츠 철거 | TODO [가정] |
| 배치물 저장/로드 | `BuildingSaveSystem` 경유? TODO [가정] |

## 5. 최소 동작 스니펫
> TODO(2차): 터레인 건축 샘플에서 실측 검증된 최소 예제.

## 6. 씬/프리팹 셋업 요건 ★
> TODO(2차): 터레인 샘플의 컴포넌트·계층·초기화 순서 역추출.
- 씬에 `BuildingManager` + 파츠 컬렉션 필요(추정). [가정]

## 7. 데이터·좌표·단위 규약
> TODO(2차): 그리드 단위·소켓·좌표 규약. [가정]

## 8. 런타임 vs 에디터 제약 · 함정 ★
> 사용하며 누적. TODO(2차).

## 9. 우리 프로젝트 통합 지점
- 생성 지형 ↔ EBS `BuildingTerrainSystem`(터레인 위 배치)의 seam. 상세는 추후 `Design/tech/`. TODO.
