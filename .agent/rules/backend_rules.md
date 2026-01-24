# 프로젝트 백엔드 설계 규칙 (UGS 기반)

이 문서는 모든 팀원이 일관된 방식으로 백엔드 기능을 구현하고 사용하기 위한 규칙을 정의합니다. 기능을 개발하기 전에 반드시 이 규칙을 숙지해야 합니다.

## 1. 서버 권한 구조 (Server-Authoritative)

핵심 게임 로직과 데이터의 진실성(Truth)은 항상 서버에 있습니다.

### 1-1. 재화 및 주요 데이터 보호
- **수정 금지**: 골드(Gold), 보석(Gem), 경험치(Exp), 레벨(Level), 아이템 획득 등 핵심 데이터는 **클라이언트에서 직접 값을 수정할 수 없습니다.**
- **요청 및 응답**: 클라이언트는 '행동(Action)'이나 '이벤트(Event)'를 서버로 전송하고, 서버가 계산한 **결과값**을 받아서 로컬 상태를 갱신해야 합니다.
  - *Bad*: `playerData.gold += 100; SaveToServer();`
  - *Good*: `BackendManager.Instance.CallCloudFunction("ClearStage", stageId);` (서버가 보상을 계산해서 반환)

### 1-2. 검증 로직
- 상점 구매, 스테이지 클리어, 보상 획득 등은 반드시 Cloud Code(서버 함수)를 통해 검증 과정을 거쳐야 합니다.

## 2. 데이터 동기화 및 관리 규약

모든 인게임 데이터는 중앙화된 매니저를 통해 관리됩니다.

### 2-1. PlayerDataManager 사용 강제
- `PlayerDataManager`는 서버 데이터의 로컬 캐시 역할을 수행합니다.
- **직접 접근 금지**: UI나 여타 시스템이 개별적으로 서버 API를 호출하여 데이터를 가져오면 안 됩니다.
- **동기화 흐름**:
  1. `BackendManager`를 통해 서버 로직 수행.
  2. 결과로 받은 최신 데이터를 `PlayerDataManager`에 반영.
  3. `PlayerDataManager`가 `OnDataChanged` 이벤트를 발행.
  4. 구독 중인 UI가 자동으로 갱신됨.

### 2-2. 이벤트 기반 UI 갱신
- UI 스크립트는 `Update()`에서 데이터를 폴링하지 않습니다.
- `PlayerDataManager.Instance.OnDataChanged += UpdateUI;` 와 같이 이벤트를 구독하여 데이터 변경 시점에만 갱신합니다.

## 3. 백엔드 통신 규칙 (싱글톤 강제)

서버와의 직접적인 통신은 오직 `BackendManager` 클래스만 수행해야 합니다.

### 3-1. BackendManager 경유
- UGS(Unity Gaming Services) API (Cloud Code, Authentication, Cloud Save 등)를 개별 스크립트에서 직접 호출하지 마십시오.
- 반드시 `BackendManager`에 래핑된 메서드를 사용해야 합니다.
  - 예: `AuthenticationService.Instance.SignIn...` (X) -> `BackendManager.Instance.Login...` (O)

### 3-2. 전역 접근성
- `BackendManager`와 `PlayerDataManager`는 싱글톤(Singleton)으로 구현되어 있으며, 어디서든 접근 가능합니다.
- 단, 씬 전환 시 파괴되지 않도록(`DontDestroyOnLoad`) 설정되어야 합니다.

## 4. 에러 핸들링 표준

- 모든 백엔드 요청은 비동기(`async/await`)로 처리됩니다.
- 네트워크 오류, 타임아웃, 서버 로직 에러 발생 시 중앙화된 에러 처리기(혹은 팝업)를 통해 유저에게 알립니다.
- 개별 기능 구현 시 `try-catch` 블록으로 예외를 잡되, 치명적인 오류는 로그를 남기고 적절한 피드백을 주어야 합니다.
