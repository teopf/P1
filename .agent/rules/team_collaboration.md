# Team Collaboration Rules

## 1. Version Control (Git)
- **Commit Messages**: [Type]: Description
  - `feat`: New feature
  - `fix`: Bug fix
  - `refactor`: Code restructuring
  - `chore`: Meta tasks (build, tools)
  - `docs`: Documentation
  - *Example*: `feat: Add Offline Reward Logic`

## 2. Scene Management
- **Rule**: Avoid working on the same Scene file simultaneously.
- **Prefab First**: Make everything a Prefab. Edit Prefabs instead of scene objects.
- **Scene Split**:
  - `MainScene` (Managers, Persistent objects)
  - `UIScene` (Canvas, HUD)
  - `GameScene` (Gameplay elements)
  - Use `SceneManager.LoadScene(..., LoadSceneMode.Additive)` to combine them.

## 3. Code Review
- **Language**: Comments in Code Review should be in Korean.
- **Checklist**:
  - Does it follow `unity_coding_rules.md`?
  - Is text localized (`l10n_rules.md`)?
  - Are big numbers handled correctly (`idle_game_patterns.md`)?

## 4. Review Etiquette
- Be constructive.
- Focus on logic and stability, not personal preference (unless strict style guide violation).
