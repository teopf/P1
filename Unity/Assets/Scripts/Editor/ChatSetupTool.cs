using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UI.Chat;
using UnityEditor.Events; // For UnityEventTools

namespace EditorTools
{
    public class ChatSetupTool : EditorWindow
    {
        [MenuItem("Tools/Setup Chat UI")]
        public static void SetupChatUI()
        {
            // 1. HUD_Canvas 찾기
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null)
            {
                Debug.LogError("HUD_Canvas not found in the scene!. Run 'Tools/Generate HUD Layout' first or create one.");
                return;
            }

            // 2. Chat Panel 생성
            string panelName = "Panel_Chat";
            Transform existingPanel = hudCanvas.transform.Find(panelName);
            if (existingPanel != null)
            {
                Debug.LogWarning($"Panel_Chat already exists under HUD_Canvas. Skipping creation.");
                return;
            }

            // Create Panel (Container)
            GameObject panelObj = new GameObject(panelName, typeof(RectTransform), typeof(Image));
            panelObj.transform.SetParent(hudCanvas.transform, false);
            
            Image panelImage = panelObj.GetComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 3. Components 추가
            ChatUI chatUI = panelObj.AddComponent<ChatUI>();
            ChatPresenter chatPresenter = panelObj.AddComponent<ChatPresenter>();

            // 4. 하위 UI 요소 생성
            
            // Header (Text)
            GameObject headerObj = new GameObject("Text_Header", typeof(RectTransform));
            headerObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "Chat Room";
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.fontSize = 40;
            headerText.color = Color.white;
            
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Scroll View (Body)
            GameObject scrollViewObj = new GameObject("Scroll_MessageList", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollViewObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform scrollRectTrans = scrollViewObj.GetComponent<RectTransform>();
            scrollRectTrans.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRectTrans.anchorMax = new Vector2(0.95f, 0.85f);
            scrollRectTrans.offsetMin = Vector2.zero;
            scrollRectTrans.offsetMax = Vector2.zero;
            
            Image scrollImage = scrollViewObj.GetComponent<Image>();
            scrollImage.color = new Color(1, 1, 1, 0.05f); // Very faint background
            
            ScrollRect scrollRect = scrollViewObj.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 20;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);
            
            Image viewportImage = viewportObj.GetComponent<Image>();
            viewportImage.raycastTarget = true; // Mask needs it to interact/cull
            
