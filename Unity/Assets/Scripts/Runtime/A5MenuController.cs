using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UI.Core;

/// <summary>
/// a5 퀘스트 메뉴 컨트롤러 - 메뉴 생명주기 및 이벤트 관리
/// </summary>
public class A5MenuController : UIBase
{
    [Header("UI References")]
    public Button btnClose; // b101 닫기 버튼
    public ToggleGroup categoryGroup; // AA503 카테고리 토글 그룹
    public QuestScrollView questScrollView; // AD601 퀘스트 스크롤 뷰
    public Image progressBarFill; // AD602 전체 진행률 바 Fill
    public Text progressText; // AD602 전체 진행률 텍스트

    private CanvasGroup canvasGroup;
    private bool isAnimating = false;

    protected override void Awake()
    {
        base.Awake();

        // CanvasGroup 자동 찾기 또는 추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Auto-link UI References
        if (btnClose == null)
        {
            var btn = transform.Find("Panel_AA101_Header/Btn_Close_b101");
            if (btn != null) btnClose = btn.GetComponent<Button>();
        }

        if (categoryGroup == null)
        {
            var group = transform.Find("Panel_AA503_Category");
            if (group != null) categoryGroup = group.GetComponent<ToggleGroup>();
        }

        if (questScrollView == null)
        {
            questScrollView = GetComponentInChildren<QuestScrollView>();
        }

        if (progressBarFill == null)
        {
            var fill = transform.Find("Panel_AD602_OverallProgress/ProgressBar_Background/ProgressBar_Fill");
            if (fill != null) progressBarFill = fill.GetComponent<Image>();
        }

        if (progressText == null)
        {
            var text = transform.Find("Panel_AD602_OverallProgress/Text_Progress");
            if (text != null) progressText = text.GetComponent<Text>();
        }

        // 버튼 이벤트 리스너 설정
        if (btnClose != null)
        {
            btnClose.onClick.AddListener(OnCloseClicked);
        }

        // 카테고리 토글 이벤트 리스너 설정
        if (categoryGroup != null)
        {
            var toggles = categoryGroup.GetComponentsInChildren<Toggle>(true);
            foreach (var toggle in toggles)
            {
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) OnCategoryChanged(toggle.name);
                });
            }

            // 첫 번째 토글 활성화 (Daily)
            if (toggles.Length > 0)
            {
                toggles[0].isOn = true;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // 진입 애니메이션
        StartCoroutine(AnimatePanel(true));

        // 퀘스트 데이터 새로고침
        RefreshQuestData();
    }

    /// <summary>
    /// 퀘스트 데이터 새로고침
    /// </summary>
    private void RefreshQuestData()
    {
        if (questScrollView == null) return;

        // 현재 활성화된 카테고리 가져오기
        string category = GetActiveCategoryName();

        // 스크롤 뷰 새로고침
        questScrollView.RefreshData(category);

        // 전체 진행률 업데이트
        UpdateProgressBar(category);
    }

    /// <summary>
    /// 현재 활성화된 카테고리 이름 가져오기
    /// </summary>
    private string GetActiveCategoryName()
    {
        if (categoryGroup == null) return "C501";

        var activeToggle = categoryGroup.GetFirstActiveToggle();
        if (activeToggle == null) return "C501";

        // "Tgl_Category_C501" → "C501"
        string toggleName = activeToggle.name;
        return toggleName.Replace("Tgl_Category_", "");
    }

    /// <summary>
    /// 카테고리 변경 이벤트 핸들러
    /// </summary>
    private void OnCategoryChanged(string toggleName)
    {
        if (isAnimating) return;

        // "Tgl_Category_C501" → "C501"
        string category = toggleName.Replace("Tgl_Category_", "");

        Debug.Log($"A5MenuController: 카테고리 변경 - {category}");

        // 퀘스트 스크롤 뷰 새로고침
        if (questScrollView != null)
        {
            questScrollView.RefreshData(category);
        }

        // 전체 진행률 업데이트
        UpdateProgressBar(category);
    }

    /// <summary>
    /// 전체 진행률 바 업데이트
    /// </summary>
    private void UpdateProgressBar(string category)
    {
        // TODO: QuestManager에서 실제 진행률 데이터 가져오기
        // Mock: 랜덤 진행률 표시
        float mockProgress = Random.Range(0.5f, 0.95f);
        int completed = Mathf.RoundToInt(mockProgress * 20);
        int total = 20;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = mockProgress;
        }

        if (progressText != null)
        {
            string categoryName = ConvertCategoryIdToName(category);
            progressText.text = $"{categoryName}: {completed}/{total}";
        }
    }

    /// <summary>
    /// 카테고리 ID를 이름으로 변환
    /// </summary>
    private string ConvertCategoryIdToName(string categoryId)
    {
        switch (categoryId)
        {
            case "C501": return "Daily";
            case "C502": return "Weekly";
            case "C503": return "Achievement";
            case "C504": return "Repeat";
            default: return "Daily";
        }
    }

    /// <summary>
    /// 닫기 버튼 클릭 핸들러
    /// </summary>
    private void OnCloseClicked()
    {
        if (isAnimating) return;

        Debug.Log("A5MenuController: 닫기 버튼 클릭");
        StartCoroutine(CloseSequence());
    }

    /// <summary>
    /// 닫기 시퀀스 (애니메이션 + 비활성화)
    /// </summary>
    private IEnumerator CloseSequence()
    {
        // 1. 버튼 펀치 애니메이션
        if (btnClose != null)
        {
            yield return StartCoroutine(PunchScale(btnClose.transform));
        }

        // 2. 패널 종료 애니메이션
        yield return StartCoroutine(AnimatePanel(false));

        // 3. GameObject 비활성화
        gameObject.SetActive(false);

        // 4. HUDController에 닫힘 알림
        var hudController = FindObjectOfType<HUDController>();
        if (hudController != null)
        {
            hudController.OnSubMenuClosed();
        }
    }

    /// <summary>
    /// 패널 진입/종료 애니메이션 (코루틴 기반)
    /// </summary>
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
            t = t * (2 - t); // Ease Out Quad

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        transform.localScale = endScale;

        isAnimating = false;
    }

    /// <summary>
    /// 버튼 펀치 스케일 애니메이션
    /// </summary>
    private IEnumerator PunchScale(Transform target)
    {
        float duration = 0.1f;
        Vector3 originalScale = Vector3.one;

        // Punch in (1.0 → 0.9)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = Mathf.Lerp(1f, 0.9f, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        // Bounce back (0.9 → 1.05 → 1.0)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = Mathf.Lerp(0.9f, 1.05f, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        // Return to normal
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            float s = Mathf.Lerp(1.05f, 1f, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        target.localScale = originalScale;
    }
}
