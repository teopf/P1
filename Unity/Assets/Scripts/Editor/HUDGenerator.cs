using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDGenerator : EditorWindow
{
    private const int REF_WIDTH = 1080;
    private const int REF_HEIGHT = 1920;
    
    // Colors
    private static readonly Color COLOR_PANEL_TOP = new Color32(107, 140, 255, 217); // #6B8CFF, 85%
    private static readonly Color COLOR_PANEL_BOTTOM = new Color32(155, 107, 255, 217); // #9B6BFF, 85%
    
    private static readonly Color COLOR_BTN_PINK_TOP = new Color32(255, 179, 186, 217); // #FFB3BA
    private static readonly Color COLOR_BTN_PINK_BOTTOM = new Color32(255, 166, 176, 217); // #FFA6B0
    
    private static readonly Color COLOR_BTN_BEIGE_TOP = new Color32(255, 228, 196, 217); // #FFE4C4
    private static readonly Color COLOR_BTN_BEIGE_BOTTOM = new Color32(245, 222, 179, 217); // #F5DEB3

    [MenuItem("Tools/UI/Generate HUD Layout")]
    public static void Generate()
    {
        // 1. Setup Canvas
        GameObject oldMain = GameObject.Find("Main_Canvas");
        if (oldMain != null) DestroyImmediate(oldMain);
        
        GameObject canvasObj = GameObject.Find("HUD_Canvas");
        if (canvasObj != null) DestroyImmediate(canvasObj);
        
        canvasObj = new GameObject("HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 0.5f; // Balanced scaling
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem exists
        if (GameObject.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Check for New Input System
#if ENABLE_INPUT_SYSTEM
            try 
            {
                // Reflection to avoid compilation errors if package is missing in some contexts, 
                // though usually #if handles it. 
                // Using standard component addition if type is available.
                var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputModuleType != null)
                {
                    esObj.AddComponent(inputModuleType);
                }
                else
                {
                    // Fallback to Standalone (might prompt for upgrade)
                    esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            catch
            {
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
#else
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }
        
        // 2. Resources (Sprites)
        Sprite roundedSprite = GetRoundedSprite();
        
        // 3. AA1 Top Panel
        CreateTopPanel(canvasObj.transform, roundedSprite);

        // 4. Content Area (Below Top Panel, Above Bottom Panel)
        CreateLeftStack(canvasObj.transform, roundedSprite);
        CreateRightStack(canvasObj.transform, roundedSprite);
        
        // 5. Bottom Corner Buttons
        CreateBottomLeftStack(canvasObj.transform, roundedSprite);
        CreateBottomRightStack(canvasObj.transform, roundedSprite);
        
        // 6. Bottom Panel Area (Above AB1)
        CreateBottomRowAbovePanel(canvasObj.transform, roundedSprite);

        // 7. AB1 Bottom Panel
        CreateBottomPanel(canvasObj.transform, roundedSprite);
        
        // Attach Runtime Controller
        if (canvasObj.GetComponent<HUDController>() == null)
        {
            canvasObj.AddComponent<HUDController>();
        }

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create HUD Layout");
        Selection.activeGameObject = canvasObj;
        Debug.Log("HUD Layout Generated Successfully!");
    }

    private static void CreateTopPanel(Transform parent, Sprite sprite)
    {
        // Panel AA1
        GameObject panelObj = CreateImage("AA1_TopPanel", parent, sprite, COLOR_PANEL_TOP);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        
        // Top Stretch
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 120);
        
        SetGradient(panelObj.GetComponent<Image>(), COLOR_PANEL_TOP, COLOR_PANEL_BOTTOM);

        float startX = -40; // From right edge
        float gap = 20;
        float size = 80;
        
        // b18 (Rightmost)
        CreateButton("b18", panelObj.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 0.5f), new Vector2(1, 0.5f), 
            new Vector2(startX - size/2, 0), new Vector2(size, size));
            
        // b17
        CreateButton("b17", panelObj.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 0.5f), new Vector2(1, 0.5f), 
            new Vector2(startX - size - gap - size/2, 0), new Vector2(size, size));

        // b16
        CreateButton("b16", panelObj.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 0.5f), new Vector2(1, 0.5f), 
            new Vector2(startX - size*2 - gap*2 - size/2, 0), new Vector2(size, size));
    }

    private static void CreateLeftStack(Transform parent, Sprite sprite)
    {
        GameObject container = CreateRect("LeftStack", parent);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(40, -140); 
        
        // a21 (100x100, Pink)
        CreateButton("a21", container.transform, sprite, COLOR_BTN_PINK_TOP, new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(50, -50), new Vector2(100, 100));
            
        float currentY = -100 - 30; // a21 height + space
        
        // b7
        CreateButton("b7", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 1), new Vector2(0.5f, 1),
            new Vector2(50, currentY - 40), new Vector2(80, 80)); 
            
        currentY -= (80 + 30);
        // b6
        CreateButton("b6", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 1), new Vector2(0.5f, 1),
            new Vector2(50, currentY - 40), new Vector2(80, 80));
            
        currentY -= (80 + 30);
        // b5
        CreateButton("b5", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 1), new Vector2(0.5f, 1),
            new Vector2(50, currentY - 40), new Vector2(80, 80));
    }

    private static void CreateRightStack(Transform parent, Sprite sprite)
    {
        GameObject container = CreateRect("RightStack", parent);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-40, -140); 

        float buttonSize = 80;
        float spacing = 30;
        
        // b15
        float currentY = 0;
        CreateButton("b15", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(0, -buttonSize/2), new Vector2(buttonSize, buttonSize));
            
        // b14 - b10
        string[] names = { "b14", "b13", "b12", "b11", "b10" };
        currentY -= (buttonSize + spacing);
        
        foreach(var name in names)
        {
            CreateButton(name, container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(0, currentY - buttonSize/2), new Vector2(buttonSize, buttonSize));
            currentY -= (buttonSize + spacing);
        }
    }

    private static void CreateBottomLeftStack(Transform parent, Sprite sprite)
    {
        GameObject container = CreateRect("BottomLeftStack", parent);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); // Bottom Left
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        
        float startY = 240; 
        
        // b1 (Bottom-most) -> Chat
        CreateButton("Chat", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(40, startY + 40), new Vector2(80, 80)); 
            
        // b2
        CreateButton("b2", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(40, startY + 80 + 30 + 40), new Vector2(80, 80));
            
        // b3
        CreateButton("b3", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(40, startY + (80+30)*2 + 40), new Vector2(80, 80));
            
        // b4
        CreateButton("b4", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(40, startY + (80+30)*3 + 40), new Vector2(80, 80));
    }

    private static void CreateBottomRightStack(Transform parent, Sprite sprite)
    {
        GameObject container = CreateRect("BottomRightStack", parent);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        
        float startY = 240; 
        
        // b8 (Bottom-most)
        CreateButton("b8", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-40, startY + 40), new Vector2(80, 80));
            
        // b9
        CreateButton("b9", container.transform, sprite, COLOR_BTN_BEIGE_TOP, new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-40, startY + 80 + 30 + 40), new Vector2(80, 80));
    }

    private static void CreateBottomRowAbovePanel(Transform parent, Sprite sprite)
    {
        GameObject container = CreateRect("RowAboveAB1", parent);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 120 + 10); 
        
        string[] names = { "a11", "a12", "a13", "a14", "a15", "a16", "a17" };
        float width = 140;
        float height = 100;
        float spacing = 10;
        
        float totalWidth = (names.Length * width) + ((names.Length - 1) * spacing);
        float startX = -totalWidth / 2 + width / 2;
        
        for (int i = 0; i < names.Length; i++)
        {
            CreateButton(names[i], container.transform, sprite, COLOR_BTN_PINK_TOP, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(startX + i * (width + spacing), height/2), new Vector2(width, height));
        }
    }

    private static void CreateBottomPanel(Transform parent, Sprite sprite)
    {
        // AB1 Panel
        GameObject panelObj = CreateImage("AB1_BottomPanel", parent, sprite, COLOR_PANEL_TOP);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        
        // Bottom Stretch
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 120);
        
        SetGradient(panelObj.GetComponent<Image>(), COLOR_PANEL_TOP, COLOR_PANEL_BOTTOM);
        
        string[] names = { "a1", "a2", "a3", "a99", "a4", "a5", "a6" };
        float width = 140;
        float height = 100;
        float spacing = 10;
        
        float totalWidth = (names.Length * width) + ((names.Length - 1) * spacing);
        float startX = -totalWidth / 2 + width / 2;
        
        for (int i = 0; i < names.Length; i++)
        {
            CreateButton(names[i], panelObj.transform, sprite, COLOR_BTN_PINK_TOP, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(startX + i * (width + spacing), 0), new Vector2(width, height)); 
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
        img.sprite = sprite;
        img.color = color;
        img.type = Image.Type.Sliced;
        return go;
    }

    private static void CreateButton(string name, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = CreateImage(name, parent, sprite, color);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        
        Button btn = btnObj.AddComponent<Button>();
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = name;
        
        // FIX: Reverting to LegacyRuntime.ttf as Arial.ttf is problematic in newer Unity versions
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); 
        
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.2f, 0.2f, 0.2f);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = 30;
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        // Shadow
        var shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.3f);
        shadow.effectDistance = new Vector2(2, -2);

        // Hover Effect
        btnObj.AddComponent<ButtonHoverEffect>();
    }
    
    private static void SetGradient(Image img, Color top, Color bottom)
    {
        Texture2D tex = new Texture2D(1, 128);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 128; y++)
        {
            float t = (float)y / 127f;
            tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
        }
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0,0,1,128), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Simple;
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
