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
