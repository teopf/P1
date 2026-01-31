using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UI.Core;

public class A2MenuController : UIBase
{
    [Header("UI References")]
    public Button btnClose; // b101
    public ToggleGroup categoryGroup; // AA202
    public Transform contentPanel; // The main panel to animate (or canvas group)
    
    [Header("Inventory System")]
    public InventoryScrollView inventoryScrollView;

    private CanvasGroup canvasGroup;
    private bool isAnimating = false;

    protected override void Awake()
    {
        base.Awake();

        // Auto-link references if not assigned
        if (contentPanel == null) contentPanel = transform.Find("Panel_AA101_Header")?.parent; // Canvas Root
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Find Close Button
        if (btnClose == null)
            btnClose = transform.Find("Panel_AA101_Header/Btn_Close_b101")?.GetComponent<Button>();
            
        // Find Category Group
        if (categoryGroup == null)
            categoryGroup = transform.Find("Panel_AA202_Category")?.GetComponent<ToggleGroup>();

        // Find Scroll View
        if (inventoryScrollView == null)
        {
            var scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (scrollRect != null)
            {
                inventoryScrollView = scrollRect.gameObject.GetComponent<InventoryScrollView>();
                if (inventoryScrollView == null) inventoryScrollView = scrollRect.gameObject.AddComponent<InventoryScrollView>();
            }
        }

        // Setup Listeners
        if (btnClose != null) btnClose.onClick.AddListener(OnCloseClicked);
        
        if (categoryGroup != null)
        {
            var toggles = categoryGroup.GetComponentsInChildren<Toggle>(true);
            foreach (var t in toggles)
            {
                t.onValueChanged.AddListener((isOn) => 
                {
                    if (isOn) OnCategoryChanged(t.name);
                });
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Animate Entry
        StartCoroutine(AnimatePanel(true));
        
        // Refresh Inventory
        if (inventoryScrollView != null)
        {
            inventoryScrollView.RefreshData(GetCategoryFromToggle());
        }
    }

    private string GetCategoryFromToggle()
    {
        if (categoryGroup == null) return "All";
        var active = categoryGroup.GetFirstActiveToggle();
        return active != null ? active.name.Replace("Tgl_Category_", "") : "All";
    }

    public void OnCloseClicked()
    {
        StartCoroutine(CloseSequence());
    }

    private void OnCategoryChanged(string categoryName)
    {
        string cat = categoryName.Replace("Tgl_Category_", "");
        Debug.Log($"Category Changed to: {cat}");
        if (inventoryScrollView != null)
        {
            inventoryScrollView.RefreshData(cat);
        }
    }

    private IEnumerator CloseSequence()
    {
        // Animate Button Click
        if (btnClose != null) yield return StartCoroutine(PunchScale(btnClose.transform));

        // Animate Fade Out
        yield return StartCoroutine(AnimatePanel(false));
        
        // Deactivate Ref to Main HUD? 
        // We just disable this object, HUDController handles the rest via state check if needed
        // Or we notify HUD. For now, just hide self.
        gameObject.SetActive(false);
        
        // Also notify HUD to revert "a2" state? 
        // HUDController checks active state or we can call specific method. 
        // Ideally HUDController listens to this, but valid direct call:
        var hud = FindObjectOfType<HUDController>();
        if (hud != null) hud.OnSubMenuClosed();
    }

    private IEnumerator AnimatePanel(bool show)
    {
        isAnimating = true;
        float duration = 0.2f;
        float elapsed = 0f;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;
        
        Vector3 startScale = show ? Vector3.one * 0.95f : Vector3.one;
        Vector3 endScale = show ? Vector3.one : Vector3.one * 0.95f;

        canvasGroup.alpha = startAlpha;
        transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease Out Quad
            t = t * (2 - t); 
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        transform.localScale = endScale;
        isAnimating = false;
    }

    private IEnumerator PunchScale(Transform target)
    {
        float duration = 0.1f;
        float elapsed = 0f;
        Vector3 original = Vector3.one;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Punch in
            float s = Mathf.Lerp(1f, 0.9f, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Bounce back
            float s = Mathf.Lerp(0.9f, 1.0f, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = original;
    }
}
