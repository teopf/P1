# Unity & C# 코딩 규칙

## 1. 코드 스타일 & 명명법
- **언어**: 모든 주석과 문서는 반드시 **한국어**로 작성해야 합니다.
- **클래스 이름**: `PascalCase` (예: `PlayerController`).
- **메서드 이름**: `PascalCase` (예: `CalculateDamage`).
- **필드**:
  - `private` 필드: `_camelCase`와 유효한 접두사 (예: `_playerHealth`).
  - `public` 필드: `PascalCase` (public 필드는 피하고 프로퍼티를 사용하십시오).
  - 직렬화(Serialized) 필드: `[SerializeField] private Type _variableName;` (public보다 권장됨).
- **프로퍼티**: `PascalCase` (예: `public int Health { get; private set; }`).

## 2. 성능 & 최적화
- **캐싱**: `GetComponent`, `Find`, `GameObject.FindWithTag`는 `Awake`나 `Start`에서 호출해야 하며, 절대 `Update`에서 호출하지 마십시오.
- **Update**: `Update` 내의 로직을 최소화하십시오. UI 업데이트에는 이벤트/델리게이트(옵저버 패턴)를 사용하십시오.
- **문자열 연결**: 빈번한 업데이트에서 `+` 연산자를 피하십시오. `StringBuilder`나 `L10N` 포맷팅을 사용하십시오.
- **코루틴/비동기**: `UniTask`(사용 가능한 경우) 또는 타이밍 동작에는 `Coroutine`을 사용하십시오. `Invoke`는 피하십시오.

## 3. Unity 특이사항
- **직렬화**: private 필드를 인스펙터에 노출하려면 `[SerializeField]`를 사용하십시오.
- **속성(Attributes)**: 인스펙터를 사용자 친화적으로 만들기 위해 `[Header]`, `[Tooltip]`(한국어)을 사용하십시오.
  ```csharp
  [Header("이동 설정")]
  [SerializeField, Tooltip("플레이어의 이동 속도입니다.")]
  private float _moveSpeed = 5.0f;
  ```

## 4. 아키텍처
- **매니저**: `Singleton` 패턴을 주의해서 사용하십시오. 필요한 경우 매니저가 씬 간에 유지되도록 하십시오 (`DontDestroyOnLoad`).
- **결합도 낮추기 (Decoupling)**: 특정 로직은 뷰(View)와 분리되어야 합니다. UI 스크립트는 게임 데이터 로직이 아닌 표시(Display)만 처리해야 합니다.
