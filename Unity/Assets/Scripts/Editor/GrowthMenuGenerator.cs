using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GrowthMenuGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // Colors
    private static readonly Color COLOR_GRADIENT_START = new Color32(107, 140, 255, 255); // #6B8CFF
    private static readonly Color COLOR_GRADIENT_END = new Color32(155, 107, 255, 255);   // #9B6BFF
    private static readonly Color COLOR_BTN_ORANGE = new Color(1f, 0.533f, 0f, 0.25f);    // rgba(255,136,0,0.25)
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color(1f, 0.431f, 0f, 0.5f); // rgba(255,110,0,0.5)
    private static readonly Color COLOR_CARD_GREEN = new Color(0.659f, 1f, 0.698f, 0.5f); // rgba(168,255,178,0.5)
    private static readonly Color COLOR_CARD_BORDER = new Color(0.392f, 0.784f, 0.392f, 0.8f); // rgba(100,200,100,0.8)
    private static readonly Color COLOR_STATS_RED = new Color(1f, 0f, 0f, 0.25f);         // rgba(255,0,0,0.25)
    private static readonly Color COLOR_TITLE_GREEN = new Color32(45, 95, 45, 255);       // #2D5F2D

    // ========== POSITIONS (From User's Manual Adjustment) ==========
    // AA101 Top Panel
    private const float AA101_Y = -60f;
    private const float AA101_HEIGHT = 90f;
    
    // AE301 Main Content
    private const float AE301_Y = -783f;
    private const float AE301_HEIGHT = 845f;
    
    // Cards inside AE301
    private const float CARD_Y = -357f;
    private static readonly float[] CARD_X = { 71f, 394f, 714f };
    private const float CARD_WIDTH = 300f;
    private const float CARD_HEIGHT = 450f;
    
    // AA1 Action Bar
    private const float AA1_Y = -786f;
    private const float AA1_HEIGHT = 90f;
    private static readonly float[] AA1_BTN_X = { 750f, 880f, 1010f }; // b18, b17, b16 center X
    
    // AA302 Category Toggles
    private const float AA302_Y = -1641f;
    private const float AA302_HEIGHT = 90f;
    private static readonly float[] TOGGLE_X = { 17f, 278f, 542f, 804f };
    private static readonly float[] TOGGLE_W = { 261f, 262f, 262f, 261f };

    [MenuItem("Tools/UI/Generate a3 Growth Menu")]
    public static void Generate()
    {
        string canvasName = "Canvas_a3_Growth";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false); // Initially hidden

        Sprite roundedSprite = CreateRoundedSprite(12);

        // Container (Stretch to fill)
        GameObject container = CreateRect("a3_MenuContainer", canvasObj.transform);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero; 
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // 1. AA101 Top Panel
        CreateAA101TopPanel(container.transform, roundedSprite);

        // 2. AE301 Main Content
        CreateAE301MainContent(container.transform, roundedSprite);

        // 3. AA1 Action Bar
        CreateAA1ActionBar(container.transform, roundedSprite);

        // 4. AA302 Category Toggles
        CreateAA302CategoryToggles(container.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create a3 Growth Menu");
        Selection.activeGameObject = canvasObj;
        Debug.Log("a3 Growth Menu Generated Successfully!");
    }

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
        AddButtonShadow(btn);
    }

    private static void CreateAE301MainContent(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AE301_MainContentPanel", parent);
        SetRectTopLeft(panel, 0, AE301_Y, REF_WIDTH, AE301_HEIGHT);
        
        string[] cardNames = { "AF301_StatsCard", "AF302_StatsCard", "AF303_StatsCard" };
        string[] statsNames = { "E121", "E122", "E123" };
        
        for (int i = 0; i < 3; i++)
        {
            // Card
            GameObject card = CreateImage(cardNames[i], panel.transform, sprite, COLOR_CARD_GREEN);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0, 1); 
            cardRT.anchorMax = new Vector2(0, 1);
            cardRT.pivot = new Vector2(0, 1);
            cardRT.anchoredPosition = new Vector2(CARD_X[i], CARD_Y);
            cardRT.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);
            
            card.AddComponent<Outline>().effectColor = COLOR_CARD_BORDER;
            card.GetComponent<Outline>().effectDistance = new Vector2(2, -2);
            Shadow cardShadow = card.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0, 0, 0, 0.25f);
            cardShadow.effectDistance = new Vector2(0, -6);
            
            // Card Title
            CreateStretchedTMPText($"Lbl_{cardNames[i]}", card.transform, cardNames[i].Split('_')[0],
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -20), new Vector2(280, 50), 32, COLOR_TITLE_GREEN);
            
            // Stats Display
            GameObject stats = CreateImage(statsNames[i], card.transform, sprite, COLOR_STATS_RED);
            RectTransform statsRT = stats.GetComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0.5f, 0); 
            statsRT.anchorMax = new Vector2(0.5f, 0);
            statsRT.pivot = new Vector2(0.5f, 0);
            statsRT.anchoredPosition = new Vector2(0, 24);
            statsRT.sizeDelta = new Vector2(200, 200);
            AddButtonShadow(stats);
            
            // Stats Label
            CreateStretchedTMPText($"Lbl_{statsNames[i]}", stats.transform, statsNames[i],
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 40), 28, Color.white);
        }
    }

    private static void CreateAA1ActionBar(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA1_SharedActionBar", parent);
        SetRectTopLeft(panel, 0, AA1_Y, REF_WIDTH, AA1_HEIGHT);
        
        string[] names = { "b18", "b17", "b16" };
        
        for (int i = 0; i < 3; i++)
        {
            GameObject btn = CreateButton(names[i], panel.transform, sprite, COLOR_BTN_ORANGE, names[i]);
            RectTransform btnRT = btn.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0.5f); 
            btnRT.anchorMax = new Vector2(0, 0.5f);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = new Vector2(AA1_BTN_X[i], 0);
            btnRT.sizeDelta = new Vector2(90, 90);
            AddButtonShadow(btn);
        }
    }

    private static void CreateAA302CategoryToggles(Transform parent, Sprite sprite)
    {
        GameObject panel = CreateGradientPanel("AA302_CategoryTogglePanel", parent);
        SetRectTopLeft(panel, 0, AA302_Y, REF_WIDTH, AA302_HEIGHT);
        
        ToggleGroup toggleGroup = panel.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false;
        
        string[] toggleNames = { "C301", "C302", "C303", "C304" };
        
        for (int i = 0; i < 4; i++)
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
            
            // Stretch label
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

    private static void AddButtonShadow(GameObject go)
    {
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
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
