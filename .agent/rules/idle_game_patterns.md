# Idle Game Patterns

## 1. Big Number Handling
- **Type**: Use a custom `BigDouble` or `BigInteger` wrapper for all currency and damage values. NEVER use `int` or `float` for gold/damage that scales infinitely.
- **Display**: Use a formatter that converts large numbers to readable strings (e.g., "1.2A", "500B").
  ```csharp
  public string FormatCurrency(BigDouble value) {
      // Implementation for 3-character suffix (A, B, C...)
  }
  ```

## 2. Offline Reward
- **Timestamp**: Store `LastLoginTime` (UTC) in PlayerPrefs or SaveFile on application quit/pause.
- **Calculation**: On start, `CurrentTime - LastLoginTime` = `OfflineSeconds`.
- **Limit**: Apply a maximum offline time limit (e.g., 8 hours).
- **Formula**: `Reward = OfflineSeconds * CPS (Currency Per Second)`.

## 3. Data Persistence
- **Auto Save**: Trigger save every N seconds (e.g., 30s) and on application pause/quit.
- **Format**: Use JSON for flexibility or Binary for performance/security.
- **Cloud Save**: Prepare structure to sync local save with backend (Firebase/PlayFab).

## 4. Managers
- **GameManager**: Central hub for game state (Playing, Paused, etc.).
- **CurrencyManager**: Handles add/subtract of currencies and invokes events `OnCurrencyChanged`.
- **UpgradeManager**: Handles cost calculation and level-up logic for stats.
