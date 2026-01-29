using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UI;

/// <summary>
/// HUD에 유저 데이터 표시 UI를 자동으로 생성하는 Editor 툴
/// Tools > Setup HUD User Data Display 메뉴로 실행
/// </summary>
public class HUDUserDataDisplaySetup : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // 색상 정의
    private static readonly Color COLOR_WHITE = Color.white;
    private static readonly Color COLOR_GOLD = new Color32(255, 215, 0, 255); // #FFD700
    private static readonly Color COLOR_CYAN = new Color32(0, 255, 255, 255); // #00FFFF
    private static readonly Color COLOR_GREEN = new Color32(76, 175, 80, 255); // #4CAF50
    private static readonly Color COLOR_BLACK_TRANSPARENT = new Color(0, 0, 0, 0.8f); // 80% Alpha

    [MenuItem("Tools/Setup HUD User Data Display")]
    public static void SetupHUDUserDataDisplay()
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

        // 2. 기존 요소가 있는지 확인 (중복 생성 방지)
        bool hasExisting = topPanel.Find("PlayerInfo_Group") != null ||
                          topPanel.Find("ExpBar_Group") != null;

        if (hasExisting)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "확인",
                "이미 유저 데이터 표시 UI가 존재합니다.\n기존 요소를 삭제하고 다시 생성하시겠습니까?",
                "예",
                "아니오"
            );

            if (!overwrite)
            {
                return;
            }

            // 기존 요소 삭제
            Transform existing = topPanel.Find("PlayerInfo_Group");
            if (existing != null) DestroyImmediate(existing.gameObject);

            existing = topPanel.Find("ExpBar_Group");
            if (existing != null) DestroyImmediate(existing.gameObject);
        }

        // 3. UI 요소 생성
        CreatePlayerInfoGroup(topPanel);
        CreateExpBarGroup(topPanel);
        SetupCurrencyPanels(topPanel);

        // 4. HUDUserDataDisplay 컴포넌트 추가 및 연결
        SetupHUDUserDataDisplayComponent(topPanel.gameObject);

        // 5. PlayerDataManager 확인
        CheckPlayerDataManager();

        // 완료 메시지
        EditorUtility.DisplayDialog(
            "완료",
            "HUD 유저 데이터 표시 UI가 성공적으로 생성되었습니다!\n\n" +
            "다음 단계:\n" +
            "1. Play Mode로 진입\n" +
            "2. HUDUserDataDisplay 컴포넌트에서 우클릭 > Test 메뉴로 테스트\n" +
            "3. CheatManager로 골드/젬/경험치 조작 가능",
            "확인"
        );

        Debug.Log("[HUDUserDataDisplaySetup] UI 생성 완료!");
    }

    /// <summary>
    /// PlayerInfo_Group 생성 (ID, 레벨 표시)
    /// </summary>
    private static void CreatePlayerInfoGroup(Transform parent)
    {
        // PlayerInfo_Group 컨테이너
        GameObject groupObj = new GameObject("PlayerInfo_Group");
        groupObj.transform.SetParent(parent, false);

        RectTransform groupRect = groupObj.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0, 1); // Top-Left
        groupRect.anchorMax = new Vector2(0, 1);
        groupRect.pivot = new Vector2(0, 1);
        groupRect.anchoredPosition = new Vector2(20, -20); // 좌측 상단에서 20px 여백
        groupRect.sizeDelta = new Vector2(150, 80);

        // Vertical Layout Group 추가
        VerticalLayoutGroup layout = groupObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // PlayerID_Text 생성
        GameObject playerIdObj = new GameObject("PlayerID_Text");
        playerIdObj.transform.SetParent(groupObj.transform, false);

        TextMeshProUGUI playerIdText = playerIdObj.AddComponent<TextMeshProUGUI>();
        playerIdText.text = "ID: Player";
        playerIdText.fontSize = 14;
        playerIdText.color = COLOR_WHITE;
        playerIdText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform playerIdRect = playerIdObj.GetComponent<RectTransform>();
        playerIdRect.sizeDelta = new Vector2(150, 20);

        // Level_Text 생성
        GameObject levelObj = new GameObject("Level_Text");
        levelObj.transform.SetParent(groupObj.transform, false);

        TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "Lv. 1";
        levelText.fontSize = 16;
        levelText.color = COLOR_GOLD;
        levelText.alignment = TextAlignmentOptions.TopLeft;
        levelText.fontStyle = FontStyles.Bold;

        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.sizeDelta = new Vector2(150, 22);

        Debug.Log("[Setup] PlayerInfo_Group 생성 완료");
    }

    /// <summary>
    /// ExpBar_Group 생성 (경험치 바)
    /// </summary>
    private static void CreateExpBarGroup(Transform parent)
    {
        // ExpBar_Group 컨테이너
        GameObject groupObj = new GameObject("ExpBar_Group");
        groupObj.transform.SetParent(parent, false);

        RectTransform groupRect = groupObj.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0, 1); // Top-Left
        groupRect.anchorMax = new Vector2(1, 1); // Top-Right (Stretch Horizontal)
        groupRect.pivot = new Vector2(0.5f, 1);
        groupRect.anchoredPosition = new Vector2(0, -20);
        groupRect.offsetMin = new Vector2(200, groupRect.offsetMin.y); // Left: 200px
        groupRect.offsetMax = new Vector2(-200, groupRect.offsetMax.y); // Right: -200px
        groupRect.sizeDelta = new Vector2(0, 30); // Height만 지정

        // ExpBar_Background 생성
        GameObject bgObj = new GameObject("ExpBar_Background");
        bgObj.transform.SetParent(groupObj.transform, false);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = COLOR_BLACK_TRANSPARENT;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // ExpBar_Fill 생성
        GameObject fillObj = new GameObject("ExpBar_Fill");
        fillObj.transform.SetParent(groupObj.transform, false);

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = COLOR_GREEN;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0.5f; // 테스트용 50%

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2, 2); // 2px 내부 여백
        fillRect.offsetMax = new Vector2(-2, -2);

        // ExpBar_Text 생성
        GameObject textObj = new GameObject("ExpBar_Text");
        textObj.transform.SetParent(groupObj.transform, false);

        TextMeshProUGUI expText = textObj.AddComponent<TextMeshProUGUI>();
        expText.text = "0 / 100";
        expText.fontSize = 12;
        expText.color = COLOR_WHITE;
        expText.alignment = TextAlignmentOptions.Center;
        expText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Debug.Log("[Setup] ExpBar_Group 생성 완료");
    }

    /// <summary>
    /// b16, b17 패널에 텍스트 추가 (Gem, Gold)
    /// </summary>
    private static void SetupCurrencyPanels(Transform parent)
    {
        // b16 (Gem Panel) 설정
        Transform b16 = parent.Find("b16");
        if (b16 != null)
        {
            // 기존 Gem_Text가 있으면 삭제
            Transform existingGem = b16.Find("Gem_Text");
            if (existingGem != null) DestroyImmediate(existingGem.gameObject);

            GameObject gemTextObj = new GameObject("Gem_Text");
            gemTextObj.transform.SetParent(b16, false);

            TextMeshProUGUI gemText = gemTextObj.AddComponent<TextMeshProUGUI>();
            gemText.text = "젬: 0";
            gemText.fontSize = 14;
            gemText.color = COLOR_CYAN;
            gemText.alignment = TextAlignmentOptions.Center;
            gemText.fontStyle = FontStyles.Bold;

            RectTransform gemRect = gemTextObj.GetComponent<RectTransform>();
            gemRect.anchorMin = Vector2.zero;
            gemRect.anchorMax = Vector2.one;
            gemRect.offsetMin = new Vector2(10, 10);
            gemRect.offsetMax = new Vector2(-10, -10);

            Debug.Log("[Setup] b16 > Gem_Text 생성 완료");
        }
        else
        {
            Debug.LogWarning("[Setup] b16 패널을 찾을 수 없습니다.");
        }

        // b17 (Gold Panel) 설정
        Transform b17 = parent.Find("b17");
        if (b17 != null)
        {
            // 기존 Gold_Text가 있으면 삭제
            Transform existingGold = b17.Find("Gold_Text");
            if (existingGold != null) DestroyImmediate(existingGold.gameObject);

            GameObject goldTextObj = new GameObject("Gold_Text");
            goldTextObj.transform.SetParent(b17, false);

            TextMeshProUGUI goldText = goldTextObj.AddComponent<TextMeshProUGUI>();
            goldText.text = "골드: 0";
            goldText.fontSize = 14;
            goldText.color = COLOR_GOLD;
            goldText.alignment = TextAlignmentOptions.Center;
            goldText.fontStyle = FontStyles.Bold;

            RectTransform goldRect = goldTextObj.GetComponent<RectTransform>();
            goldRect.anchorMin = Vector2.zero;
            goldRect.anchorMax = Vector2.one;
            goldRect.offsetMin = new Vector2(10, 10);
            goldRect.offsetMax = new Vector2(-10, -10);

            Debug.Log("[Setup] b17 > Gold_Text 생성 완료");
        }
        else
        {
            Debug.LogWarning("[Setup] b17 패널을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// HUDUserDataDisplay 컴포넌트 추가 및 필드 자동 연결
    /// </summary>
    private static void SetupHUDUserDataDisplayComponent(GameObject topPanel)
    {
        // 기존 컴포넌트가 있으면 제거
        HUDUserDataDisplay existingComponent = topPanel.GetComponent<HUDUserDataDisplay>();
        if (existingComponent != null)
        {
            DestroyImmediate(existingComponent);
        }

        // 새 컴포넌트 추가
        HUDUserDataDisplay component = topPanel.AddComponent<HUDUserDataDisplay>();

        // SerializedObject를 사용하여 private 필드에 접근
        SerializedObject serializedObject = new SerializedObject(component);

        // 필드 찾기 및 연결
        Transform playerInfoGroup = topPanel.transform.Find("PlayerInfo_Group");
        Transform expBarGroup = topPanel.transform.Find("ExpBar_Group");
        Transform b16 = topPanel.transform.Find("b16");
        Transform b17 = topPanel.transform.Find("b17");

        // goldText 연결
        if (b17 != null)
        {
            Transform goldText = b17.Find("Gold_Text");
            if (goldText != null)
            {
                SerializedProperty goldTextProp = serializedObject.FindProperty("goldText");
                goldTextProp.objectReferenceValue = goldText.GetComponent<TextMeshProUGUI>();
            }
        }

        // gemText 연결
        if (b16 != null)
        {
            Transform gemText = b16.Find("Gem_Text");
            if (gemText != null)
            {
                SerializedProperty gemTextProp = serializedObject.FindProperty("gemText");
                gemTextProp.objectReferenceValue = gemText.GetComponent<TextMeshProUGUI>();
            }
        }

        // playerIdText 연결
        if (playerInfoGroup != null)
        {
            Transform playerIdText = playerInfoGroup.Find("PlayerID_Text");
            if (playerIdText != null)
            {
                SerializedProperty playerIdTextProp = serializedObject.FindProperty("playerIdText");
                playerIdTextProp.objectReferenceValue = playerIdText.GetComponent<TextMeshProUGUI>();
            }

            // levelText 연결
            Transform levelText = playerInfoGroup.Find("Level_Text");
            if (levelText != null)
            {
                SerializedProperty levelTextProp = serializedObject.FindProperty("levelText");
                levelTextProp.objectReferenceValue = levelText.GetComponent<TextMeshProUGUI>();
            }
        }

        // expBarFill 연결
        if (expBarGroup != null)
        {
            Transform expBarFill = expBarGroup.Find("ExpBar_Fill");
            if (expBarFill != null)
            {
                SerializedProperty expBarFillProp = serializedObject.FindProperty("expBarFill");
                expBarFillProp.objectReferenceValue = expBarFill.GetComponent<Image>();
            }

            // expText 연결
            Transform expText = expBarGroup.Find("ExpBar_Text");
            if (expText != null)
            {
                SerializedProperty expTextProp = serializedObject.FindProperty("expText");
                expTextProp.objectReferenceValue = expText.GetComponent<TextMeshProUGUI>();
            }
        }

        // 변경사항 적용
        serializedObject.ApplyModifiedProperties();

        Debug.Log("[Setup] HUDUserDataDisplay 컴포넌트 추가 및 필드 연결 완료");
    }

    /// <summary>
    /// PlayerDataManager 확인 및 생성
    /// </summary>
    private static void CheckPlayerDataManager()
    {
        GameObject existingManager = GameObject.Find("PlayerDataManager");

        if (existingManager == null)
        {
            bool createManager = EditorUtility.DisplayDialog(
                "PlayerDataManager 없음",
                "씬에 PlayerDataManager가 없습니다.\n자동으로 생성하시겠습니까?",
                "예",
                "아니오"
            );

            if (createManager)
            {
                GameObject managerObj = new GameObject("PlayerDataManager");
                managerObj.AddComponent<Backend.PlayerDataManager>();

                Debug.Log("[Setup] PlayerDataManager 생성 완료");
            }
            else
            {
                Debug.LogWarning("[Setup] PlayerDataManager가 없습니다. 수동으로 생성해주세요.");
            }
        }
        else
        {
            Debug.Log("[Setup] PlayerDataManager 확인 완료");
        }
    }
}
