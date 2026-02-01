using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Backend 상태 표시 UI를 HUD_Canvas에 자동 생성하는 Editor 툴
/// Tools > UI > Setup Backend Status Display 메뉴로 실행
/// 
/// 구조 변경: PlayerID_Text의 자식으로 Backend_PID_Text를 배치하여 VerticalLayoutGroup 문제 해결
/// </summary>
public class BackendStatusDisplaySetup : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;
    
    // 색상 정의
    private static readonly Color COLOR_WHITE = Color.white;
    private static readonly Color COLOR_GREEN = new Color32(76, 175, 80, 255);
    private static readonly Color COLOR_LOG_BG = new Color(0, 0, 0, 0.7f);

    [MenuItem("Tools/UI/Setup Backend Status Display")]
    public static void SetupBackendStatusDisplay()
    {
        // 1. HUD_Canvas 찾기
        GameObject canvasObj = GameObject.Find("HUD_Canvas");
        if (canvasObj == null)
        {
            EditorUtility.DisplayDialog("오류", "HUD_Canvas를 찾을 수 없습니다.\n먼저 'Tools > UI > Generate HUD Layout'을 실행해주세요.", "확인");
            return;
        }

        // 2. AA1_TopPanel 찾기
        Transform topPanel = canvasObj.transform.Find("AA1_TopPanel");
        if (topPanel == null)
        {
            EditorUtility.DisplayDialog("오류", "AA1_TopPanel을 찾을 수 없습니다.\n먼저 HUD를 생성해주세요.", "확인");
            return;
        }

        // 3. PlayerInfo_Group 찾기
        Transform playerInfoGroup = topPanel.Find("PlayerInfo_Group");
        if (playerInfoGroup == null)
        {
            EditorUtility.DisplayDialog("오류", "PlayerInfo_Group을 찾을 수 없습니다.\n먼저 'Tools > UI > Setup HUD User Data Display'를 실행해주세요.", "확인");
            return;
        }

        // 4. 복구 및 정리 (기존 Row 방식 제거)
        CleanupPreviousAttempts(playerInfoGroup, canvasObj.transform);

        // 5. UI 요소 생성
        CreateBackendPIDText(playerInfoGroup);
        CreateBackendLogPanel(canvasObj.transform);

        // 완료 메시지
        EditorUtility.DisplayDialog(
            "완료",
            "Backend 상태 표시 UI가 성공적으로 생성되었습니다!\n\n" +
            "구조 변경:\n" +
            "• PlayerID_Text에 ContentSizeFitter 적용\n" +
            "• Backend_PID_Text가 PlayerID_Text 바로 우측에 붙음\n\n" +
            "BackendStatusDisplay 컴포넌트가 자동으로 연결됩니다.",
            "확인"
        );

        Debug.Log("[BackendStatusDisplaySetup] Backend 상태 표시 UI 생성 완료!");
    }

    private static void CleanupPreviousAttempts(Transform playerInfoGroup, Transform hudCanvas)
    {
        // 1. PlayerID_Row가 있는지 확인 (이전 시도)
        Transform row = playerInfoGroup.Find("PlayerID_Row");
        if (row != null)
        {
            // PlayerID_Text를 구출
            Transform pIdText = row.Find("PlayerID_Text");
            if (pIdText != null)
            {
                pIdText.SetParent(playerInfoGroup, false);
                pIdText.SetSiblingIndex(0); // 맨 위로
            }
            // Row 삭제
            DestroyImmediate(row.gameObject);
        }

        // 2. Backend_PID_Text가 PlayerInfo_Group 직속으로 있는지 확인 (초기 버전)
        Transform directPid = playerInfoGroup.Find("Backend_PID_Text");
        if (directPid != null) DestroyImmediate(directPid.gameObject);

        // 3. Backend_LogPanel 삭제 (재생성 위해)
        Transform logPanel = hudCanvas.Find("Backend_LogPanel");
        if (logPanel != null) DestroyImmediate(logPanel.gameObject);
        
        // 4. PlayerID_Text 하위에 이미 붙어있는 Backend_PID_Text 제거
        Transform pIdTextInGroup = playerInfoGroup.Find("PlayerID_Text");
        if (pIdTextInGroup != null)
        {
            Transform childPid = pIdTextInGroup.Find("Backend_PID_Text");
            if (childPid != null) DestroyImmediate(childPid.gameObject);
        }
    }

    /// <summary>
    /// PlayerID_Text의 자식으로 Backend_PID_Text를 생성하여 우측에 배치
    /// </summary>
    private static void CreateBackendPIDText(Transform playerInfoGroup)
    {
        // 1. PlayerID_Text 찾기
        Transform playerIdTextObj = playerInfoGroup.Find("PlayerID_Text");
        if (playerIdTextObj == null)
        {
            Debug.LogError("[Setup] PlayerID_Text를 찾을 수 없습니다.");
            return;
        }

        // 2. [핵심] PlayerInfo_Group의 ChildControlWidth 끄기
        // 그래야 PlayerID_Text가 자신의 ContentSizeFitter대로 줄어들 수 있음
        VerticalLayoutGroup parentVLG = playerInfoGroup.GetComponent<VerticalLayoutGroup>();
        if (parentVLG != null)
        {
            parentVLG.childControlWidth = false; 
            // childControlHeight는 true/false 상관없지만, 세로 정렬이므로 텍스트 높이만큼만 쓰려면 false가 나을수도 있음
            // 보통 폰트사이즈에 맞게 높이가 결정되므로 둠
        }

        // 3. PlayerID_Text에 ContentSizeFitter 추가
        ContentSizeFitter csf = playerIdTextObj.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = playerIdTextObj.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // RectTransform 초기화 (Anchor/Pivot 변경 없이 사이즈만 Auto)
        RectTransform pIdRect = playerIdTextObj.GetComponent<RectTransform>();
        // Vertical Layout Group 안에 있으므로 위치값은 VLG가 제어함

        // 4. Backend_PID_Text 생성 (PlayerID_Text의 자식)
        GameObject textObj = new GameObject("Backend_PID_Text");
        textObj.transform.SetParent(playerIdTextObj, false);

        // TextMeshProUGUI 추가
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = " (PID: ...)";
        tmpText.fontSize = 14; 
        tmpText.color = new Color(0.8f, 0.8f, 0.8f); // 약간 회색
        tmpText.alignment = TextAlignmentOptions.Left; // 왼쪽 정렬
        tmpText.enableAutoSizing = false;
        tmpText.raycastTarget = false;

        // NotoSansKR SDF 폰트 로드
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Fonts/NOTOSANSKR-VF SDF.asset"
        );
        if (fontAsset != null) tmpText.font = fontAsset;

        // ContentSizeFitter 추가
        ContentSizeFitter childCsf = textObj.AddComponent<ContentSizeFitter>();
        childCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        childCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 5. [핵심] 위치 설정 (Parent의 오른쪽 끝에 붙이기)
        RectTransform rt = textObj.GetComponent<RectTransform>();
        
        // Anchor: Right Center
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        
        // Pivot: Left Center
        rt.pivot = new Vector2(0, 0.5f);
        
        // Pos: 10px 띄우고
        rt.anchoredPosition = new Vector2(10, 0);

        Debug.Log("[Setup] Backend_PID_Text 생성 완료 (Child 방식)");
    }

    private static void CreateBackendLogPanel(Transform hudCanvas)
    {
        // LogPanel 생성 로직 (기존과 동일)
        GameObject panelObj = new GameObject("Backend_LogPanel");
        panelObj.transform.SetParent(hudCanvas, false);

        Image bg = panelObj.AddComponent<Image>();
        bg.color = COLOR_LOG_BG;

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0.35f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CanvasGroup cg = panelObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;

        GameObject logTextObj = new GameObject("LogText");
        logTextObj.transform.SetParent(panelObj.transform, false);

        TextMeshProUGUI logText = logTextObj.AddComponent<TextMeshProUGUI>();
        logText.text = "";
        logText.fontSize = 18;
        logText.color = COLOR_GREEN;
        logText.alignment = TextAlignmentOptions.BottomLeft;
        logText.overflowMode = TextOverflowModes.Truncate;
        logText.enableAutoSizing = false;
        logText.raycastTarget = false;

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Fonts/NOTOSANSKR-VF SDF.asset"
        );
        if (fontAsset != null) logText.font = fontAsset;

        RectTransform logTextRect = logTextObj.GetComponent<RectTransform>();
        logTextRect.anchorMin = Vector2.zero;
        logTextRect.anchorMax = Vector2.one;
        logTextRect.offsetMin = new Vector2(10, 10);
        logTextRect.offsetMax = new Vector2(-10, -10);

        GameObject hintTextObj = new GameObject("HintText");
        hintTextObj.transform.SetParent(panelObj.transform, false);

        TextMeshProUGUI hintText = hintTextObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "Log: F1";
        hintText.fontSize = 20;
        hintText.color = COLOR_WHITE;
        hintText.alignment = TextAlignmentOptions.TopRight;
        hintText.enableAutoSizing = false;
        hintText.raycastTarget = false;
        if (fontAsset != null) hintText.font = fontAsset;

        RectTransform hintRect = hintTextObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(1, 1);
        hintRect.anchorMax = new Vector2(1, 1);
        hintRect.pivot = new Vector2(1, 1);
        hintRect.anchoredPosition = new Vector2(-10, -10);
        hintRect.sizeDelta = new Vector2(100, 30);

        panelObj.SetActive(false);
    }
}
