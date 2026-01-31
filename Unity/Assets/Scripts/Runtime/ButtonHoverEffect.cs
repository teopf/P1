using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI.Core;

// UI.Core 네임스페이스 또는 전역 네임스페이스 유지
public class ButtonHoverEffect : UIBase, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Coroutine currentCoroutine;
    [SerializeField] private float scaleFactor = 1.1f;
    [SerializeField] private float duration = 0.1f;

    protected override void Awake() // UIBase override
    {
        base.Awake();
        originalScale = transform.localScale;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // 활성화 시 원래 스케일로 보장 (애니메이션 중간에 꺼졌을 때 대비)
        transform.localScale = originalScale;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // 비활성화 시 스케일 복구
        transform.localScale = originalScale;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsActive()) return; // UIBehaviour.IsActive() 체크

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateScale(originalScale * scaleFactor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsActive()) return;

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateScale(originalScale));
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth step (EaseInOut)
            t = t * t * (3f - 2f * t);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
