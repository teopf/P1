---
name: ui_skill
description: Create a UGUI Panel with DOTween animations (Pop-up/Modal style)
---

# Create DOTween UI Panel

This skill creates a new UI Panel script and sets up the basic structure for UGUI + DOTween interactions.

## Instructions
1.  **Create Script**: Create a C# script named `[PanelName]Panel.cs` inheriting from `UIPanelBase` (or `MonoBehaviour` if base missing).
2.  **Imports**: Add `using DG.Tweening;` and `using UnityEngine.UI;` / `using TMPro;`.
3.  **Components**:
    - Require `CanvasGroup`.
    - Define UI elements with `[SerializeField] private`.
4.  **Animation Logic**:
    - Implement `Show()`: Enable gameObject -> Reset Alpha/Scale -> Animate In.
    - Implement `Hide()`: Animate Out -> OnComplete(Disable gameObject).

## Template Code

```csharp
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class [PanelName]Panel : MonoBehaviour // or inherits from a Base UI Class
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _contentRect; // Main content container for scaling
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
        
        // Reset state
        _canvasGroup.alpha = 0f;
        if (_contentRect != null) _contentRect.localScale = Vector3.zero;

        // Animation Sequence
        Sequence seq = DOTween.Sequence();
        seq.Append(_canvasGroup.DOFade(1f, _animDuration));
        if (_contentRect != null)
        {
            seq.Join(_contentRect.DOScale(1f, _animDuration).SetEase(Ease.OutBack));
        }
    }

    public void Hide()
    {
        // Animation Sequence
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
    
    // L10N Example
    public void SetTitle(string key)
    {
        // Assuming Localization wrapper exists
        // _titleText.text = Localization.Get(key);
    }
}
```

## Checklist after generation
- [ ] Script created?
- [ ] `DG.Tweening` namespace included?
- [ ] `Show()` uses `Ease.OutBack` (or similar bouncy effect)?
- [ ] `Hide()` uses `OnComplete` to deactivate?
