# L10N (Localization) Rules

## 1. No Hardcoding
- **Strict Rule**: NEVER hardcode display text in scripts or the Inspector properties that are visible to users.
- **Keys**: All strings must use a unique Key (string).
  - e.g., `StartButton_Text`, `Damage_Format`.

## 2. Implementation
- **Component**: Use Unity's Localization package `LocalizeStringEvent` or a custom `L10NText` component on all Text objects.
- **Script usage**:
  ```csharp
  // BAD
  _titleText.text = "게임 시작";
  
  // GOOD (Example)
  _titleText.text = Localization.Get("Title_GameStart");
  ```

## 3. Formatting
- **Dynamic Text**: Use placeholders `{0}`, `{1}` in the translation values.
  - Key `Msg_GetGold`: "골드를 {0} 획득했습니다."
  - Code: `Localization.Format("Msg_GetGold", 500);`

## 4. Fonts
- Ensure the TextMeshPro font asset supports all target languages (Korean, English, etc.) or use Fallback fonts standard.
