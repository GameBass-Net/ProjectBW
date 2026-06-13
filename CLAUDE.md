# ProjectBW

## 기획 / 설계 문서

기획·설계 문서는 git root의 **`Design/`** 폴더에 관심사별로 나눠 Markdown으로 보관한다.

```
Design/
├── README.md                 # 인덱스 + 결정 로그 요약
├── 01-concept.md             # 게임 컨셉/비전/아트/레퍼런스
├── 02-requirements.md        # 기능 요구 + 비기능(NFR) + 플랫폼/서버 방침
├── 03-content.md             # 컨텐츠 계획(바이옴/동굴/물/적/자원/구조물)
├── assets.md                 # 에셋 후보·도입 현황
├── roadmap.md                # 프로토타입 범위·빌드 순서·마일스톤
└── tech/                     # 기술 설계·검증
    ├── world-generation.md   # 절차적 월드 생성 설계(본체)
    ├── feasibility.md         # 기술 검증 체크리스트(트래커)
    └── server-networking.md  # 데디서버/네트코드
```

규칙:
- 설계 논의 결과는 해당 관심사 문서에 반영하고, 주요 결정은 `Design/README.md` 결정 로그에 한 줄 요약한다.
- 빈 placeholder 문서는 만들지 않는다. 내용이 생기면 그때 문서를 추가한다.
- Unity 프로젝트 본체는 `Client/` 에 있다.

## 코드 구조

- **Unity 프로젝트**: `Client/` (Unity 6000.4 / URP 17.4).
- **엔진 비종속 코어 라이브러리**: `Core/` (Unity 밖 독립 .NET 솔루션 `GameBassLib.sln`).
  - `Bass.Core` (멀티타깃 `netstandard2.1;net10.0`, 순수 C#, UnityEngine·Unity.Mathematics 의존 0) — 결정성 필요한 로직(절차 생성 등). 향후 서버 포팅 대비.
  - `Bass.Core.Test` (xUnit, net10) — `dotnet test`로 검증.
  - **Unity 연동**: Release/`netstandard2.1` 빌드 DLL을 **수동으로** `Client/Assets/Plugins/Bass.Core.dll`에 복사·커밋. Unity는 Plugins의 DLL만 사용(빌드 파이프라인에 넣지 않음).
- 작업 체크리스트는 `Design/TODO.md`, 구현 구조 상세는 `Design/tech/world-generation.md`.

## 작업 환경

- Unity가 떠 있으면 **unity-mcp**(MCP) 도구로 컴파일 에러 확인(`read_console`)·테스트(`run_tests`)·메뉴 실행 등이 가능하다. 코드 변경 후 `refresh_unity`→`read_console`로 검증할 것.
