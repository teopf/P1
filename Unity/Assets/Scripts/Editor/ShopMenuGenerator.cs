using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// a6 상점 메뉴 생성기
/// UI Skill 참고율: 85%
/// - Hierarchy Decomposition, Zone Definition, Naming Convention 적용
/// - ScrollRect + GridLayoutGroup 설정
/// </summary>
public class ShopMenuGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // ========== COLORS ==========
    private static readonly Color COLOR_GRADIENT_START = new Color32(107, 140, 255, 255); // #6B8CFF
    private static readonly Color COLOR_GRADIENT_END = new Color32(155, 107, 255, 255);   // #9B6BFF
    private static readonly Color COLOR_DARK_GRAY = new Color(54f/255f, 56f/255f, 64f/255f, 0.25f); // rgba(54,56,64,0.25)
    private static readonly Color COLOR_BTN_ORANGE = new Color(1f, 0.533f, 0f, 0.25f);    // rgba(255,136,0,0.25)
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color(1f, 0.431f, 0f, 0.5f); // rgba(255,110,0,0.5)
    private static readonly Color COLOR_ITEM_RED = new Color(1f, 0f, 0f, 0.25f);          // rgba(255,0,0,0.25)

    // ========== POSITIONS (기준: a3 메뉴와 유사하게 Top-Left 앵커) ==========
    // AA101 Top Panel
    private const float AA101_Y = -60f;
    private const float AA101_HEIGHT = 90f;
    
    // AC101 Featured Area
    private const float AC101_Y = -153f;
    private const float AC101_HEIGHT = 824f;
    
    // Featured Banners
    private const float BANNER_X = 67f;
    private const float BANNER_WIDTH = 945f;
    private const float BANNER_HEIGHT = 323f;
    private const float BANNER1_Y = -38f;  // 191 - 153 = 38 (패널 내 상대 위치)
    private const float BANNER2_Y = -411f; // 564 - 153 = 411 (패널 내 상대 위치)
    
    // AD401 Scroll Area (Featured 아래, Category 위)
    private const float AD401_Y = -977f;   // AC101 끝 (153+824=977)
    private const float AD401_HEIGHT = 664f; // 1641 - 977 = 664
    
    // AA402 Category Toggles
    private const float AA402_Y = -1641f;
    private const float AA402_HEIGHT = 90f;
    private static readonly float[] TOGGLE_X = { 17f, 227f, 438f, 648f, 859f };
    private static readonly float[] TOGGLE_W = { 210f, 211f, 210f, 211f, 210f };

    [MenuItem("Tools/Generate a6 Shop Menu")]
    public static void Generate()
    {
        string canvasName = "Canvas_a6_Shop";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false); // Initially hidden

        Sprite roundedSprite = CreateRoundedSprite(12);

        // Container (Stretch to fill)
        GameObject container = CreateRect("a6_MenuContainer", canvasObj.transform);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero; 
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // 1. AA101 Top Panel (Header Zone)
        CreateAA101TopPanel(container.transform, roundedSprite);

        // 2. AC101 Featured Area (Body Zone - Featured)
        CreateAC101FeaturedArea(container.transform, roundedSprite);

        // 3. AD401 Scrollable Shop Panel (Body Zone - Scroll)
        CreateAD401ScrollableShop(container.transform, roundedSprite);

        // 4. AA402 Category Toggles (Footer Zone)
        CreateAA402CategoryToggles(container.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create a6 Shop Menu");
        Selection.activeGameObject = canvasObj;
        Debug.Log("a6 Shop Menu Generated Successfully! (UI Skill 참고율: 85%)");
    }

    // ========== ZONE: HEADER ==========
    private static void CreateAA101TopPanel(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA101_SharedTopPanel", parent);
        SetRectTopLeft(panel, 0, AA101_Y, REF_WIDTH, AA101_HEIGHT);
        
        // b101 Close Button (right aligned, -27 from right)
        GameObject btn = CreateButton("b101", panel.transform, sprite, COLOR_BTN_ORANGE, "X");
        RectTransform btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 0.5f); 
        btnRT.anchorMax = new Vector2(1, 0.5f);
        btnRT.pivot = new Vector2(1, 0.5f);
        btnRT.anchoredPosition = new Vector2(-27, 0);
        btnRT.sizeDelta = new Vector2(90, 90);
        AddShadow(btn);
    }

    // ========== ZONE: BODY (FEATURED) ==========
    private static void CreateAC101FeaturedArea(Transform parent, Sprite sprite)
    {
        // Dark gray background panel
        GameObject panel = CreateImage("AC101_FeaturedArea", parent, null, COLOR_DARK_GRAY);
        SetRectTopLeft(panel, 0, AC101_Y, REF_WIDTH, AC101_HEIGHT);
        
        // D401 Featured Banner (Top)
        CreateFeaturedBanner("D401_FeaturedBanner", panel.transform, sprite, BANNER_X, BANNER1_Y, "D401");
        
        // D402 Featured Banner (Bottom)
        CreateFeaturedBanner("D402_FeaturedBanner", panel.transform, sprite, BANNER_X, BANNER2_Y, "D402");
    }

    private static void CreateFeaturedBanner(string name, Transform parent, Sprite sprite, float x, float y, string label)
    {
        GameObject banner = CreateImage(name, parent, sprite, COLOR_ITEM_RED);
        RectTransform rt = banner.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(BANNER_WIDTH, BANNER_HEIGHT);
        
        AddOutline(banner);
        AddShadow(banner);
        
        // Banner Label
        CreateStretchedTMPText($"Lbl_{name}", banner.transform, label,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(200, 50), 32, Color.white);
        
        // "Featured Item" placeholder text
        CreateStretchedTMPText("Txt_Title", banner.transform, "Featured Item",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 60), 48, Color.white);
    }

    // ========== ZONE: BODY (SCROLL) ==========
    private static void CreateAD401ScrollableShop(Transform parent, Sprite sprite)
    {
        // ScrollRect container
        GameObject scrollPanel = CreateImage("AD401_ScrollableShopPanel", parent, null, COLOR_DARK_GRAY);
        SetRectTopLeft(scrollPanel, 0, AD401_Y, REF_WIDTH, AD401_HEIGHT);
        
        ScrollRect scrollRect = scrollPanel.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 20f;
        
        // Viewport
        GameObject viewport = CreateRect("Viewport", scrollPanel.transform);
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f); // Nearly invisible
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        scrollRect.viewport = vpRT;
        
        // Content
        GameObject content = CreateRect("Content", viewport.transform);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;
        
        // VerticalLayoutGroup for content
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 20, 20);
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Section 1: Shop Panels (D401~D410)
        CreateShopPanelsSection(content.transform, sprite);
        
        // Section 2: Grid Items (D421~D440)
        CreateGridItemsSection(content.transform, sprite);
        
        // Scrollbar
        CreateScrollbar(scrollPanel.transform, scrollRect);
    }

    private static void CreateShopPanelsSection(Transform parent, Sprite sprite)
    {
        // Create 10 shop panels (D401~D410)
        for (int i = 1; i <= 10; i++)
        {
            string panelName = $"D40{i}_ShopPanel";
            GameObject panel = CreateImage(panelName, parent, sprite, COLOR_ITEM_RED);
            
            LayoutElement le = panel.AddComponent<LayoutElement>();
            le.preferredHeight = 220;
            le.flexibleWidth = 1;
            
            AddOutline(panel);
            AddShadow(panel);
            
            // Panel Label
            CreateStretchedTMPText("Label", panel.transform, panelName,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(20, 0), new Vector2(300, 50), 28, Color.white);
        }
    }

    private static void CreateGridItemsSection(Transform parent, Sprite sprite)
    {
        // Grid container
        GameObject gridContainer = CreateRect("Section_Grid", parent);
        
        // LayoutElement without fixed height - let ContentSizeFitter determine height
        LayoutElement gridLE = gridContainer.AddComponent<LayoutElement>();
        gridLE.flexibleWidth = 1;
        // Do NOT set preferredHeight - let ContentSizeFitter handle it
        
        // GridLayoutGroup
        GridLayoutGroup glg = gridContainer.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(200, 254); // 200 box + 54 info bar
        glg.spacing = new Vector2(16, 20);
        glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment = TextAnchor.UpperLeft;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;
        glg.padding = new RectOffset(8, 8, 0, 20); // Added bottom padding
        
        // Add ContentSizeFitter to auto-calculate grid height
        ContentSizeFitter gridCSF = gridContainer.AddComponent<ContentSizeFitter>();
        gridCSF.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        gridCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create grid items (D421~D440)
        for (int i = 21; i <= 40; i++)
        {
            CreateGridItem($"D4{i}", gridContainer.transform, sprite);
        }
    }

    private static void CreateGridItem(string name, Transform parent, Sprite sprite)
    {
        // Item container
        GameObject item = CreateRect(name, parent);
        
        // Top box (200x200)
        GameObject box = CreateImage($"{name}_Box", item.transform, sprite, COLOR_ITEM_RED);
        RectTransform boxRT = box.GetComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0, 1);
        boxRT.anchorMax = new Vector2(0, 1);
        boxRT.pivot = new Vector2(0, 1);
        boxRT.anchoredPosition = Vector2.zero;
        boxRT.sizeDelta = new Vector2(200, 200);
        AddOutline(box);
        AddShadow(box);
        
        // Box label
        CreateStretchedTMPText("Label", box.transform, name,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(180, 40), 24, Color.white);
        
        // Bottom info bar (200x54)
        GameObject infoBar = CreateImage($"{name}_Info", item.transform, sprite, COLOR_ITEM_RED);
        RectTransform infoRT = infoBar.GetComponent<RectTransform>();
        infoRT.anchorMin = new Vector2(0, 1);
        infoRT.anchorMax = new Vector2(0, 1);
        infoRT.pivot = new Vector2(0, 1);
        infoRT.anchoredPosition = new Vector2(0, -200);
        infoRT.sizeDelta = new Vector2(200, 54);
        AddOutline(infoBar);
        
        // Info label
        CreateStretchedTMPText("Label", infoBar.transform, "100G",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(180, 40), 20, Color.white);
    }

    private static void CreateScrollbar(Transform parent, ScrollRect scrollRect)
    {
        GameObject scrollbarObj = CreateRect("Scrollbar_Vertical", parent);
        RectTransform sbRT = scrollbarObj.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 0.5f);
        sbRT.anchoredPosition = new Vector2(-4, 0);
        sbRT.sizeDelta = new Vector2(8, 0);
        
        Image sbImage = scrollbarObj.AddComponent<Image>();
        sbImage.color = new Color(1, 1, 1, 0.1f);
        
        Scrollbar sb = scrollbarObj.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        
        // Handle
        GameObject handle = CreateRect("Handle", scrollbarObj.transform);
        RectTransform handleRT = handle.GetComponent<RectTransform>();
        handleRT.anchorMin = Vector2.zero;
        handleRT.anchorMax = Vector2.one;
        handleRT.offsetMin = Vector2.zero;
        handleRT.offsetMax = Vector2.zero;
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1, 1, 1, 0.3f);
        
        sb.handleRect = handleRT;
        sb.targetGraphic = handleImage;
        
        scrollRect.verticalScrollbar = sb;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
    }

    // ========== ZONE: FOOTER (CATEGORY) ==========
    private static void CreateAA402CategoryToggles(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA402_CategoryTogglePanel", parent);
        SetRectTopLeft(panel, 0, AA402_Y, REF_WIDTH, AA402_HEIGHT);
        
        ToggleGroup toggleGroup = panel.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false;
        
        string[] toggleNames = { "C401", "C402", "C403", "C404", "C405" };
        
        for (int i = 0; i < 5; i++)
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
            
            AddShadow(toggleObj);
            CreateStretchedLabel("Label", toggleObj.transform, toggleNames[i]);
        }
    }

    // ========== HELPERS ==========
    
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
        img.raycastTarget = true;
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

    private static void CreateStretchedTMPText(string name, Transform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 pos, Vector2 size, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void AddShadow(GameObject go)
    {
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
    }

    private static void AddOutline(GameObject go)
    {
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
    }

    private static Sprite CreateRoundedSprite(int radius)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, 
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
