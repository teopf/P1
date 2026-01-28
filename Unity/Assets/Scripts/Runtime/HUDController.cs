using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    private GameObject subMenuCanvas;

    private void Awake()
    {
        // Find SubMenu Canvas (even if inactive)
        subMenuCanvas = FindInactiveObject("SubMenu_Canvas");

        // Find all buttons in HUD (this hierarchy)
        List<Button> allButtons = new List<Button>();
        allButtons.AddRange(GetComponentsInChildren<Button>(true));

        // Find buttons in SubMenu if available
        if (subMenuCanvas != null)
        {
            allButtons.AddRange(subMenuCanvas.GetComponentsInChildren<Button>(true));
        }

        foreach (var btn in allButtons)
        {
            string btnName = btn.name;
            
            // Remove existing listeners just in case
            btn.onClick.RemoveAllListeners();

            if (btnName == "a99" || btnName == "b101")
            {
                btn.onClick.AddListener(() => OnCloseAllClicked(btn));
            }
            else if (btnName == "a1")
            {
                 btn.onClick.AddListener(() => OnMenuToggleClicked(btn));
            }
            else
            {
                // Capture variable for lambda closure
                string id = btnName;
                btn.onClick.AddListener(() => OnGenericButtonClicked(id));
            }
        }
    }

    private void OnMenuToggleClicked(Button btn)
    {
        Debug.Log("HUDController: 'a1' clicked. Toggling Sub-Menu...");
        
        if (subMenuCanvas != null)
        {
            bool isActive = subMenuCanvas.activeSelf;
            subMenuCanvas.SetActive(!isActive);
            
            // Optional: Animation punch on a1
            StartCoroutine(AnimateButtonPunch(btn.transform));
        }
        else
        {
            Debug.LogError("HUDController: SubMenu_Canvas not found! Please generate it.");
            // Retry find?
            subMenuCanvas = FindInactiveObject("SubMenu_Canvas");
            if (subMenuCanvas != null)
            {
                subMenuCanvas.SetActive(true);
            }
        }
    }

    private void OnCloseAllClicked(Button btn)
    {
        Debug.Log("HUDController: 'Close All' (a99) clicked. Closing all panels...");
        
        // Scale animation (Punch effect)
        StartCoroutine(AnimateButtonPunch(btn.transform));

        if (subMenuCanvas != null)
        {
            subMenuCanvas.SetActive(false);
        }
    }

    private void OnGenericButtonClicked(string id)
    {
        Debug.Log($"HUDController: Button '{id}' clicked.");
    }

    private GameObject FindInactiveObject(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            // Root objects only or check hierarchy? Editor scene setup usually puts Canvases at root.
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }

    private IEnumerator AnimateButtonPunch(Transform target)
    {
        Vector3 originalScale = Vector3.one; // Assuming default is 1, but might be modified by Hover.
        // Actually best to punch relative to current or force a set punch
        
        float duration = 0.15f;
        float elapsed = 0f;

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
