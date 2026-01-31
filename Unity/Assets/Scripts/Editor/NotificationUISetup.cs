using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UI;

/// <summary>
/// TopPanel 하단에 백엔드 알림 토스트 UI를 자동 생성하는 Editor 툴.
/// Tools > UI > Generate User Data Notification 메뉴로 실행합니다.
/// </summary>
public class NotificationUISetup : Editor
{
    // 색상 정의
    private static readonly Color COLOR_BG = new Color(0, 0, 0, 0.75f);
    private static readonly Color COLOR_TEXT = Color.white;

    [MenuItem("Tools/UI/Generate User Data Notification")]
    public static void GenerateNotificationUI()
    {
        // 1. HUD_Canvas와 AA1_TopPanel 찾기
        GameObject canvasObj = GameObject.Find("HUD_Canvas");
        if (canvasObj == null)
        {
            EditorUtility.DisplayDialog("오류", "HUD_Canvas를 찾을 수 없습니다.\n먼저 HUD를 생성해주세요.", "확인");
            return;
        }

        Transform topPanel = canvasObj.transform.Find("AA1_TopPanel");
        if (topPanel == null)
        {
            EditorUtility.DisplayDialog("오류", "AA1_TopPanel을 찾을 수 없습니다.\n먼저 HUD를 생성해주세요.", "확인");
            return;
        }

        // 2. 기존 NotificationPanel이 있는지 확인 (중복 생성 방지)
        Transform existing = topPanel.Find("NotificationPanel");
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "확인",
                "이미 NotificationPanel이 존재합니다.\n기존 요소를 삭제하고 다시 생성하시겠습니까?",
                "예",
                "아니오"
            );

            if (!overwrite) return;

            DestroyImmediate(existing.gameObject);
        }

        // 3. NotificationPanel 생성
        GameObject panelObj = CreateNotificationPanel(topPanel);

        // 4. NotificationUI 컴포넌트 추가 및 필드 연결
        SetupNotificationUIComponent(panelObj);

        // 5. Undo 등록 (Ctrl+Z로 되돌리기 가능)
        Undo.RegisterCreatedObjectUndo(panelObj, "Generate Notification UI");

        // 완료 메시지
        EditorUtility.DisplayDialog(
            "완료",
            "백엔드 알림 UI가 성공적으로 생성되었습니다!\n\n" +
            "생성된 오브젝트: AA1_TopPanel > NotificationPanel\n\n" +
            "검증 방법:\n" +
            "1. Play Mode 진입 시 '유저 데이터 로드 중...' 메시지 확인\n" +
            "2. 저장/초기화 시 토스트 메시지 확인",
            "확인"
        );

        Debug.Log("[NotificationUISetup] 백엔드 알림 UI 생성 완료!");
    }

    /// <summary>
    /// NotificationPanel (배경 Image + 메시지 Text) 오브젝트를 생성합니다.
    /// TopPanel 하단 중앙에 배치됩니다.
    /// </summary>
    private static GameObject CreateNotificationPanel(Transform parent)
    {
        // NotificationPanel 컨테이너
        GameObject panelObj = new GameObject("NotificationPanel");
        panelObj.transform.SetParent(parent, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        // TopPanel 하단 중앙에 배치
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(400, 50);

        // 배경 이미지
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = COLOR_BG;

        // CanvasGroup (페이드 아웃용)
        CanvasGroup canvasGroup = panelObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // 초기에는 숨김
        canvasGroup.blocksRaycasts = false;

        // 메시지 텍스트
        GameObject textObj = new GameObject("Message_Text");
        textObj.transform.SetParent(panelObj.transform, false);

        TextMeshProUGUI messageText = textObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "";
        messageText.fontSize = 20;
        messageText.color = COLOR_TEXT;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        // 텍스트가 패널 전체를 채우도록 설정
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        Debug.Log("[NotificationUISetup] NotificationPanel 생성 완료");
        return panelObj;
    }

    /// <summary>
    /// NotificationUI 컴포넌트를 추가하고 SerializedObject로 private 필드를 연결합니다.
    /// </summary>
    private static void SetupNotificationUIComponent(GameObject panelObj)
    {
        // NotificationUI 컴포넌트 추가
        NotificationUI component = panelObj.AddComponent<NotificationUI>();

        // SerializedObject를 사용하여 private SerializeField에 접근
        SerializedObject serializedObject = new SerializedObject(component);

        // _canvasGroup 연결
        SerializedProperty canvasGroupProp = serializedObject.FindProperty("_canvasGroup");
        canvasGroupProp.objectReferenceValue = panelObj.GetComponent<CanvasGroup>();

        // _messageText 연결
        Transform messageText = panelObj.transform.Find("Message_Text");
        if (messageText != null)
        {
            SerializedProperty messageTextProp = serializedObject.FindProperty("_messageText");
            messageTextProp.objectReferenceValue = messageText.GetComponent<TextMeshProUGUI>();
        }

        // 변경사항 적용
        serializedObject.ApplyModifiedProperties();

        Debug.Log("[NotificationUISetup] NotificationUI 컴포넌트 추가 및 필드 연결 완료");
    }
}
