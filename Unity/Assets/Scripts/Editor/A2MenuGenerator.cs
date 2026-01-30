using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class A2MenuGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;

    // Colors
    private static readonly Color COLOR_PANEL_HEADER = new Color32(176, 196, 222, 255); // #B0C4DE
    private static readonly Color COLOR_PANEL_BG = new Color32(107, 140, 255, 255);     // Blue/Purple gradient start
    private static readonly Color COLOR_PANEL_BG_END = new Color32(155, 107, 255, 255); // Blue/Purple gradient end
    
    private static readonly Color COLOR_BTN_ORANGE = new Color32(255, 136, 0, 64);      // Default Orange (25% opacity)
    private static readonly Color COLOR_BTN_ORANGE_ACTIVE = new Color32(255, 110, 0, 128); // Active Orange
    
    // Items
    private static readonly Color COLOR_ITEM_BG = new Color32(255, 0, 0, 64);           // Red/Salmon (25%)

    [MenuItem("Tools/UI/Generate A2 Inventory Menu")]
    public static void Generate()
    {
        string canvasName = "Canvas_A2_Inventory";
        GameObject canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) DestroyImmediate(canvasObj);

        canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20; // Higher than Main HUD

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Add Controller
        // Note: A2MenuController will be added by the user or script later if it exists
        // canvasObj.AddComponent<A2MenuController>(); 
        
        // Initially Inactive
        canvasObj.SetActive(false);

        Sprite roundedSprite = GetRoundedSprite();

        // 1. AA101 Top Header (Shared Layout)
        CreateHeader(canvasObj.transform, roundedSprite);

        // 2. AA1 Secondary Action Bar
        CreateActionBar(canvasObj.transform, roundedSprite);

        // 3. AE201 Inventory Grid (Scroll)
        CreateInventoryGrid(canvasObj.transform, roundedSprite);

        // 4. AA202 Category Toggle
        CreateCategoryToggle(canvasObj.transform, roundedSprite);

        // 5. AB1 Bottom HUD (Visual Copy)
        CreateBottomHUD(canvasObj.transform, roundedSprite);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create A2 Menu Layout");
        Selection.activeGameObject = canvasObj;
        Debug.Log("A2 Inventory Menu Layout Generated Successfully!");
    }

    private static void CreateHeader(Transform parent, Sprite sprite)
    {
        // Panel_AA101_Header: 90px (Request says 90px height, Pos 63 from top)
        // Previous AA101 was 120px. Adjusting to spec: 90px.
        // Spec: Position 63px from top.
        
        GameObject panel = CreateImage("Panel_AA101_Header", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -63);
        rt.sizeDelta = new Vector2(0, 90);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        // Btn_Close_b101: Right aligned, 90x90
        // Position: 963px from left => Right margin: 1080 - 963 - 90 = 27px
        CreateButton("Btn_Close_b101", panel.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-27 - 45, 0), new Vector2(90, 90)); // -27 margin, -45 for pivot center
    }

    private static void CreateActionBar(Transform parent, Sprite sprite)
    {
        // Panel_AA1_ActionBar
        GameObject panel = CreateImage("Panel_AA1_ActionBar", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        // Position: 998px from top
        rt.anchoredPosition = new Vector2(0, -998);
        rt.sizeDelta = new Vector2(0, 90);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        // Buttons b18, b17, b16
        // b18: 705px from left. (705 + 45 = 750 center)
        // b17: 835px from left. 
        // b16: 965px from left.
        
        // Relative to centered pivot (0,0 is center of screen 540)
        // Left 0 = -540. 
        // 705 from left = -540 + 705 = +165 (Left edge)?? No, standard UI coords.
        // Let's use Anchor Min/Max 0,1 (Top Left) for easier absolute positioning?
        // Or keep Center Top anchor and calculate offset.
        // 1080 width. Center is 540.
        // 705 from left = 705 - 540 = 165 from center. (Left edge).
        // Center of button (90w) = 165 + 45 = 210.
        
        CreateButton("Btn_Action_b18", panel.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), // Left anchor
            new Vector2(705 + 45, 0), new Vector2(90, 90));
            
        CreateButton("Btn_Action_b17", panel.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), 
            new Vector2(835 + 45, 0), new Vector2(90, 90));

        CreateButton("Btn_Action_b16", panel.transform, sprite, COLOR_BTN_ORANGE,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), 
            new Vector2(965 + 45, 0), new Vector2(90, 90));
    }

    private static void CreateInventoryGrid(Transform parent, Sprite sprite)
    {
        // Panel_AE201_InventoryGrid
        // Top: 1026px. Bottom: 1626px. Height: 600px.
        GameObject panel = CreateImage("Panel_AE201_InventoryGrid", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -1026);
        rt.sizeDelta = new Vector2(0, 600);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        ScrollRect sr = panel.AddComponent<ScrollRect>();
        sr.vertical = true; sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 20;

        GameObject viewport = CreateRect("Viewport", panel.transform);
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f); // Transparent raycast target
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
        glg.padding = new RectOffset(8, 8, 82, 20); // Top padding 82px as per spec (first row position)
        glg.cellSize = new Vector2(200, 254);
        glg.spacing = new Vector2(16, 20);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;
        glg.childAlignment = TextAnchor.UpperLeft;

        // Sample Items D121-D145
        for (int i = 0; i < 25; i++)
        {
            CreateInventorySlot($"Slot_Item_D{121+i}", content.transform, sprite);
        }
    }

    private static void CreateInventorySlot(string name, Transform parent, Sprite sprite)
    {
        GameObject slotObj = CreateRect(name, parent);
        LayoutElement le = slotObj.AddComponent<LayoutElement>();
        le.preferredWidth = 200; le.preferredHeight = 254;

        // Top Box (Item Display) 200x200
        GameObject topBox = CreateImage("Img_ItemDisplay", slotObj.transform, sprite, COLOR_ITEM_BG);
        RectTransform topRT = topBox.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0.5f, 1); topRT.anchorMax = new Vector2(0.5f, 1);
        topRT.pivot = new Vector2(0.5f, 1);
        topRT.anchoredPosition = Vector2.zero;
        topRT.sizeDelta = new Vector2(200, 200);

        // Bottom Bar (Info) 200x54
        GameObject bottomBar = CreateImage("Img_InfoBar", slotObj.transform, sprite, COLOR_ITEM_BG);
        RectTransform botRT = bottomBar.GetComponent<RectTransform>();
        botRT.anchorMin = new Vector2(0.5f, 0); botRT.anchorMax = new Vector2(0.5f, 0);
        botRT.pivot = new Vector2(0.5f, 0);
        botRT.anchoredPosition = Vector2.zero;
        botRT.sizeDelta = new Vector2(200, 54);
        
        CreateText("Txt_Info", bottomBar.transform, "Info", 14);
    }

    private static void CreateCategoryToggle(Transform parent, Sprite sprite)
    {
        // Panel_AA202_Category
        GameObject panel = CreateImage("Panel_AA202_Category", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        // Position: 1641px from top
        rt.anchoredPosition = new Vector2(0, -1641);
        rt.sizeDelta = new Vector2(0, 90);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        ToggleGroup group = panel.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;
        
        // C201 (17px left, 348w), C202 (365px left, 348w), C203 (716px left, 349w)
        CreateToggle("Tgl_Category_C201", panel.transform, sprite, group, 17, 348);
        CreateToggle("Tgl_Category_C202", panel.transform, sprite, group, 365, 348);
        CreateToggle("Tgl_Category_C203", panel.transform, sprite, group, 716, 349);
    }

    private static void CreateToggle(string name, Transform parent, Sprite sprite, ToggleGroup group, float xPos, float width)
    {
        GameObject toggleObj = CreateImage(name, parent, sprite, COLOR_BTN_ORANGE);
        RectTransform rt = toggleObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.sizeDelta = new Vector2(width, 90);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.group = group;
        toggle.targetGraphic = toggleObj.GetComponent<Image>();
        
        ColorBlock cb = toggle.colors;
        cb.normalColor = COLOR_BTN_ORANGE;
        cb.selectedColor = COLOR_BTN_ORANGE_ACTIVE;
        toggle.colors = cb;
        
        CreateText("Label", toggleObj.transform, name.Replace("Tgl_Category_", ""));
    }

    private static void CreateBottomHUD(Transform parent, Sprite sprite)
    {
        // Panel_AB1_BottomHUD
        GameObject panel = CreateImage("Panel_AB1_BottomHUD", parent, sprite, COLOR_PANEL_HEADER);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        // Position: 1746px from top
        rt.anchoredPosition = new Vector2(0, -1746);
        rt.sizeDelta = new Vector2(0, 170);
        
        SetGradient(panel.GetComponent<Image>(), COLOR_PANEL_BG, COLOR_PANEL_BG_END);

        string[] names = { "Btn_Menu_a1", "Btn_Menu_a2", "Btn_Menu_a3", "Btn_Menu_a99", "Btn_Menu_a4", "Btn_Menu_a5", "Btn_Menu_a6" };
        float[] xPositions = { 17, 157, 297, 477, 656, 797, 937 };
        
        for (int i = 0; i < names.Length; i++)
        {
            Color c = (names[i] == "Btn_Menu_a2") ? new Color32(255, 0, 0, 128) : new Color32(255, 0, 0, 64);
            CreateButton(names[i], panel.transform, sprite, c,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(xPositions[i] + 64, 0), new Vector2(128, 128)); // +64 for center (128/2)
        }
    }

    // --- Helpers (Modified for TMP support if possible, else standard Text) ---
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

    private static void CreateText(string name, Transform parent, string content, int fontSize = 24)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        // Try simple Text for now to ensure no compile errors if TMP missing
        // Ideally we use TMP via reflection or Assembly Definition checks
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        // Fix for Unity 2022+ where Arial.ttf is removed from built-in resources
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize + 10;
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
    }

    private static GameObject CreateButton(string name, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = CreateImage(name, parent, sprite, color);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Button btn = btnObj.AddComponent<Button>();
        CreateText("Text", btnObj.transform, name.Replace("Btn_Menu_", "").Replace("Btn_Action_", "").Replace("Btn_Close_", ""), 20);
        
        Shadow shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(0, -4);
        
        return btnObj;
    }
    
    private static void SetGradient(Image img, Color top, Color bottom)
    {
        // Simple vertical gradient texture generation
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
        // Simple rounded rect generation
        int size = 64;
        int radius = 12;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Basic logic for rounded corners
                // ... (Simplified for this snippet, assumes standard white rounded box)
                colors[y * size + x] = Color.white;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
