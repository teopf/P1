using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatScrollController : MonoBehaviour
{
    public Transform contentRoot;
    public ScrollRect scrollRect;
    
    private List<GameObject> messages = new List<GameObject>();
    private const int MAX_MESSAGES = 50;

    private void Awake()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (contentRoot == null && scrollRect != null) contentRoot = scrollRect.content;
    }

    public void AddMessage(string text, string user = "System")
    {
        // Remove old if limit reached
        if (messages.Count >= MAX_MESSAGES)
        {
            var old = messages[0];
            messages.RemoveAt(0);
            Destroy(old); // Simple destroy for prototype, Pool better for prod
        }

        // Create Message
        GameObject msgObj = new GameObject("Message_" + messages.Count);
        msgObj.transform.SetParent(contentRoot, false);
        messages.Add(msgObj);

        // Background
        Image img = msgObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.3f); // Semi-transparent black

        // Layout
        VerticalLayoutGroup vlg = msgObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 12, 12);
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        ContentSizeFitter csf = msgObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Text
        GameObject textObj = new GameObject("Txt_Content");
        textObj.transform.SetParent(msgObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = $"<b><color=#FFD700>{user}</color></b>: {text}";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 36;
        t.lineSpacing = 1.2f; // 44px line height approx
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        
        // Ensure Text adjusts height
        ContentSizeFitter textCsf = textObj.AddComponent<ContentSizeFitter>();
        textCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Scroll to bottom
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0); // Scroll to bottom
        }
    }
}
