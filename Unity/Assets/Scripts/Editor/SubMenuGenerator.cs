using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class SubMenuGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // Colors
    private static readonly Color COLOR_PANEL_HEADER = new Color32(176, 196, 222, 255); // #B0C4DE Periwinkle Blue
    private static readonly Color COLOR_PANEL_BG = new Color32(160, 176, 224, 255);     // #A0B0E0 Darker Periwinkle
    private static readonly Color COLOR_PANEL_TOGGLE_BG = new Color32(211, 211, 211, 255); // #D3D3D3 Light Grey
    
    private static readonly Color COLOR_BTN_BROWN = new Color32(192, 160, 144, 255);    // #C0A090 Muted Brown
    private static readonly Color COLOR_BTN_SLOT = new Color32(205, 160, 176, 255);     // #CDA0B0 Muted Rose
    private static readonly Color COLOR_BTN_TOGGLE = new Color32(224, 208, 192, 255);   // #E0D0C0 Beige
    private static readonly Color COLOR_BTN_TOGGLE_ACTIVE = new Color32(200, 180, 160, 255); // Darker Beige

    private static readonly Color COLOR_BTN_HUD_ACTIVE = new Color32(255, 179, 232, 255); // Active Pink
    private static readonly Color COLOR_BTN_HUD = new Color32(232, 180, 232, 217);        // Normal Pink

    [MenuItem("Tools/UI/Generate Sub-Menu Layout")]
    public static void Generate()
    {
        string canvasName = "SubMenu_Canvas";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20; // Higher than Main HUD (0)

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Initially Inactive (as requested)
        canvasObj.SetActive(false);

        Sprite roundedSprite = GetRoundedSprite();

        // 1. AA101 Top Header
        CreateHeader(canvasObj.transform, roundedSprite);

        // 2. AC101 Fixed Grid
        CreateFixedGrid(canvasObj.transform, roundedSprite);
        
        // 3. AA102 Toggle Strip (Create first to layout AD101 relative to it)
        GameObject toggleStrip = CreateToggleStrip(canvasObj.transform, roundedSprite);
        
        // 4. AB1 Footer (Create first to layout AD101 relative to it)
        CreateBottomPanel(canvasObj.transform, roundedSprite);

        // 5. AD101 Scroll List (Fills space between AC101 and AA102)
        // AC101 Bottom: 120 (Header) + 350 (AC101) = 470px from top.
        // AA102 Top: Bottom 0 + 150 (Footer) = 150px from bottom. AA102 Height 100.
        // So AA102 is at Bottom 150. Top is Bottom 250.
        // AD101 Top: 470px. Bottom: 250px.
        CreateScrollList(canvasObj.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Sub-Menu Layout");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Sub-Menu Layout Generated Successfully!");
    }

    private static void CreateHeader(Transform parent, Sprite sprite)
    {
        // AA101: 120px, Top
        GameObject panel = CreateImage("AA101_Header", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 120);

        // Label
        CreateText("Label", panel.transform, "AA101", TextAnchor.MiddleLeft, new Vector2(30, 0));

        // b101 Button: Right aligned, 100x100
        CreateButton("b101", panel.transform, sprite, COLOR_BTN_BROWN,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-50 - 20, 0), new Vector2(100, 100)); // -50 for center, -20 margin
    }

    private static void CreateFixedGrid(Transform parent, Sprite sprite)
    {
        // AC101: Height 350, Top 120
        GameObject panel = CreateImage("AC101_FixedGrid", parent, sprite, COLOR_PANEL_BG);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -120);
        rt.sizeDelta = new Vector2(0, 350);

        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.cellSize = new Vector2(190, 160);
        grid.spacing = new Vector2(15, 15);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;

        // D101-D110
        for (int i = 1; i <= 10; i++)
        {
            CreateButton($"D{100+i}", panel.transform, sprite, COLOR_BTN_SLOT, 
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        }
    }

    private static GameObject CreateToggleStrip(Transform parent, Sprite sprite)
    {
        // AA102: Height 100, Above Footer (Footer is 150)
        GameObject panel = CreateImage("AA102_ToggleStrip", parent, sprite, COLOR_PANEL_TOGGLE_BG);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 150);
        rt.sizeDelta = new Vector2(0, 100);

        HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true; hlg.childControlHeight = false;
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.spacing = 10;

        ToggleGroup group = panel.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;

        string[] names = { "C101", "C102", "C103", "C104" };
        for (int i = 0; i < names.Length; i++)
        {
            // Base image color white for tinting
            GameObject toggleObj = CreateImage(names[i], panel.transform, sprite, Color.white);
            LayoutElement le = toggleObj.AddComponent<LayoutElement>();
            le.preferredHeight = 80;

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.group = group;
            toggle.isOn = (i == 0);
            toggle.targetGraphic = toggleObj.GetComponent<Image>();
            
            ColorBlock cb = toggle.colors;
            cb.normalColor = COLOR_BTN_TOGGLE;
            cb.selectedColor = COLOR_BTN_TOGGLE_ACTIVE;
            cb.highlightedColor = Color.Lerp(COLOR_BTN_TOGGLE, Color.white, 0.2f);
            cb.pressedColor = COLOR_BTN_TOGGLE_ACTIVE;
            toggle.colors = cb;
            
            CreateText("Label", toggleObj.transform, names[i]);
            
            // Shadow
            Shadow shadow = toggleObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.2f);
            shadow.effectDistance = new Vector2(0, -4);
            
            // Hover Effect
            toggleObj.AddComponent<ButtonHoverEffect>();
        }
        return panel;
    }

    private static void CreateBottomPanel(Transform parent, Sprite sprite)
    {
        // AB1: Height 150, Bottom 0
        GameObject panel = CreateImage("AB1_BottomPanel", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 150);

        string[] names = { "a1", "a2", "a3", "a99", "a4", "a5", "a6" };
        float w = 130, h = 100, spacing = 10;
        
        float totalWidth = (names.Length * w) + ((names.Length - 1) * spacing);
        float startX = -totalWidth / 2 + w / 2;

        for (int i = 0; i < names.Length; i++)
        {
            bool isActive = (names[i] == "a1");
            Color c = isActive ? COLOR_BTN_HUD_ACTIVE : COLOR_BTN_HUD;
            if (isActive) w = 140;
            else w = 130;

            CreateButton(names[i], panel.transform, sprite, c,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(startX + i * (130 + spacing), 0), new Vector2(w, h));
        }
    }

    private static void CreateScrollList(Transform parent, Sprite sprite)
    {
        // AD101: Flexible
        // Top Edge: 120 (Header) + 350 (AC101) = 470
        // Bottom Edge: 150 (Footer) + 100 (Toggle) = 250
        
        GameObject panel = CreateImage("AD101_ScrollList", parent, sprite, COLOR_PANEL_BG);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMax = new Vector2(0, -470); // Top inset
        rt.offsetMin = new Vector2(0, 250);  // Bottom inset

        ScrollRect sr = panel.AddComponent<ScrollRect>();
        sr.vertical = true; sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 20;

        GameObject viewport = CreateRect("Viewport", panel.transform);
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        GameObject content = CreateRect("Content", viewport.transform);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 1); contentRT.anchorMax = new Vector2(0.5f, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        sr.content = contentRT;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GridLayoutGroup glg = content.AddComponent<GridLayoutGroup>();
        glg.padding = new RectOffset(20, 20, 20, 20);
        glg.cellSize = new Vector2(190, 160);
        glg.spacing = new Vector2(15, 15);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;
        glg.childAlignment = TextAnchor.UpperCenter;

        // Sample Items D121-D220 (Creating subset for editor perfo, e.g., 20)
        // Request says 100 items. Let's create 25 to show structure.
        for (int i = 0; i < 25; i++)
        {
            CreateButton($"D{121+i}", content.transform, sprite, COLOR_BTN_SLOT, 
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        }
    }

    // --- Helpers ---
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
        if(sprite != null) img.sprite = sprite;
        img.color = color;
        img.type = Image.Type.Sliced;
        return go;
    }

    private static void CreateText(string name, Transform parent, string content, TextAnchor align = TextAnchor.MiddleCenter, Vector2 offset = default)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = align;
        text.color = new Color(0.1f, 0.1f, 0.1f);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = 30;
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(offset.x, offset.y); 
        textRT.offsetMax = new Vector2(-offset.x, -offset.y);
    }

    private static GameObject CreateButton(string name, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = CreateImage(name, parent, sprite, color);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        if (size != Vector2.zero) rt.sizeDelta = size;

        Button btn = btnObj.AddComponent<Button>();
        CreateText("Text", btnObj.transform, name);
        // Shadow
        Shadow shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
        
        // Hover Effect
        btnObj.AddComponent<ButtonHoverEffect>();
        
        return btnObj;
    }

    private static Sprite GetRoundedSprite()
    {
        int size = 64;
        int radius = 16;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Color trans = Color.clear;
        Color white = Color.white;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                if ((x < radius && y < radius) && Vector2.Distance(new Vector2(x, y), new Vector2(radius-0.5f, radius-0.5f)) > radius) inside = false;
                else if ((x > size-1-radius && y < radius) && Vector2.Distance(new Vector2(x, y), new Vector2(size-1-radius+0.5f, radius-0.5f)) > radius) inside = false;
                else if ((x < radius && y > size-1-radius) && Vector2.Distance(new Vector2(x, y), new Vector2(radius-0.5f, size-1-radius+0.5f)) > radius) inside = false;
                else if ((x > size-1-radius && y > size-1-radius) && Vector2.Distance(new Vector2(x, y), new Vector2(size-1-radius+0.5f, size-1-radius+0.5f)) > radius) inside = false;
                colors[y * size + x] = inside ? white : trans;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
