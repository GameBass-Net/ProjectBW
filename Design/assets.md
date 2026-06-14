# 에셋 후보 / 도입 현황

## 에셋 사용 문서 인덱스
에셋별 사용법·주의사항·Claude 작업 지침은 `Design/assets/`에 모음. 작성 규칙은 [assets/_guide.md](assets/_guide.md).

| 에셋 | 사람용 메모 | Claude 소비자 문서 | 벤더/공식 문서 |
|---|---|---|---|
| Digger Pro | [digger.md](assets/digger.md) | [digger.claude.md](assets/digger.claude.md) | [벤더 CLAUDE.md](../Client/Assets/Digger/CLAUDE.md) · [Documentation/](../Client/Assets/Digger/Documentation/) |
| Easy Build System | [easy-build-system.md](assets/easy-build-system.md) | [easy-build-system.claude.md](assets/easy-build-system.claude.md) | [gitbook](https://mindcodeinteractive.gitbook.io/easy-build-system) |

## 보유(설치됨)
- **Digger Pro**(런타임 포함) — 복셀 지형 변형/동굴. 런타임 `SetupRuntimeTerrain`/`Modify` 사용 예정.
- **Suriyun** — 툰/SD 캐릭터.
- **Easy Builder System** — 건축(추후).
- **TextMesh Pro** — UI 텍스트.

## 지형 생성 엔진
- **결정: 커스텀 순수 C# 코어(D2).** Unity 종속 생성 에셋(MapMagic2/Gaia) **미사용**. 아래는 "아이디어 소스"로만 참조.
| 후보 | 성격 | 비고 |
|---|---|---|
| MapMagic 2 | 노드 기반 절차생성+바이옴 | Unity 종속 → 포팅 불가, 참조용만 |
| Gaia Pro | 지형 생성+바이옴+물 | 에디터 굽기 지향, 무거움 |
| **FastNoiseLite (MIT)** | 순수 C# 노이즈 라이브러리 | **커스텀 코어 노이즈 후보 1순위** |
| Sebastian Lague(오픈소스) | 지형 생성+침식 기법 | 기법 참조 |

## 텍스처링 / 바이옴
- **MicroSplat** — 터레인 셰이더(텍스처 배열·단일패스로 다수 레이어). **Digger 공식 연동**(asmdef `JBooth.MicroSplat.Core`). 프로토타입 7장이라 **현재 불필요**, 바이옴/텍스처 확장 시 도입 검토.

## 물 / 바다 (NFR: 저사양·툰 → 스타일라이즈드 우선)
| 후보 | 성격 | NFR 적합 |
|---|---|---|
| **Stylized Water 2 (URP)** | 경량 스타일라이즈드, 내장 부력 | **1순위(렌더)** |
| **Dynamic Water Physics 2** | 부력/물리 전용(렌더 아님) | 본격 물리 필요 시 SW2와 조합 |
| Crest Ocean System | 최상급 리얼 바다 | 무거움 → 부적합 |
| KWS Water System | 고품질 물/수중 | 무거움 → 부적합 |
