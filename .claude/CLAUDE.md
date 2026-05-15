# 프로젝트: Project Prism
- Unity Version: 6000.3.9f1

# 유니티(C#) 기반의 캐릭터 수집형 요소가 결합된 빛 반사 및 굴절 퍼즐 게임입니다.

## 코드 스타일
- 명명 규칙: 클래스 및 메서드는 PascalCase, 프라이빗 필드는 _camelCase, 지역 변수는 camelCase를 사용합니다.
- Unity API: GameObject.Find 또는 SendMessage 사용을 금지하며, GetComponent 호출은 Awake 또는 Start에서 캐싱하여 사용합니다.
- 직렬화: 인스펙터 노출이 필요한 필드는 [SerializeField] private를 사용하며, public 필드는 가급적 지양합니다.
- 비동기: 복잡한 연산이나 리소스 로딩 시 코루틴(Coroutine) 대신 Unitask 또는 async/await 패턴을 우선적으로 고려합니다.

## 명령어
- 테스트: Unity Test Framework 실행 (Ctrl+Shift+T)
- 빌드 파이프라인:
- ProjectWindow -> Build -> Android/iOS (수동)
- npm run build:ci: (CI 환경 구축 시) 유니티 배치 모드 빌드 실행
- 에셋 번들: Addressables -> Groups -> Build -> New Build 실행

## 아키텍처
/Assets/Scripts/Core: 전역 게임 상태 관리 (GameManager, IGameService)
/Assets/Scripts/Core/Auth: Firebase 인증 서비스 및 MVP 뷰/프레젠터
/Assets/Scripts/Core/LifetimeScope: VContainer DI 스코프 설정

/Assets/Scripts/Puzzles/Board: 빛 반사·굴절 퍼즐 보드 시스템
  - BoardManager (IBoardService 파사드)
  - BoardState (런타임 상태 데이터)
  - StageGenerator (스테이지 생성 알고리즘)
  - LightSimulator (빛 경로 시뮬레이션)
  - AstarAlgorithm/ (A* 경로 탐색)

/Assets/Scripts/Characters/Spirit: 정령 데이터, 인벤토리, 스킬 시스템
  - Skill/ (SkillData, SkillCaster, ISkillEffect)
  - Skill/Effects/ (DamageEffect, ShuffleBoardEffect, AddTimeEffect)

/Assets/Scripts/Characters/Monster: 몬스터 데이터 및 전투 시스템

/Assets/Scripts/UI: MVP/MVVM 패턴 기반의 UI 컨트롤러

/Assets/Scripts/Utility: 공통 유틸리티 (ObservableProperty, CanvasGroupExtensions 등)

/Assets/Prefabs: 재사용 가능한 퍼즐 기믹 및 캐릭터 프리팹

/Assets/Settings: Renderer Features (URP), 물리 및 태그 설정

## 씬 오브젝트 추가 워크플로우
Unity 씬(.unity) 또는 프리팹(.prefab)에 새 GameObject/컴포넌트를 직접 추가해야 하는 경우:
1. 먼저 사용자에게 추가할 오브젝트의 이름, 계층 구조, 컴포넌트, Inspector 참조 등을 확인한다.
2. 사용자 승인 후, Unity Editor 없이 Unity YAML을 직접 작성하여 씬/프리팹 파일을 수정한다.
   - fileID는 기존 씬에서 충돌하지 않는 범위를 사전에 확인한다.
   - 컴포넌트 GUID는 기존 씬/프리팹에서 동일 컴포넌트의 사용 예를 Grep으로 찾아 검증한다.
   - 부모 RectTransform의 m_Children에 새 자식 fileID를 추가하는 것을 빠뜨리지 않는다.
   - .meta 파일이 필요한 신규 에셋은 반드시 함께 생성한다.
3. Inspector에서 드래그로 연결해야 할 참조는 YAML의 SerializeField 필드에 직접 기입한다.

## 중요사항
- 에셋 관리: 대용량 캐릭터 리소스는 Addressables 시스템을 통해 동적으로 로드 및 언로드합니다.
- 버전 관리: .meta 파일 누락은 엄격히 금지하며, 대용량 바이너리는 LFS (Large File Storage)를 사용합니다.
- 데이터 구조: 캐릭터 및 퍼즐 데이터는 ScriptableObject를 기반으로 정의합니다.
- 내가 지시할 때마다 /.claude/previousWork.md 파일에 내용과 결과를 요약해서 영어로 추가로 작성해줘.
- 이전 세션의 내용을 /.claude/previousWork.md 파일에서 세션이 시작할 때 한번 읽은 후 md 파일의 내용을 한번 초기화하고 이전 작업을 이어서 진행해줘.