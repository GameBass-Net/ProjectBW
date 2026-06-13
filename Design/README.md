# ProjectBW 설계 문서

게임 기획 + 기술 설계의 살아있는 문서 모음. grill 토론으로 갱신함.

## 문서 지도
| 문서 | 내용 |
|---|---|
| [01-concept.md](01-concept.md) | 게임 컨셉·비전·핵심 경험·레퍼런스·아트 방향 |
| [02-requirements.md](02-requirements.md) | 기능 요구 + 비기능(NFR) + 타깃 플랫폼/서버 방침 |
| [03-content.md](03-content.md) | 컨텐츠 계획(바이옴/동굴/물/적/자원/구조물) — 대부분 TBD |
| [tech/world-generation.md](tech/world-generation.md) | 절차적 월드 생성 설계 (본체) |
| [tech/feasibility.md](tech/feasibility.md) | 기술 검증 체크리스트(트래커) |
| [tech/server-networking.md](tech/server-networking.md) | 데디서버/네트코드 (추후) |
| [assets.md](assets.md) | 에셋 후보·도입 현황 |
| [roadmap.md](roadmap.md) | 프로토타입 범위·빌드 순서·마일스톤 |

## 현재 상태
- **완성 기준 = 프로토타입** (싱글 클라, "그럴싸함 먼저, 디테일 후순위").
- 설계 초안 1차 grill 완료. 첫 마일스톤 = 생성 코어 + 단일 존(아래 결정 로그 D 참조).

## 결정 로그 (요약)
| ID | 결정 | 상세 |
|---|---|---|
| D1 | 생성 시점 = 런타임 유한 스트리밍(발하임식). 월드맵→존→존시드별 디테일, load-or-generate | world-generation |
| D2 | 생성 엔진 = 커스텀 순수 C# 코어(에셋은 아이디어만, Unity 종속 생성에셋 미사용) | world-generation |
| D3 | 동굴 = Digger 첫생성 카빙+영속화 / 도적·몬스터 소굴 규모 / 전 존+바이옴별 분기 | world-generation |
| D4 | 바이옴 = 연속(A-2) + 혼합 결정(B) + 월드맵 단계 전역 계산 / 구동=텍스처·하이트맵·동굴 | world-generation |
| D5 | 생성 파이프라인 = 러프HM→바이옴맵→바이옴보정HM→디테일 (조건 C1~C4) | world-generation |
| D6 | 물 = 단계적(해수면→호수→강), 프로토타입=전역 해수면+Stylized Water 2 | 03-content / assets |
| D7 | 영속성 = 시드 재생성+델타만 저장, 근처 존만, 러프HM은 저장(원경 LOD) | world-generation |
| D8 | 서버 = 프로토타입 보류·싱글 클라. 가드레일=생성코어 순수 C#. MT 결정성 해결 | server-networking |
| NFR | 저사양 타깃 + 툰/SD 아트 + 연산 최소 + 최초 생성 1회성 비용 허용 | 02-requirements |
