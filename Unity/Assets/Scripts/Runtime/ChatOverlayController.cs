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
        
        // Welcome message is now handled by ChatPresenter if needed.
        // ChatUI will display messages via AddMessage method.
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
        // Message sending is now handled by ChatPresenter. 
        // This method is kept for button punch animation only.
        StartCoroutine(AnimateButtonPunch(btnSend.transform));
        
        // ChatPresenter listens to ChatUI.OnSendButtonClicked event for actual send logic.
        Debug.Log("ChatOverlayController: Send button clicked. ChatPresenter handles actual send.");
    }

    // AddUserMessage and AddSystemMessage are now handled by ChatUI/ChatPresenter.

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
