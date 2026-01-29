using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HUDController : MonoBehaviour
{
    // Dictionary to manage multiple sub-menus: "a1" -> CanvasObj
    private Dictionary<string, GameObject> subMenus = new Dictionary<string, GameObject>();

    // Mapping button IDs to Canvas Names
    private readonly Dictionary<string, string> buttonToCanvasMap = new Dictionary<string, string>()
    {
        { "a1", "SubMenu_Canvas" },
        { "a2", "Canvas_A2_Inventory" },
        { "a3", "Canvas_a3_Growth" },  // Growth Menu
        { "a6", "Canvas_a6_Shop" },    // Shop Menu
        { "Chat", "Canvas_ChatOverlay" } // Maps "Chat" button to Overlay
    };

    private void Awake()
    {
        // 1. Register known sub-menus from map
        foreach (var kvp in buttonToCanvasMap)
        {
            var canvasObj = FindInactiveObject(kvp.Value);
            if (canvasObj != null)
            {
                subMenus[kvp.Key] = canvasObj;
            }
        }

        // 2. Setup HUD Buttons (Local)
        List<Button> hudButtons = new List<Button>();
        hudButtons.AddRange(GetComponentsInChildren<Button>(true));

        foreach (var btn in hudButtons)
        {
            string btnName = btn.name;
            string id = btnName.Replace("Btn_Menu_", "").Replace("Btn_Action_", "");

            btn.onClick.RemoveAllListeners();

            if (id == "a99" || btnName.Contains("Close")) // Global Close in HUD ?
            {
                btn.onClick.AddListener(() => OnCloseAllClicked(btn));
            }
            else if (buttonToCanvasMap.ContainsKey(id))
            {
                btn.onClick.AddListener(() => OnMenuToggleClicked(btn, id));
            }
            else
            {
                btn.onClick.AddListener(() => OnGenericButtonClicked(id));
            }
        }
        
        // 3. GLOBAL: Find "b101" (Close) buttons in ALL Canvases (Scene)
        // This ensures ANY menu with a b101 button will close itself when clicked.
        RegisterGlobalCloseButtons();
    }

    private void RegisterGlobalCloseButtons()
    {
        // Find all Canvases in component list (including inactive root objects if we use Resources.FindObjectsOfTypeAll with filtering)
        // Note: FindObjectsOfType<Canvas>(true) checks all active/inactive in SCENE (Unity 2020+)
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);

        foreach (var canvas in allCanvases)
        {
            if (canvas.name == "HUD_Canvas") continue; // Skip self

            // Find b101 or Btn_Close_b101 recursively
            var buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.name == "b101" || btn.name == "Btn_Close_b101")
                {
                    // Remove existing to avoid duplicates if other controllers added them
                    // But be careful if other controllers add CRITICAL logic. 
                    // User request: "b101 returns to HUD screen" -> Implies Close Canvas.
                    
                    // We simply AddListener. Unity allows multiple listeners.
                    // If we want to override, we'd remove. But safely appending is better unless it conflicts.
                    // Given the request "Create handle...", let's assume valid Close behaviour.
                    
                    btn.onClick.AddListener(() => 
                    {
                        Debug.Log($"Global Close: Closing {canvas.name} via {btn.name}");
                        StartCoroutine(AnimateButtonPunch(btn.transform));
                        
                        // Close the canvas
                        canvas.gameObject.SetActive(false);
                        
                        // Reset HUD state
                        OnSubMenuClosed();
                    });
                    
                    Debug.Log($"HUDController: Wired 'Close' logic to {btn.name} in {canvas.name}");
                }
            }
        }
    }

    private void OnMenuToggleClicked(Button btn, string id)
    {
        Debug.Log($"HUDController: '{id}' clicked.");
        
        // 1. Close all OTHER menus
        foreach (var kvp in subMenus)
        {
            if (kvp.Key != id && kvp.Value != null && kvp.Value.activeSelf)
            {
                kvp.Value.SetActive(false);
            }
        }

        // 2. Toggle TARGET menu
        if (subMenus.ContainsKey(id) && subMenus[id] != null)
        {
            bool isActive = subMenus[id].activeSelf;
            
            // Just Toggle
            subMenus[id].SetActive(!isActive);
            
            if (!isActive)
            {
                StartCoroutine(AnimateButtonPunch(btn.transform));
            }
        }
        else
        {
            // Try Dynamic Find
             var canvasName = buttonToCanvasMap.ContainsKey(id) ? buttonToCanvasMap[id] : "";
             var obj = FindInactiveObject(canvasName);
             if (obj != null)
             {
                 subMenus[id] = obj;
                 obj.SetActive(true);
             }
        }
    }

    private void OnCloseAllClicked(Button btn)
    {
        StartCoroutine(AnimateButtonPunch(btn.transform));
        OnSubMenuClosed();
    }
    
    public void OnSubMenuClosed()
    {
        // Close known menus
        foreach (var kvp in subMenus)
        {
            if (kvp.Value != null) kvp.Value.SetActive(false);
        }
        
        // Note: Global RegisterGlobalCloseButtons handles the specific canvas closing.
        // This method ensures our *Tracked* menus are marked closed if we called this from HUD.
    }

    private void OnGenericButtonClicked(string id)
    {
        Debug.Log($"HUDController: Button '{id}' clicked.");
    }

    private GameObject FindInactiveObject(string name)
    {
        // Optimized find for scene objects only
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        foreach (var t in objs)
        {
            if (t.hideFlags == HideFlags.None && t.name == name && t.gameObject.scene.IsValid())
            {
                return t.gameObject;
            }
        }
        return null;
    }

    private IEnumerator AnimateButtonPunch(Transform target)
    {
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 originalScale = Vector3.one; 
        
        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1f, 0.9f, t);
            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Bounce back
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0.9f, 1.05f, t); 
            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        target.localScale = originalScale;
    }
}


