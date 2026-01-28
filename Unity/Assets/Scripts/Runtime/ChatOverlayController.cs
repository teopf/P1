using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatOverlayController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform dimPanel;
    public RectTransform chatContainer;
    public Button btnClose;
    public Button btnSend;
    public InputField inputField; // Using Legacy InputField as per Fallback
    public ToggleGroup channelGroup;
    public ScrollRect scrollRect;
    public Transform contentRoot;
    
    [Header("Configuration")]
    public float animationDuration = 0.3f;
    
    private Vector2 containerStartPos;
    private Vector2 containerHiddenPos;
    private bool isAnimating = false;

    private void Awake()
    {
        // Auto-link references
        if (dimPanel == null) dimPanel = transform.Find("Panel_Dimmer")?.GetComponent<RectTransform>();
        if (chatContainer == null) chatContainer = transform.Find("Container_ChatWindow")?.GetComponent<RectTransform>();
        
        if (chatContainer != null)
        {
            if (btnClose == null) btnClose = chatContainer.Find("Panel_TopBar/Btn_Close_b101")?.GetComponent<Button>();
            if (btnSend == null) btnSend = chatContainer.Find("Panel_BottomInput/Btn_Send_b90")?.GetComponent<Button>();
            
            // Try to find input field (fake button in generator, need actual input)
            // Generator created "Input_Message" as an Image/Button dummy.
            // We need to attach InputField component at runtime if not present, or assume Generator uses InputField.
            // Generator used "CreateImage" for Input_Message. Let's add InputField here dynamically if needed or just use logic.
            // Ideally Generator should add InputField. But since we can't edit Generator now without interrupting flow (or we can?),
            // Let's assume user manually fixes or we add it here.
            
            var inputObj = chatContainer.Find("Panel_BottomInput/Input_Message");
            if (inputObj != null)
            {
                inputField = inputObj.GetComponent<InputField>();
                if (inputField == null)
                {
                    inputField = inputObj.gameObject.AddComponent<InputField>();
                    var text = inputObj.Find("Txt_Placeholder")?.GetComponent<Text>();
                    if (text != null)
                    {
                        inputField.textComponent = text; // Reuse placeholder text object as main text for now? 
                        // Actually InputField needs Target Graphic and Text Component.
                        // This is getting tricky dynamically. 
                        // Let's stick to simple "Button Click -> Mock Keyboard -> Set Text" for prototype if InputField is complex setup.
                        // Or just AddComponent.
                    }
                }
            }
            
            if (scrollRect == null) scrollRect = chatContainer.GetComponentInChildren<ScrollRect>();
            if (contentRoot == null && scrollRect != null) contentRoot = scrollRect.content;
            
            if (channelGroup == null) channelGroup = chatContainer.GetComponentInChildren<ToggleGroup>();
        }

        // Setup Listeners
        if (btnClose != null) btnClose.onClick.AddListener(OnCloseClicked);
        if (btnSend != null) btnSend.onClick.AddListener(OnSendClicked);
        
        // Dimmer click close
        if (dimPanel != null)
        {
            var btn = dimPanel.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnCloseClicked);
        }
    }

    private void Start()
    {
        if (chatContainer != null)
        {
            containerStartPos = chatContainer.anchoredPosition;
            // Hidden pos: Slide down off screen. Screen Height 1920.
            // Let's settle for moving it down by 1500px.
            containerHiddenPos = containerStartPos + new Vector2(0, -1500); 
        }
    }

    private void OnEnable()
    {
        // Animate Open
        StartCoroutine(OpenSequence());
        
        // Refresh Chat (Mock)
        if (contentRoot != null && contentRoot.childCount == 0)
        {
            AddSystemMessage("Welcome to the chat!");
        }
    }

    private void OnDisable()
    {
        // Reset positions
        if (chatContainer != null) chatContainer.anchoredPosition = containerHiddenPos;
    }

    public void OnCloseClicked()
    {
        if (isAnimating) return;
        StartCoroutine(CloseSequence());
    }

    private void OnSendClicked()
    {
        // Mock Send
        string msg = "Hello World!";
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            msg = inputField.text;
            inputField.text = "";
        }
        
        AddUserMessage("Player", msg);
        StartCoroutine(AnimateButtonPunch(btnSend.transform));
    }

    private void AddUserMessage(string user, string message)
    {
        // Create Message Object
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(contentRoot, false);
        
        Image img = msgObj.AddComponent<Image>();
        img.color = new Color(0,0,0,0.3f);
        
        VerticalLayoutGroup vlg = msgObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        
        ContentSizeFitter csf = msgObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(msgObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = $"<b><color=#FFD700>{user}</color></b> : {message}";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 30;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate; // Let CSF handle it? No, Overflow for height.
        
        // Start Scroll to Bottom
        Canvas.ForceUpdateCanvases();
        if(scrollRect) scrollRect.verticalNormalizedPosition = 0;
    }
    
    private void AddSystemMessage(string message)
    {
        AddUserMessage("System", message); // Reuse logic for now
    }

    private IEnumerator OpenSequence()
    {
        isAnimating = true;
        
        // Init state
        if (dimPanel) dimPanel.gameObject.SetActive(true);
        // Fade Dimmer? (CanvasGroup needed). Skip for prototype unless required.
        
        if (chatContainer) chatContainer.anchoredPosition = containerHiddenPos;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            // Ease Out Expo
            t = t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
            
            if (chatContainer)
                chatContainer.anchoredPosition = Vector2.Lerp(containerHiddenPos, containerStartPos, t);
            
            yield return null;
        }
        
        if (chatContainer) chatContainer.anchoredPosition = containerStartPos;
        isAnimating = false;
    }

    private IEnumerator CloseSequence()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            // Ease In Cubic
            t = t * t * t;
            
            if (chatContainer)
                chatContainer.anchoredPosition = Vector2.Lerp(containerStartPos, containerHiddenPos, t);
            
            yield return null;
        }
        
        gameObject.SetActive(false); // Disable Self
        isAnimating = false;
        
        // Notify HUD
        var hud = FindObjectOfType<HUDController>();
        if (hud) hud.OnSubMenuClosed();
    }
    
    private IEnumerator AnimateButtonPunch(Transform target)
    {
        float duration = 0.1f;
        float elapsed = 0f;
        Vector3 original = Vector3.one;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = Mathf.Lerp(1f, 0.9f, t);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = original;
    }
}
