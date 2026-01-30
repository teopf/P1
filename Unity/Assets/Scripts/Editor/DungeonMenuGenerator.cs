using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// a4 던전 메뉴 UI 생성기
/// 하단 HUD의 a4 버튼 클릭 시 표시되는 던전 목록 메뉴
/// </summary>
public class DungeonMenuGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // ========== COLORS ==========
    private static readonly Color COLOR_GRADIENT_START = new Color32(107, 140, 255, 255); // #6B8CFF
    private static readonly Color COLOR_GRADIENT_END = new Color32(155, 107, 255, 255);   // #9B6BFF
    
    private static readonly Color COLOR_BTN_ORANGE = new Color(1f, 0.533f, 0f, 0.25f);       // rgba(255,136,0,0.25)
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color(1f, 0.431f, 0f, 0.5f); // rgba(255,110,0,0.5)
    private static readonly Color COLOR_CARD_RED = new Color(1f, 0f, 0f, 0.25f);             // rgba(255,0,0,0.25)

    // ========== POSITIONS ==========
    private const float SAFE_TOP = 63f;
    
    // AA101 Top Panel
    private const float AA101_HEIGHT = 90f;
    
    // Dungeon Cards
    private const float CARD_X = 67f;
    private const float CARD_WIDTH = 945f;
    private const float CARD_HEIGHT = 323f;
    private const float CARD_SPACING = 50f;
    private const float FIRST_CARD_Y = 191f;
    
    // DT Ticket Buttons (inside cards)
    private const float DT_X_IN_CARD = 24f;  // 91 - 67
    private const float DT_Y_IN_CARD = 197f;
    private const float DT_WIDTH = 333f;
    private const float DT_HEIGHT = 90f;
    
    // AA503 Category Toggles (하단 HUD 위에 위치)
    private const float AA503_HEIGHT = 90f;
    private static readonly float[] TOGGLE_X = { 17f, 278f, 542f, 804f };
    private static readonly float[] TOGGLE_W = { 261f, 262f, 262f, 261f };
    
    // AB1 Bottom HUD (공통 HUD에서 재활용, 여기서는 여백만 지정)
    private const float AB1_HEIGHT = 170f;

    [MenuItem("Tools/UI/Generate a4 Dungeon Menu")]
    public static void Generate()
    {
        string canvasName = "Canvas_a4_Dungeon";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        // Canvas 생성
        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false); // 초기 비활성화

        Sprite roundedSprite = CreateRoundedSprite(12);
        Sprite roundedSprite16 = CreateRoundedSprite(16);

        // 메인 컨테이너
        GameObject container = CreateRect("a4_MenuContainer", canvasObj.transform);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // 1. AA101 상단 패널
        CreateAA101TopPanel(container.transform, roundedSprite);

        // 2. AD501 스크롤 가능한 던전 목록
        CreateAD501ScrollList(container.transform, roundedSprite, roundedSprite16);

        // 3. AA503 카테고리 토글 패널
        CreateAA503CategoryToggles(container.transform, roundedSprite);

        // NOTE: AB1 하단 HUD는 HUD_Canvas에서 공통으로 재활용됨 (별도 생성하지 않음)

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create a4 Dungeon Menu");
        Selection.activeGameObject = canvasObj;
        Debug.Log("a4 던전 메뉴 UI가 성공적으로 생성되었습니다! (AB1 HUD는 공통 사용)");
    }

    // ========== UI 생성 메서드 ==========

    private static void CreateAA101TopPanel(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA101_SharedTopPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -SAFE_TOP);
        rt.sizeDelta = new Vector2(0, AA101_HEIGHT);

        // b101 닫기 버튼
        GameObject btn = CreateButton("b101", panel.transform, sprite, COLOR_BTN_ORANGE, "X");
        RectTransform btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 0.5f);
        btnRT.anchorMax = new Vector2(1, 0.5f);
        btnRT.pivot = new Vector2(1, 0.5f);
        btnRT.anchoredPosition = new Vector2(-27, 0);
        btnRT.sizeDelta = new Vector2(90, 90);
        AddButtonShadow(btn);
    }

    private static void CreateAD501ScrollList(Transform parent, Sprite sprite, Sprite sprite16)
    {
        // 스크롤 영역 - AA101 아래, AA503 위 (AB1 공간도 고려)
        float topOffset = SAFE_TOP + AA101_HEIGHT;
        float bottomOffset = AA503_HEIGHT + AB1_HEIGHT; // AB1 공간 확보
        
        GameObject scrollPanel = CreateGradientPanel("AD501_DungeonScrollList", parent);
        RectTransform scrollRT = scrollPanel.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = new Vector2(0, bottomOffset);
        scrollRT.offsetMax = new Vector2(0, -topOffset);

        ScrollRect sr = scrollPanel.AddComponent<ScrollRect>();
        sr.vertical = true;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.elasticity = 0.1f;
        sr.scrollSensitivity = 20;
        sr.decelerationRate = 0.135f;

        // Viewport
        GameObject viewport = CreateRect("Viewport", scrollPanel.transform);
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(1, 1, 1, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        // Content
        GameObject content = CreateRect("Content", viewport.transform);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        sr.content = contentRT;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(0, 0, 20, 20);
        vlg.spacing = CARD_SPACING;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // 던전 카드 생성 (D401-D410)
        for (int i = 1; i <= 10; i++)
        {
            CreateDungeonCard(content.transform, sprite, sprite16, i);
        }

        // 스크롤바
        GameObject scrollbar = CreateRect("Scrollbar", scrollPanel.transform);
        RectTransform sbRT = scrollbar.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 0.5f);
        sbRT.anchoredPosition = Vector2.zero;
        sbRT.sizeDelta = new Vector2(8, 0);
        
        Image sbImg = scrollbar.AddComponent<Image>();
        sbImg.color = new Color(1, 1, 1, 0.3f);
        
        Scrollbar sb = scrollbar.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
    }

    private static void CreateDungeonCard(Transform parent, Sprite sprite, Sprite sprite16, int index)
    {
        string cardName = $"D{400 + index}";
        string ticketName = $"DT{100 + index}";
        
        // 메인 카드
        GameObject card = CreateImage(cardName, parent, sprite16, COLOR_CARD_RED);
        RectTransform cardRT = card.GetComponent<RectTransform>();
        cardRT.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);
        
        LayoutElement le = card.AddComponent<LayoutElement>();
        le.preferredWidth = CARD_WIDTH;
        le.preferredHeight = CARD_HEIGHT;
        
        AddCardShadow(card);
        
        // 카드 버튼 컴포넌트
        card.AddComponent<Button>();

        // 던전 썸네일 영역 (상단)
        GameObject thumbnail = CreateImage("Thumbnail", card.transform, null, new Color(0, 0, 0, 0.2f));
        RectTransform thumbRT = thumbnail.GetComponent<RectTransform>();
        thumbRT.anchorMin = new Vector2(0, 0.4f);
        thumbRT.anchorMax = new Vector2(1, 1);
        thumbRT.offsetMin = new Vector2(10, 0);
        thumbRT.offsetMax = new Vector2(-10, -10);
        
        // 던전 이름
        CreateTMPText("Txt_DungeonName", card.transform, $"던전 {index}",
            new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(800, 60), 48, Color.white, FontStyles.Bold);
        
        // 난이도 표시
        CreateTMPText("Txt_Difficulty", card.transform, "★★★ Hard",
            new Vector2(0.9f, 0.9f), new Vector2(0.9f, 0.9f), new Vector2(1, 1),
            Vector2.zero, new Vector2(200, 40), 28, new Color(1f, 0.5f, 0f));
        
        // 추천 전투력
        CreateTMPText("Txt_Power", card.transform, "추천: 12,000+",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 35), 28, new Color(0.8f, 0.8f, 0.8f));

        // DT 티켓 버튼
        GameObject ticketBtn = CreateImage(ticketName, card.transform, sprite, COLOR_BTN_ORANGE);
        RectTransform ticketRT = ticketBtn.GetComponent<RectTransform>();
        ticketRT.anchorMin = new Vector2(0, 0);
        ticketRT.anchorMax = new Vector2(0, 0);
        ticketRT.pivot = new Vector2(0, 0);
        ticketRT.anchoredPosition = new Vector2(DT_X_IN_CARD, 10);
        ticketRT.sizeDelta = new Vector2(DT_WIDTH, DT_HEIGHT);
        
        ticketBtn.AddComponent<Button>();
        AddButtonShadow(ticketBtn);
        
        // 티켓 텍스트
        CreateTMPText("Txt_Ticket", ticketBtn.transform, "입장권: 5/10",
            new Vector2(0.4f, 0.5f), new Vector2(0.4f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(200, 50), 32, Color.white);
        
        // 에너지 비용
        CreateTMPText("Txt_Energy", ticketBtn.transform, "⚡ 20",
            new Vector2(0.85f, 0.5f), new Vector2(0.85f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(100, 50), 32, Color.yellow);
    }

    private static void CreateAA503CategoryToggles(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA503_CategoryTogglePanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, AB1_HEIGHT); // AB1 HUD 위에 배치
        rt.sizeDelta = new Vector2(0, AA503_HEIGHT);

        ToggleGroup toggleGroup = panel.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false;

        string[] toggleNames = { "C501", "C502", "C503", "C504" };
        string[] toggleLabels = { "전체", "일일", "이벤트", "레이드" };

        for (int i = 0; i < toggleNames.Length; i++)
        {
            GameObject toggleObj = CreateImage(toggleNames[i], panel.transform, sprite, COLOR_BTN_ORANGE);
            RectTransform toggleRT = toggleObj.GetComponent<RectTransform>();
            toggleRT.anchorMin = new Vector2(0, 0.5f);
            toggleRT.anchorMax = new Vector2(0, 0.5f);
            toggleRT.pivot = new Vector2(0, 0.5f);
            toggleRT.anchoredPosition = new Vector2(TOGGLE_X[i], 0);
            toggleRT.sizeDelta = new Vector2(TOGGLE_W[i], 90);

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
            CreateStretchedLabel("Label", toggleObj.transform, toggleLabels[i]);
        }
    }

    // NOTE: CreateAB1BottomHUD 메서드 제거됨
    // AB1 하단 HUD는 HUD_Canvas에서 공통으로 재활용됨

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

    private static void AddCardShadow(GameObject go)
    {
        // 외곽선
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        // 그림자
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.25f);
        shadow.effectDistance = new Vector2(0, -6);
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
