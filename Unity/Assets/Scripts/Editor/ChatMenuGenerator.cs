using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatMenuGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // Colors
    private static readonly Color COLOR_DIMMER = new Color32(0, 0, 0, 76); // 30% Black
    private static readonly Color COLOR_PANEL_BG_START = new Color32(107, 140, 255, 255); // #6B8CFF
    private static readonly Color COLOR_PANEL_BG_END = new Color32(155, 107, 255, 255);   // #9B6BFF
    
    private static readonly Color COLOR_BTN_ORANGE = new Color32(255, 136, 0, 64);        // 25% Orange
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color32(255, 110, 0, 128); // 50% Orange
    
    // Position Vars (Chat Window)
    private const int WINDOW_W = 766;
    private const int WINDOW_H = 1093; // 90(Top) + 90(Tab) + 813(Content) + 90(Input) + spacing? 
    // Spec says Top Offset 81px. Left 193px.
    
    [MenuItem("Tools/Generate Chat Overlay")]
    public static void Generate()
    {
        string canvasName = "Canvas_ChatOverlay";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Overlay on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false); // Initially hidden

        Sprite roundedSprite = GetRoundedSprite();

        // 1. Dimmer Background
        CreateDimmer(canvasObj.transform);

        // 2. Chat Window Container
        GameObject container = CreateRect("Container_ChatWindow", canvasObj.transform);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        // Updated based on User Dump (UI_Dump.txt)
        rt.anchoredPosition = new Vector2(161f, -531f); 
        rt.sizeDelta = new Vector2(0f, 1093f); // Width 0 (Children use sizeDelta to stretch/size)

        // 3. Top Bar
        CreateTopBar(container.transform, roundedSprite);

        // 4. Tab Panel
        CreateTabPanel(container.transform, roundedSprite);

        // 5. Chat Content
        CreateChatContent(container.transform, roundedSprite);

        // 6. Bottom Input
        CreateBottomInput(container.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Chat Overlay");
        Selection.activeGameObject = canvasObj;
        Debug.Log("Chat Overlay Generated Successfully!");
    }

    private static void CreateDimmer(Transform parent)
    {
        GameObject dimmer = CreateImage("Panel_Dimmer", parent, null, COLOR_DIMMER);
        RectTransform rt = dimmer.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        // Button to close on click outside
        Button btn = dimmer.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        // Logic will be handled by controller finding "Panel_Dimmer" button
    }

    private static void CreateTopBar(Transform parent, Sprite sprite)
    {
        // Pos: 0, 0 inside container. 90 high.
        GameObject panel = CreateImage("Panel_TopBar", parent, sprite, COLOR_PANEL_BG_START);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(0, 0); // Container-relative
        rt.sizeDelta = new Vector2(WINDOW_W, 90);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG_START, COLOR_PANEL_BG_END);

        // Btn_Close_b101: 647px from left.
        CreateButton("Btn_Close_b101", panel.transform, sprite, COLOR_BTN_ORANGE,
             new Vector2(0, 0.5f), new Vector2(0, 0.5f),
             new Vector2(647 + 45, 0), new Vector2(90, 90));
    }

    private static void CreateTabPanel(Transform parent, Sprite sprite)
    {
        // Below Top Bar. Top 109px (approx 90 + spacing?). Spec says 109 from Chat Window Top?
        // Spec: Position 109px from chat window top.
        // Wait, Screen Top 109... Chat Window Top is Screen Top 81. Diff = 28px? 
        // No, Spec: Top Bar at 81. Tab Panel at 109 from chat window top.
        // Let's trust "109 from chat window top".
        
        GameObject panel = CreateImage("Panel_Tab", parent, sprite, COLOR_PANEL_BG_START);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(0, -109);
        rt.sizeDelta = new Vector2(WINDOW_W, 90);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG_START, COLOR_PANEL_BG_END);

        ToggleGroup group = panel.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;

        // C111: 32px left
        CreateToggle("Tgl_Channel_C111", panel.transform, sprite, group, 32, 234);
        // C112: 266px left
        CreateToggle("Tgl_Channel_C112", panel.transform, sprite, group, 266, 235);
        // C113: 503px left
        CreateToggle("Tgl_Channel_C113", panel.transform, sprite, group, 503, 234);
    }

    private static void CreateChatContent(Transform parent, Sprite sprite)
    {
        // Height 813. Pos 109 from screen top... Wait.
        // If Chat Window Top is 81. Tab is 109 from Window Top (190 from screen).
        // Spec says: Chat Content Position 109px from SCREEN top.
        // BUT Tab Panel is ALSO 109 from Chat Top? Overlap?
        // Let's infer logic: Top Bar (90) -> (maybe gap) -> Tab (90).
        // If Content is below Tab? 
        // Spec says: Chat Content Position 109 from SCREEN top. (1072 - 991 base? No 1100 - 991 = 109).
        // Chat Window Top: 81. Top Bar 90. So Top Bar ends at 171.
        // If Content starts at 109... it overlaps Top Bar. 
        // USER Spec Inconsistency? 
        // "Chat Tab Panel... Position: 109px from chat window top". Window Top 81. Tab Top = 190.
        // "Main Chat Content Area... Position: 109px from screen top". 
        
        // Re-reading: "Chat Tab Panel ... Position: 109px from chat window top" -> 81 + 109 = 190 screen y.
        // "Main Chat Content ... Position: 109px from screen top". 
        // This is physically above the Tab Panel?
        
        // Let's assume the spec meant "109px from Tab Panel bottom" or similar?
        // Or typical layout: Top (90) -> Tab (90) -> Content (Fill) -> Input (90).
        
        // Let's place Content BELOW Tab.
        // TopBar (0-90). Tab (109-199). 
        // Content should start around 200+.
        
        // Spec: "Height 813". "Bottom Input Panel Position 922 from SCREEN top". 
        // Screen Top 922. Chat Top 81. Delta = 841.
        
        // Let's stick to a safe stack:
        // TopBar (0, 90)
        // Tab (Start 90 or 109? Let's use 90 for snug fit, or 109 for gap).
        // Let's use 109 (gap 19px).
        // Tab ends at 109+90 = 199.
        // Content starts at 199?
        // Input starts at (Window Height - 90)?
        
        // Let's just stack them relative to Container.
        
        // Content
        GameObject panel = CreateImage("Panel_ChatContent", parent, sprite, COLOR_PANEL_BG_START);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(0, -90 - 90 - 10); // Approx below Tab
        // Wait, let's just make it stretch between Tab and Input.
        // Top: 200. Bottom: 90.
        // Let's use specific height 813 but verify fit.
        
        rt.anchoredPosition = new Vector2(0, -199); // Below Tab
        rt.sizeDelta = new Vector2(WINDOW_W, 600); // 813 is too big for 1080 screen if offset is huge?
        // Oh, Height 813. Window H 1093. 1093 - 90 - 90 - 90 = 823. Matches~
        rt.sizeDelta = new Vector2(WINDOW_W, 813); 
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG_START, COLOR_PANEL_BG_END);

        ScrollRect sr = panel.AddComponent<ScrollRect>();
        sr.vertical = true; sr.horizontal = false;
        
        GameObject viewport = CreateRect("Viewport", panel.transform);
        viewport.AddComponent<Image>().color = new Color(0,0,0,0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        sr.viewport = vpRT;

        GameObject content = CreateRect("Content", viewport.transform);
        RectTransform cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 0); cRT.anchorMax = new Vector2(1, 0); // Bottom align?
        cRT.pivot = new Vector2(0.5f, 0);
        sr.content = cRT;
        
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.LowerLeft; // Chat flows up
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void CreateBottomInput(Transform parent, Sprite sprite)
    {
        // 922 from screen top. (Window Top 81. 922-81 = 841).
        // Or Bottom of window.
        // Window Height 1093. 841 is near bottom.
        
        GameObject panel = CreateImage("Panel_BottomInput", parent, sprite, COLOR_PANEL_BG_START);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0); // Bottom align
        rt.pivot = new Vector2(0.5f, 0);
        // Updated based on User Dump
        rt.anchoredPosition = new Vector2(381.5f, 0);
        rt.sizeDelta = new Vector2(769f, 90);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG_START, COLOR_PANEL_BG_END);

        // Input Field (Fake Button)
        GameObject inputBtn = CreateImage("Input_Message", panel.transform, sprite, new Color(1,1,1,0.15f));
        RectTransform inRT = inputBtn.GetComponent<RectTransform>();
        inRT.anchorMin = new Vector2(0, 0.5f); inRT.anchorMax = new Vector2(0, 0.5f);
        inRT.pivot = new Vector2(0, 0.5f);
        inRT.anchoredPosition = new Vector2(32, 0);
        inRT.sizeDelta = new Vector2(606, 90);
        
        CreateText("Txt_Placeholder", inputBtn.transform, "Type a message...", 30, TextAnchor.MiddleLeft, new Vector2(16,0));

        // Btn_Send_b90
        CreateButton("Btn_Send_b90", panel.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(647 + 45, 0), new Vector2(90, 90));
    }

    // --- Helpers (Reused) ---
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

    private static void CreateText(string name, Transform parent, string content, int fontSize = 24, TextAnchor align = TextAnchor.MiddleCenter, Vector2 offset = default)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = align;
        text.color = Color.white; 
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize + 10;
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(offset.x, offset.y);
        textRT.offsetMax = new Vector2(-offset.x, -offset.y);
    }
    
    private static void CreateButton(string name, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = CreateImage(name, parent, sprite, color);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Button btn = btnObj.AddComponent<Button>();
        CreateText("Text", btnObj.transform, name.Replace("Btn_Close_", "").Replace("Btn_Send_", ""), 20);
        
        Shadow shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
    }

    private static void CreateToggle(string name, Transform parent, Sprite sprite, ToggleGroup group, float xPos, float width)
    {
        GameObject toggleObj = CreateImage(name, parent, sprite, COLOR_BTN_ORANGE);
        RectTransform rt = toggleObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(xPos, 0); // Left align
        rt.sizeDelta = new Vector2(width, 90);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.group = group;
        toggle.targetGraphic = toggleObj.GetComponent<Image>();
        
        ColorBlock cb = toggle.colors;
        cb.normalColor = COLOR_BTN_ORANGE;
        cb.selectedColor = COLOR_BTN_ORANGE_ACTIVE;
        toggle.colors = cb;
        
        CreateText("Label", toggleObj.transform, name.Replace("Tgl_Channel_", ""));
    }

    private static void SetGradient(Image img, Color top, Color bottom)
    {
        Texture2D tex = new Texture2D(1, 16);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 16; y++)
        {
            tex.SetPixel(0, y, Color.Lerp(bottom, top, (float)y/15f));
        }
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0,0,1,16), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Sliced;
    }

    private static Sprite GetRoundedSprite()
    {
        int size = 64;
        int radius = 12;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                colors[y * size + x] = Color.white;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
