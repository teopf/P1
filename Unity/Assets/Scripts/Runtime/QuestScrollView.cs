using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UI.Core;
using Data;

/// <summary>
/// 퀘스트 스크롤 뷰 - Object Pooling 기반 퀘스트 엔트리 관리
/// </summary>
public class QuestScrollView : UIBase
{
    [Header("Configuration")]
    public Transform contentRoot; // ScrollRect의 Content Transform

    // Object Pool 리스트
    private List<GameObject> activeEntries = new List<GameObject>();
    private List<GameObject> pooledEntries = new List<GameObject>();

    // 현재 표시 중인 퀘스트 데이터
    private List<QuestData> currentQuests = new List<QuestData>();

    protected override void Awake()
    {
        base.Awake();

        // contentRoot가 설정되지 않았다면 자동 찾기
        if (contentRoot == null)
        {
            var scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect != null) contentRoot = scrollRect.content;
        }

        // Generator가 생성한 8개 퀘스트 엔트리를 풀에 추가
        if (contentRoot != null)
        {
            int childCount = contentRoot.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                var entry = contentRoot.GetChild(i).gameObject;
                entry.SetActive(false);
                pooledEntries.Add(entry);
            }

            Debug.Log($"QuestScrollView: {pooledEntries.Count}개 퀘스트 엔트리를 풀에 추가했습니다.");
        }
    }

    /// <summary>
    /// 카테고리별 퀘스트 데이터 새로고침
    /// </summary>
    /// <param name="category">카테고리 ID (C501=Daily, C502=Weekly, C503=Achievement, C504=Repeat)</param>
    public void RefreshData(string category)
    {
        // 1. 기존 활성 엔트리를 풀로 반환
        foreach (var entry in activeEntries)
        {
            entry.SetActive(false);
            pooledEntries.Add(entry);
        }
        activeEntries.Clear();

        // 2. 카테고리에 맞는 Mock 퀘스트 데이터 가져오기
        QuestType type = ConvertCategoryToType(category);
        currentQuests = QuestDataMock.GetMockQuests(type);

        // 3. 풀에서 엔트리를 가져와 설정
        int count = Mathf.Min(currentQuests.Count, 8); // 최대 8개까지만 표시
        for (int i = 0; i < count; i++)
        {
            GameObject entry = GetEntryFromPool();
            if (entry != null)
            {
                SetupEntry(entry, currentQuests[i]);
                activeEntries.Add(entry);
            }
        }

        Debug.Log($"QuestScrollView: {category} 카테고리로 {count}개 퀘스트를 표시합니다.");
    }

    /// <summary>
    /// 카테고리 ID를 QuestType으로 변환
    /// </summary>
    private QuestType ConvertCategoryToType(string category)
    {
        switch (category)
        {
            case "C501": return QuestType.Daily;
            case "C502": return QuestType.Weekly;
            case "C503": return QuestType.Achievement;
            case "C504": return QuestType.Repeat;
            default: return QuestType.Daily;
        }
    }

    /// <summary>
    /// 풀에서 퀘스트 엔트리 가져오기
    /// </summary>
    private GameObject GetEntryFromPool()
    {
        if (pooledEntries.Count > 0)
        {
            var entry = pooledEntries[0];
            pooledEntries.RemoveAt(0);
            entry.SetActive(true);
            return entry;
        }
        else
        {
            Debug.LogWarning("QuestScrollView: 퀘스트 엔트리 풀이 고갈되었습니다!");
            return null;
        }
    }

    /// <summary>
    /// 퀘스트 엔트리 UI 설정
    /// </summary>
    private void SetupEntry(GameObject entry, QuestData quest)
    {
        // 엔트리 이름 업데이트
        entry.name = $"Group_Quest_{quest.id}";

        // 1. 아이콘 설정 (AD6xx_Icon)
        var iconTransform = entry.transform.Find("AD619_Icon"); // 실제로는 각 엔트리마다 다른 이름
        if (iconTransform == null)
        {
            // Generator가 생성한 첫 번째 Image 찾기
            for (int i = 0; i < entry.transform.childCount; i++)
            {
                var child = entry.transform.GetChild(i);
                if (child.name.StartsWith("AD6") && child.name.Contains("Icon"))
                {
                    iconTransform = child;
                    break;
                }
            }
        }

        if (iconTransform != null)
        {
            var iconImg = iconTransform.GetComponent<Image>();
            if (iconImg != null)
            {
                // Mock: 색상 변경으로 아이콘 표현
                iconImg.color = Color.HSVToRGB((quest.iconId % 10) / 10f, 0.6f, 0.9f);
            }
        }

        // 2. QuestInfo 영역에서 제목, 설명, 진행 바 찾기
        var questInfo = entry.transform.Find("QuestInfo");
        if (questInfo != null)
        {
            // 제목 설정 (QTxxx_Title)
            var titleTransform = questInfo.Find("QT101_Title"); // 실제로는 동적으로 찾아야 함
            if (titleTransform == null)
            {
                for (int i = 0; i < questInfo.childCount; i++)
                {
                    var child = questInfo.GetChild(i);
                    if (child.name.StartsWith("QT") && child.name.Contains("Title"))
                    {
                        titleTransform = child;
                        break;
                    }
                }
            }

            if (titleTransform != null)
            {
                var titleText = titleTransform.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.text = quest.title;
                }
            }

            // 설명 설정 (QTxxx_Desc)
            var descTransform = questInfo.Find("QT101_Desc");
            if (descTransform == null)
            {
                for (int i = 0; i < questInfo.childCount; i++)
                {
                    var child = questInfo.GetChild(i);
                    if (child.name.StartsWith("QT") && child.name.Contains("Desc"))
                    {
                        descTransform = child;
                        break;
                    }
                }
            }

            if (descTransform != null)
            {
                var descText = descTransform.GetComponent<Text>();
                if (descText != null)
                {
                    descText.text = quest.description;
                }
            }

            // 진행 바 설정 (QTPxxx_ProgressBar)
            var progressBarTransform = questInfo.Find("QTP101_ProgressBar");
            if (progressBarTransform == null)
            {
                for (int i = 0; i < questInfo.childCount; i++)
                {
                    var child = questInfo.GetChild(i);
                    if (child.name.StartsWith("QTP") && child.name.Contains("ProgressBar"))
                    {
                        progressBarTransform = child;
                        break;
                    }
                }
            }

            if (progressBarTransform != null)
            {
                // 진행 바 Fill 찾기
                var fillTransform = progressBarTransform.Find("ProgressBar_Background/ProgressBar_Fill");
                if (fillTransform != null)
                {
                    var fillImg = fillTransform.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.fillAmount = quest.GetProgressRatio();

                        // 완료 시 초록색, 진행중 시 파란색
                        if (quest.status == QuestStatus.Completed)
                        {
                            fillImg.color = new Color32(76, 175, 80, 255); // Green
                        }
                        else
                        {
                            fillImg.color = new Color32(107, 140, 255, 255); // Blue
                        }
                    }
                }

                // 진행 바 텍스트 설정
                var progressTextTransform = progressBarTransform.Find("ProgressBar_Background/ProgressText");
                if (progressTextTransform != null)
                {
                    var progressText = progressTextTransform.GetComponent<Text>();
                    if (progressText != null)
                    {
                        progressText.text = $"{quest.currentProgress}/{quest.goalProgress}";
                    }
                }
            }
        }

        // 3. Claim 버튼 상태 설정 (QOKxxx_ClaimBtn)
        var claimBtnTransform = entry.transform.Find("QOK101_ClaimBtn");
        if (claimBtnTransform == null)
        {
            for (int i = 0; i < entry.transform.childCount; i++)
            {
                var child = entry.transform.GetChild(i);
                if (child.name.StartsWith("QOK") && child.name.Contains("ClaimBtn"))
                {
                    claimBtnTransform = child;
                    break;
                }
            }
        }

        if (claimBtnTransform != null)
        {
            var claimBtn = claimBtnTransform.GetComponent<Button>();
            if (claimBtn != null)
            {
                SetClaimButtonState(claimBtn, quest.status, quest.id);
            }
        }
    }

    /// <summary>
    /// Claim 버튼 상태 설정
    /// </summary>
    private void SetClaimButtonState(Button btn, QuestStatus status, int questId)
    {
        Image img = btn.GetComponent<Image>();
        Text label = btn.GetComponentInChildren<Text>();

        switch (status)
        {
            case QuestStatus.Locked:
                // 잠김 상태: 회색, 비활성화
                if (img != null) img.color = new Color32(150, 150, 150, 76);
                if (label != null) label.text = "Locked";
                btn.interactable = false;
                break;

            case QuestStatus.InProgress:
                // 진행중 상태: 주황색, 비활성화
                if (img != null) img.color = new Color32(255, 136, 0, 64);
                if (label != null) label.text = $"{Mathf.RoundToInt(GetQuestProgress(questId) * 100)}%";
                btn.interactable = false;
                break;

            case QuestStatus.Completed:
                // 완료 상태: 초록색, 활성화, 클릭 가능
                if (img != null) img.color = new Color32(255, 215, 0, 102); // Gold
                if (label != null) label.text = "Claim!";
                btn.interactable = true;

                // 클릭 이벤트 설정
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClaimClicked(questId, btn));
                break;

            case QuestStatus.Claimed:
                // 수령 완료 상태: 어두운 회색, 비활성화
                if (img != null) img.color = new Color32(76, 175, 80, 76); // Dark Green
                if (label != null) label.text = "Claimed";
                btn.interactable = false;
                break;
        }
    }

    /// <summary>
    /// Claim 버튼 클릭 핸들러
    /// </summary>
    private void OnClaimClicked(int questId, Button btn)
    {
        Debug.Log($"QuestScrollView: Quest {questId} 보상을 수령합니다!");

        // TODO: 실제 보상 지급 로직 (QuestManager.Instance.ClaimReward(questId) 등)

        // Mock: 퀘스트 상태를 Claimed로 변경
        var quest = currentQuests.Find(q => q.id == questId);
        if (quest != null)
        {
            quest.status = QuestStatus.Claimed;

            // 버튼 상태 즉시 업데이트
            SetClaimButtonState(btn, QuestStatus.Claimed, questId);
        }
    }

    /// <summary>
    /// 퀘스트 진행률 가져오기 (헬퍼 메서드)
    /// </summary>
    private float GetQuestProgress(int questId)
    {
        var quest = currentQuests.Find(q => q.id == questId);
        if (quest != null)
        {
            return quest.GetProgressRatio();
        }
        return 0f;
    }
}
