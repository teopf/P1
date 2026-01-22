# Unity & C# Coding Rules

## 1. Code Style & Naming
- **Language**: All comments and documentation MUST be in **Korean**.
- **Class Names**: `PascalCase` (e.g., `PlayerController`).
- **Method Names**: `PascalCase` (e.g., `CalculateDamage`).
- **Fields**:
  - `private` fields: `_camelCase` with valid prefixes (e.g., `_playerHealth`).
  - `public` fields: `PascalCase` (Avoid public fields; use properties).
  - Serialized fields: `[SerializeField] private Type _variableName;` (Preferred over public).
- **Properties**: `PascalCase` (e.g., `public int Health { get; private set; }`).

## 2. Performance & Optimization
- **Caching**: Ensure `GetComponent`, `Find`, and `GameObject.FindWithTag` are called in `Awake` or `Start`, NEVER in `Update`.
- **Update**: Minimize logic in `Update`. Use events/delegates (Observer Pattern) for UI updates.
- **String Concatenation**: Avoid `+` operator in frequent updates. Use `StringBuilder` or `L10N` formatting.
- **Coroutines/Async**: Use `UniTask` (if available) or `Coroutine` for timed actions. Avoid `Invoke`.

## 3. Unity Specifics
- **Serialization**: Use `[SerializeField]` to expose private fields to the Inspector.
- **Attributes**: Use `[Header]`, `[Tooltip]` (in Korean) to make the Inspector user-friendly.
  ```csharp
  [Header("이동 설정")]
  [SerializeField, Tooltip("플레이어의 이동 속도입니다.")]
  private float _moveSpeed = 5.0f;
  ```

## 4. Architecture
- **Managers**: Use the `Singleton` pattern carefully. Ensure managers persist across scenes if necessary (`DontDestroyOnLoad`).
- **Decoupling**: specific logic should be separated from the view. UI scripts should only handle display, not game data logic.
