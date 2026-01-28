# L10N (로컬라이제이션) 규칙

## 1. 하드코딩 금지
- **엄격한 규칙**: 사용자에게 보이는 스크립트나 인스펙터 속성에 표시 텍스트를 절대 하드코딩하지 마십시오.
- **키**: 모든 문자열은 고유한 Key(문자열)를 사용해야 합니다.
  - 예: `StartButton_Text`, `Damage_Format`.

## 2. 구현
- **컴포넌트**: 모든 Text 오브젝트에 Unity의 Localization 패키지 `LocalizeStringEvent` 또는 커스텀 `L10NText` 컴포넌트를 사용하십시오.
- **스크립트 사용**:
  ```csharp
  // 나쁨 (BAD)
  _titleText.text = "게임 시작";
  
  // 좋음 (GOOD) - 예시
  _titleText.text = Localization.Get("Title_GameStart");
  ```

## 3. 포맷팅
- **동적 텍스트**: 번역 값에 `{0}`, `{1}`과 같은 플레이스홀더를 사용하십시오.
  - 키 `Msg_GetGold`: "골드를 {0} 획득했습니다."
  - 코드: `Localization.Format("Msg_GetGold", 500);`

## 4. 폰트
- TextMeshPro 폰트 에셋이 모든 대상 언어(한국어, 영어 등)를 지원하는지 확인하거나 표준 Fallback 폰트를 사용하십시오.
