---
name: ui_skill
description: DOTween 애니메이션(팝업/모달 스타일)이 포함된 UGUI 패널 생성
---

# DOTween UI 패널 생성

이 스킬은 새로운 UI 패널 스크립트를 생성하고 UGUI + DOTween 상호작용을 위한 기본 구조를 설정합니다.

## 지시사항 (Instructions)
1. **스크립트 생성**: `UIPanelBase`(또는 베이스 클래스가 없다면 `MonoBehaviour`)를 상속받는 `[PanelName]Panel.cs`라는 이름의 C# 스크립트를 생성하십시오.
2. **임포트**: `using DG.Tweening;` 및 `using UnityEngine.UI;` / `using TMPro;`를 추가하십시오.
3. **컴포넌트**:
    - `CanvasGroup`이 필요합니다.
    - UI 요소들을 `[SerializeField] private`로 정의하십시오.
4. **애니메이션 로직**:
    - `Show()` 구현: gameObject 활성화 -> Alpha/Scale 리셋 -> 등장 애니메이션(Animate In).
    - `Hide()` 구현: 퇴장 애니메이션(Animate Out) -> `OnComplete`에서 gameObject 비활성화.

## 템플릿 코드 (Template Code)

```csharp
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class [PanelName]Panel : MonoBehaviour // 또는 기본 UI 클래스 상속
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _contentRect; // 스케일링을 위한 메인 콘텐츠 컨테이너
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _titleText;

    [Header("Settings")]
    [SerializeField] private float _animDuration = 0.3f;

    private void Awake()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Hide);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        
        // 상태 초기화 (Reset state)
        _canvasGroup.alpha = 0f;
        if (_contentRect != null) _contentRect.localScale = Vector3.zero;

        // 애니메이션 시퀀스 (Animation Sequence)
        Sequence seq = DOTween.Sequence();
        seq.Append(_canvasGroup.DOFade(1f, _animDuration));
        if (_contentRect != null)
        {
            seq.Join(_contentRect.DOScale(1f, _animDuration).SetEase(Ease.OutBack));
        }
    }

    public void Hide()
    {
        // 애니메이션 시퀀스 (Animation Sequence)
        Sequence seq = DOTween.Sequence();
        seq.Append(_canvasGroup.DOFade(0f, _animDuration * 0.8f));
        if (_contentRect != null)
        {
            seq.Join(_contentRect.DOScale(0.8f, _animDuration * 0.8f).SetEase(Ease.InBack));
        }

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
    
    // L10N 예시 (L10N Example)
    public void SetTitle(string key)
    {
        // Localization 래퍼가 존재한다고 가정
        // _titleText.text = Localization.Get(key);
    }
}
```

## 생성 후 체크리스트 (Checklist after generation)
- [ ] 스크립트가 생성되었나요?
- [ ] `DG.Tweening` 네임스페이스가 포함되었나요?
- [ ] `Show()`가 `Ease.OutBack` (또는 유사한 바운스 효과)을 사용하나요?
- [ ] `Hide()`가 비활성화를 위해 `OnComplete`를 사용하나요?
