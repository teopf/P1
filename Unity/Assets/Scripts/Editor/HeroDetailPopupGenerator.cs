using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 히어로 상세 팝업 UI 생성기
/// a1 메뉴의 AD101 스크롤 그리드에서 히어로 아이템 클릭 시 표시되는 팝업
/// </summary>
public class HeroDetailPopupGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // ========== COLORS ==========
    // 그라디언트 패널 색상
    private static readonly Color COLOR_GRADIENT_START = new Color32(107, 140, 255, 255); // #6B8CFF
    private static readonly Color COLOR_GRADIENT_END = new Color32(155, 107, 255, 255);   // #9B6BFF
    
    // 딤 배경
    private static readonly Color COLOR_DIMMER = new Color(0, 0, 0, 0.5f); // rgba(0,0,0,0.5)
    
    // 버튼/카드 색상
    private static readonly Color COLOR_BTN_ORANGE = new Color(1f, 0.533f, 0f, 0.25f);       // rgba(255,136,0,0.25)
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color(1f, 0.431f, 0f, 0.5f); // rgba(255,110,0,0.5)
    private static readonly Color COLOR_BTN_ORANGE_GLOW = new Color(1f, 0.533f, 0f, 0.35f);  // a99 back button
    private static readonly Color COLOR_CARD_GREEN = new Color(0.659f, 1f, 0.698f, 0.5f);    // rgba(168,255,178,0.5)
    private static readonly Color COLOR_STATS_RED = new Color(1f, 0f, 0f, 0.25f);            // rgba(255,0,0,0.25)
    private static readonly Color COLOR_STATS_RED_ACTIVE = new Color(1f, 0f, 0f, 0.4f);      // Active tab
    private static readonly Color COLOR_BTN_HUD = new Color(1f, 0f, 0f, 0.25f);              // HUD buttons
    private static readonly Color COLOR_GOLD_BORDER = new Color32(255, 215, 0, 255);         // #FFD700

    // ========== POSITIONS ==========
    // Safe Area
    private const float SAFE_TOP = 63f;
    private const float SAFE_BOTTOM = 100f;
    
    // AA101 Top Panel
    private const float AA101_Y = -63f;  // Safe area offset
    private const float AA101_HEIGHT = 90f;
    
    // Hero Preview Section
    private const float AAD111_X = 59f;
    private const float AAD111_Y = -182f;  // From AA101 bottom
    private const float AAD111_WIDTH = 200f;
    private const float AAD111_HEIGHT = 200f;
    
    private const float AAD112_X = 367f;
    private const float AAD112_Y = -182f;
    private const float AAD112_WIDTH = 350f;
    private const float AAD112_HEIGHT = 200f;
    
    private const float AAD113_X = 140f;
    private const float AAD113_Y = -409f;
    private const float AAD113_WIDTH = 800f;
    private const float AAD113_HEIGHT = 800f;
    
    // Stats Tabs
    private const float STATS_TAB_Y = -1306f;
    private const float STATS_TAB_HEIGHT = 80f;
    private static readonly float[] STATS_TAB_X = { 8f, 280f, 552f, 824f };
    private const float STATS_TAB_WIDTH = 240f;
    
    // Stats Panels
    private const float STATS_PANEL_Y = -1408f;
    private const float AAB101_X = 8f;
    private const float AAB101_WIDTH = 416f;
    private const float AAB101_HEIGHT = 200f;
    private const float AAB102_X = 440f;
    private const float AAB102_WIDTH = 625f;
    private const float AAB102_HEIGHT = 200f;
    
    // AA102 Hero Navigation
    private const float AA102_Y = -1641f;
    private const float AA102_HEIGHT = 90f;
    private static readonly float[] TOGGLE_X_PERCENT = { 0.0157f, 0.2574f, 0.5019f, 0.7444f };
    private static readonly float[] TOGGLE_WIDTH = { 261f, 262f, 262f, 261f };
    
    // AB1 Bottom HUD
    private const float AB1_Y = 0f;  // Bottom anchored
    private const float AB1_HEIGHT = 170f;

    [MenuItem("Tools/Generate Hero Detail Popup")]
    public static void Generate()
    {
        string canvasName = "Canvas_HeroDetailPopup";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        // Canvas 생성
        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 35; // a1 메뉴(20) 위, HUD 관련 팝업으로 설정

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false); // 초기 비활성화

        Sprite roundedSprite = CreateRoundedSprite(12);
        Sprite roundedSprite16 = CreateRoundedSprite(16);

        // 메인 컨테이너
        GameObject container = CreateRect("HeroDetailPopup_Container", canvasObj.transform);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // 1. 딤 배경 (클릭 시 닫기)
        CreateDimmerBackground(container.transform);

        // 2. 메인 콘텐츠 패널 (그라디언트 배경)
        GameObject mainContent = CreateGradientPanel("MainContent_Panel", container.transform);
        RectTransform mainRT = mainContent.GetComponent<RectTransform>();
        mainRT.anchorMin = Vector2.zero;
        mainRT.anchorMax = Vector2.one;
        mainRT.offsetMin = new Vector2(0, AB1_HEIGHT); // AB1 위
        mainRT.offsetMax = new Vector2(0, -SAFE_TOP);  // Safe area

        // 3. AA101 상단 패널
        CreateAA101TopPanel(mainContent.transform, roundedSprite);

        // 4. 히어로 프리뷰 섹션
        CreateHeroPreviewSection(mainContent.transform, roundedSprite, roundedSprite16);

        // 5. 스탯 탭 섹션
        CreateStatsTabs(mainContent.transform, roundedSprite);

        // 6. 스탯 패널
        CreateStatsPanels(mainContent.transform, roundedSprite);

        // 7. AA102 히어로 네비게이션
        CreateAA102HeroNavigation(mainContent.transform, roundedSprite);

        // 8. AB1 하단 HUD (별도 - 메인 콘텐츠 외부)
        CreateAB1BottomHUD(container.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Hero Detail Popup");
        Selection.activeGameObject = canvasObj;
        Debug.Log("히어로 상세 팝업 UI가 성공적으로 생성되었습니다!");
    }

    // ========== UI 생성 메서드 ==========

    private static void CreateDimmerBackground(Transform parent)
    {
        GameObject dimmer = CreateRect("Dimmer_Background", parent);
        RectTransform rt = dimmer.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = dimmer.AddComponent<Image>();
        img.color = COLOR_DIMMER;
        img.raycastTarget = true; // 클릭 감지용

        // 버튼 컴포넌트 추가 (클릭 시 닫기 동작)
        Button btn = dimmer.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
    }

    private static void CreateAA101TopPanel(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA101_SharedTopPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, AA101_HEIGHT);

        // b101 닫기 버튼 (오른쪽 정렬)
        GameObject btn = CreateButton("b101", panel.transform, sprite, COLOR_BTN_ORANGE, "X");
        RectTransform btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 0.5f);
        btnRT.anchorMax = new Vector2(1, 0.5f);
        btnRT.pivot = new Vector2(1, 0.5f);
        btnRT.anchoredPosition = new Vector2(-27, 0);
        btnRT.sizeDelta = new Vector2(90, 90);
        AddButtonShadow(btn);
    }

    private static void CreateHeroPreviewSection(Transform parent, Sprite sprite, Sprite sprite16)
    {
        // AAD111 레벨 카드
        GameObject levelCard = CreateImage("AAD111_LevelCard", parent, sprite, COLOR_CARD_GREEN);
        SetRectTopLeft(levelCard, AAD111_X, AAD111_Y - AA101_HEIGHT, AAD111_WIDTH, AAD111_HEIGHT);
        AddCardStyling(levelCard);
        
        // 레벨 라벨
        CreateTMPText("Lbl_Level", levelCard.transform, "Level", 
            new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(180, 40), 32, Color.black);
        CreateTMPText("Txt_LevelValue", levelCard.transform, "45",
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(180, 80), 64, Color.black, FontStyles.Bold);

        // AAD112 이름 카드
        GameObject nameCard = CreateImage("AAD112_NameCard", parent, sprite, COLOR_CARD_GREEN);
        SetRectTopLeft(nameCard, AAD112_X, AAD112_Y - AA101_HEIGHT, AAD112_WIDTH, AAD112_HEIGHT);
        AddCardStyling(nameCard);
        
        // 히어로 이름
        CreateTMPText("Txt_HeroName", nameCard.transform, "영웅 이름",
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(320, 60), 48, Color.black, FontStyles.Bold);
        // 클래스/타입
        CreateTMPText("Txt_HeroClass", nameCard.transform, "전사",
            new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(320, 40), 32, Color.gray);

        // AAD113 히어로 이미지
        GameObject heroImage = CreateImage("AAD113_HeroImage", parent, sprite16, COLOR_CARD_GREEN);
        SetRectTopLeft(heroImage, AAD113_X, AAD113_Y - AA101_HEIGHT, AAD113_WIDTH, AAD113_HEIGHT);
        AddCardStyling(heroImage, 16);
        
        // 플레이스홀더 텍스트
        CreateTMPText("Txt_Placeholder", heroImage.transform, "캐릭터\n이미지",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 200), 64, new Color(0, 0, 0, 0.3f));
    }

    private static void CreateStatsTabs(Transform parent, Sprite sprite)
    {
        GameObject tabContainer = CreateRect("StatsTabs_Container", parent);
        SetRectTopLeft(tabContainer, 0, STATS_TAB_Y, REF_WIDTH, STATS_TAB_HEIGHT);

        ToggleGroup toggleGroup = tabContainer.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false;

        string[] tabNames = { "AAT101", "AAT102", "AAT103", "AAT109" };
        string[] tabLabels = { "기본", "전투", "특수", "스킬" };

        for (int i = 0; i < tabNames.Length; i++)
        {
            GameObject tab = CreateImage(tabNames[i], tabContainer.transform, sprite, COLOR_STATS_RED);
            RectTransform tabRT = tab.GetComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(0, 0.5f);
            tabRT.anchorMax = new Vector2(0, 0.5f);
            tabRT.pivot = new Vector2(0, 0.5f);
            tabRT.anchoredPosition = new Vector2(STATS_TAB_X[i], 0);
            tabRT.sizeDelta = new Vector2(STATS_TAB_WIDTH, STATS_TAB_HEIGHT);

            Toggle toggle = tab.AddComponent<Toggle>();
            toggle.group = toggleGroup;
            toggle.targetGraphic = tab.GetComponent<Image>();
            toggle.isOn = (i == 0);

            ColorBlock cb = toggle.colors;
            cb.normalColor = COLOR_STATS_RED;
            cb.selectedColor = COLOR_STATS_RED_ACTIVE;
            cb.highlightedColor = COLOR_STATS_RED;
            cb.pressedColor = COLOR_STATS_RED_ACTIVE;
            toggle.colors = cb;

            AddButtonShadow(tab);
            CreateStretchedLabel("Label", tab.transform, tabLabels[i]);
        }
    }

    private static void CreateStatsPanels(Transform parent, Sprite sprite)
    {
        // AAB101 왼쪽 스탯 패널
        GameObject leftPanel = CreateImage("AAB101_LeftStatsPanel", parent, sprite, COLOR_STATS_RED);
        SetRectTopLeft(leftPanel, AAB101_X, STATS_PANEL_Y, AAB101_WIDTH, AAB101_HEIGHT);
        AddButtonShadow(leftPanel);

        // 스탯 목록
        string[] leftStats = { "HP: 2500", "ATK: 450", "DEF: 320", "SPD: 180" };
        for (int i = 0; i < leftStats.Length; i++)
        {
            CreateTMPText($"Stat_{i}", leftPanel.transform, leftStats[i],
                new Vector2(0.1f, 1f - (i + 1) * 0.2f), new Vector2(0.1f, 1f - (i + 1) * 0.2f), new Vector2(0, 0.5f),
                new Vector2(10, 0), new Vector2(380, 40), 28, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
        }

        // AAB102 오른쪽 스탯 패널
        GameObject rightPanel = CreateImage("AAB102_RightStatsPanel", parent, sprite, COLOR_STATS_RED);
        SetRectTopLeft(rightPanel, AAB102_X, STATS_PANEL_Y, AAB102_WIDTH, AAB102_HEIGHT);
        AddButtonShadow(rightPanel);

        // 추가 스탯 목록
        string[] rightStats = { "크리티컬: 25%", "크리 데미지: 150%", "명중률: 90%", "회피율: 15%" };
        for (int i = 0; i < rightStats.Length; i++)
        {
            CreateTMPText($"Stat_{i}", rightPanel.transform, rightStats[i],
                new Vector2(0.1f, 1f - (i + 1) * 0.2f), new Vector2(0.1f, 1f - (i + 1) * 0.2f), new Vector2(0, 0.5f),
                new Vector2(10, 0), new Vector2(580, 40), 28, Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
        }
    }

    private static void CreateAA102HeroNavigation(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA102_HeroNavPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, AA102_HEIGHT);

        ToggleGroup toggleGroup = panel.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false;

        string[] toggleNames = { "C101", "C102", "C103", "C104" };

        for (int i = 0; i < toggleNames.Length; i++)
        {
            GameObject toggleObj = CreateImage(toggleNames[i], panel.transform, sprite, COLOR_BTN_ORANGE);
            RectTransform toggleRT = toggleObj.GetComponent<RectTransform>();
            toggleRT.anchorMin = new Vector2(TOGGLE_X_PERCENT[i], 0.5f);
            toggleRT.anchorMax = new Vector2(TOGGLE_X_PERCENT[i], 0.5f);
            toggleRT.pivot = new Vector2(0, 0.5f);
            toggleRT.anchoredPosition = Vector2.zero;
            toggleRT.sizeDelta = new Vector2(TOGGLE_WIDTH[i], 90);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.group = toggleGroup;
            toggle.targetGraphic = toggleObj.GetComponent<Image>();
            toggle.isOn = (i == 0);

            ColorBlock cb = toggle.colors;
            cb.normalColor = COLOR_BTN_ORANGE;
            cb.selectedColor = COLOR_BTN_ORANGE_ACTIVE;
            cb.highlightedColor = COLOR_BTN_ORANGE;
            cb.pressedColor = COLOR_BTN_ORANGE_ACTIVE;
            toggle.colors = cb;

            AddButtonShadow(toggleObj);
            CreateStretchedLabel("Label", toggleObj.transform, toggleNames[i]);
        }
    }

    private static void CreateAB1BottomHUD(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AB1_BottomHUD", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, AB1_HEIGHT);

        string[] btnNames = { "a1", "a2", "a3", "a99", "a4", "a5", "a6" };
        float[] btnXPercent = { 0.0157f, 0.1454f, 0.275f, 0.4417f, 0.6074f, 0.738f, 0.8676f };
        float btnSize = 128f;

        for (int i = 0; i < btnNames.Length; i++)
        {
            bool isBackBtn = (btnNames[i] == "a99");
            Color btnColor = isBackBtn ? COLOR_BTN_ORANGE_GLOW : COLOR_BTN_HUD;
            string label = isBackBtn ? "←" : btnNames[i];

            GameObject btn = CreateButton(btnNames[i], panel.transform, sprite, btnColor, label);
            RectTransform btnRT = btn.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(btnXPercent[i], 0.5f);
            btnRT.anchorMax = new Vector2(btnXPercent[i], 0.5f);
            btnRT.pivot = new Vector2(0, 0.5f);
            btnRT.anchoredPosition = Vector2.zero;
            
            float scale = isBackBtn ? 1.1f : 1f;
            btnRT.sizeDelta = new Vector2(btnSize * scale, btnSize * scale);

            AddButtonShadow(btn);

            // a99 뒤로가기 버튼 특수 스타일링
            if (isBackBtn)
            {
                // Outline으로 골드 테두리 추가
                Outline outline = btn.AddComponent<Outline>();
                outline.effectColor = COLOR_GOLD_BORDER;
                outline.effectDistance = new Vector2(3, -3);
            }
        }
    }

    // ========== 헬퍼 메서드 ==========

    private static GameObject CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static GameObject CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = CreateRect(name, parent);
        Image img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = color;
        img.type = Image.Type.Sliced;
        return go;
    }

    private static GameObject CreateGradientPanel(string name, Transform parent)
    {
        GameObject go = CreateRect(name, parent);
        Image img = go.AddComponent<Image>();

        Texture2D tex = new Texture2D(1, 16);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < 16; i++)
        {
            tex.SetPixel(0, i, Color.Lerp(COLOR_GRADIENT_END, COLOR_GRADIENT_START, (float)i / 15f));
        }
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 16), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Sliced;

        return go;
    }

    private static void SetRectTopLeft(GameObject go, float x, float y, float w, float h)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static GameObject CreateButton(string name, Transform parent, Sprite sprite, Color color, string label)
    {
        GameObject go = CreateImage(name, parent, sprite, color);
        go.AddComponent<Button>();
        CreateStretchedLabel("Label", go.transform, label);
        return go;
    }

    private static void CreateStretchedLabel(string name, Transform parent, string content)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CreateTMPText(string name, Transform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 pos, Vector2 size, int fontSize, Color color,
        FontStyles fontStyle = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void AddButtonShadow(GameObject go)
    {
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
    }

    private static void AddCardStyling(GameObject go, int borderRadius = 12)
    {
        // 외곽선
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        // 그림자
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
    }

    private static Sprite CreateRoundedSprite(int radius)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                // 모서리 라운드 처리
                if (x < radius && y < radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius - 0.5f, radius - 0.5f)) <= radius;
                else if (x > size - 1 - radius && y < radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(size - 1 - radius + 0.5f, radius - 0.5f)) <= radius;
                else if (x < radius && y > size - 1 - radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius - 0.5f, size - 1 - radius + 0.5f)) <= radius;
                else if (x > size - 1 - radius && y > size - 1 - radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(size - 1 - radius + 0.5f, size - 1 - radius + 0.5f)) <= radius;
                
                colors[y * size + x] = inside ? Color.white : Color.clear;
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