            Mask viewportMask = viewportObj.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Content
            GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewportObj.transform, false);
            
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1); 
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 300);
            
            VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Connect ScrollRect
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;

            // Footer (Area)
            GameObject footerArea = new GameObject("Footer", typeof(RectTransform));
            footerArea.transform.SetParent(panelObj.transform, false);
            RectTransform footerRect = footerArea.GetComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0.05f, 0.02f);
            footerRect.anchorMax = new Vector2(0.95f, 0.12f);
            footerRect.offsetMin = Vector2.zero;
            footerRect.offsetMax = Vector2.zero;

            // Input Field
            GameObject inputObj = new GameObject("Input_Chat", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObj.transform.SetParent(footerArea.transform, false);
            
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 0);
            inputRect.anchorMax = new Vector2(0.8f, 1);
            // Add padding to prevent "sticking out" visually
            inputRect.offsetMin = new Vector2(10, 10); 
            inputRect.offsetMax = new Vector2(-10, -10);
            
            Image inputImage = inputObj.GetComponent<Image>();
            inputImage.color = Color.white;
            
            TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
            inputField.textViewport = inputRect; // Using self as viewport for simplicity
            
            // Input Text Area (Text Component)
            GameObject inputTextObj = new GameObject("Text Area", typeof(RectTransform));
            inputTextObj.transform.SetParent(inputObj.transform, false);
            RectTransform textAreaRect = inputTextObj.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 0); // Padding
            textAreaRect.offsetMax = new Vector2(-10, 0);
            
            GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderObj.transform.SetParent(inputTextObj.transform, false);
            TextMeshProUGUI placeholderText = placeholderObj.GetComponent<TextMeshProUGUI>();
            placeholderText.text = "Enter message...";
            placeholderText.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            placeholderText.fontStyle = FontStyles.Italic;
            placeholderText.enableWordWrapping = false;
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            
            GameObject textComponentObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textComponentObj.transform.SetParent(inputTextObj.transform, false);
            TextMeshProUGUI inputText = textComponentObj.GetComponent<TextMeshProUGUI>();
            inputText.color = Color.black;
            inputText.enableWordWrapping = false;
            RectTransform textComponentRect = textComponentObj.GetComponent<RectTransform>();
            textComponentRect.anchorMin = Vector2.zero;
            textComponentRect.anchorMax = Vector2.one;
            
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;


            // Send Button
            GameObject btnObj = new GameObject("Btn_Send", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(footerArea.transform, false);
            
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.82f, 0);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.offsetMin = new Vector2(10, 10);
            btnRect.offsetMax = new Vector2(-10, -10);
            
            Image btnImage = btnObj.GetComponent<Image>();
            btnImage.color = new Color(0.3f, 0.8f, 0.3f); // Greenish
            
            Button sendBtn = btnObj.GetComponent<Button>();
            
            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnText.text = "Send";
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 24;
            btnText.raycastTarget = false; // Important: Don't block button clicks
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;

            // 5. Connect ChatUI References using SerializeObject
            SerializedObject serializedChatUI = new SerializedObject(chatUI);
            SerializedObject serializedChatPresenter = new SerializedObject(chatPresenter);

            serializedChatPresenter.FindProperty("_chatUI").objectReferenceValue = chatUI;
            serializedChatPresenter.ApplyModifiedProperties();

            serializedChatUI.FindProperty("_input_Chat").objectReferenceValue = inputField;
            serializedChatUI.FindProperty("_btn_Send").objectReferenceValue = sendBtn;
            serializedChatUI.FindProperty("_scroll_MessageList_Content").objectReferenceValue = contentObj.transform;
            serializedChatUI.FindProperty("_scroll_Rect").objectReferenceValue = scrollRect;
            
            // Prototype for Message Item
            GameObject msgItemProto = new GameObject("Proto_MessageItem", typeof(RectTransform), typeof(TextMeshProUGUI));
            msgItemProto.transform.SetParent(panelObj.transform, false);
            msgItemProto.SetActive(false); // Hide
            
            TextMeshProUGUI protoText = msgItemProto.GetComponent<TextMeshProUGUI>();
            protoText.text = "<b>Name</b>: Message Content";
            protoText.fontSize = 28;
            protoText.color = Color.white;
            protoText.enableWordWrapping = true;
            protoText.alignment = TextAlignmentOptions.Left;
            
            // Add ContentSizeFitter to prototype so it calculates its own height based on text content
            // However, inside VLG, we usually rely on LayoutElement or the text component itself acting as one.
            // TMP acts as layout element. We just need to make sure width is constrained by parent.
            // But let's add a ContentSizeFitter to be sure the height expands.
            ContentSizeFitter protoCsf = msgItemProto.AddComponent<ContentSizeFitter>();
            protoCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            protoCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // Width controlled by VLG
            
            serializedChatUI.FindProperty("_prefab_ChatMessageItem").objectReferenceValue = msgItemProto;

            serializedChatUI.ApplyModifiedProperties();

            // 6. Toggle Button Connection
            string buttonPath = "RowAboveAB1/a16";
            Transform btnTransform = hudCanvas.transform.Find(buttonPath);
            if (btnTransform != null)
            {
                Button toggleBtn = btnTransform.GetComponent<Button>();
                if (toggleBtn != null)
                {
                    // Remove existing listeners to prevent duplicates if run multiple times (optional but good practice)
                    // But here we just add.
                    
                    // Use UnityAction with the target method on the UnityEngine.Object (chatUI)
                    UnityEventTools.AddPersistentListener(toggleBtn.onClick, new UnityEngine.Events.UnityAction(chatUI.ToggleChatWindow));
                    
                    panelObj.SetActive(false); // Default hidden
                    Debug.Log($"Connected Chat Panel toggle to {buttonPath}");
                }
                else
                {
                    Debug.LogWarning($"Found {buttonPath} but it has no Button component.");
                }
            }
            else
            {
                Debug.LogWarning($"Could not find button at path: HUD_Canvas/{buttonPath}. Please check HUD structure.");
            }

            Debug.Log("Chat UI Setup Complete!");
            Selection.activeGameObject = panelObj;
        }
    }
}
