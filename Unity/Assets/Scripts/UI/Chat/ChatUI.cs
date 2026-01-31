using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Data;
using System.Collections.Generic;
using UI.Core;

namespace UI.Chat
{
    public class ChatUI : UIBase
    {
        [Header("Atoms")]
        [SerializeField] private TMP_InputField _input_Chat;
        [SerializeField] private Button _btn_Send;
        
        [Header("Molecules")]
        // 메시지 아이템 프리팹 (Text or TextMeshProUGUI)
        [SerializeField] private GameObject _prefab_ChatMessageItem;
        
        [Header("Font Settings")]
        [SerializeField] private TMP_FontAsset _chatFont; // 채팅 메시지 폰트 
        
        [Header("Organisms")]
        [SerializeField] private Transform _scroll_MessageList_Content; // Content transform of ScrollRect
        [SerializeField] private ScrollRect _scroll_Rect;

        // Presenter에게 알릴 이벤트
        public event Action<string> OnSendButtonClicked;

        protected override void Awake()
        {
            base.Awake();

            // 리스너 등록
            if (_btn_Send != null)
            {
                _btn_Send.onClick.AddListener(OnBtnSendClicked);
            }
            
            if (_input_Chat != null)
            {
                // 엔터키 입력 처리 등
                _input_Chat.onSubmit.AddListener(OnInputSubmit);
                
                // 입력 필드 폰트 설정
                if (_chatFont != null)
                {
                    if (_input_Chat.textComponent != null)
                    {
                        _input_Chat.textComponent.font = _chatFont;
                        Debug.Log($"[ChatUI] Input field font applied: {_chatFont.name}");
                    }
                    
                    // Placeholder 텍스트에도 폰트 적용
                    if (_input_Chat.placeholder != null && _input_Chat.placeholder is TMP_Text placeholderText)
                    {
                        placeholderText.font = _chatFont;
                    }
                }
                else
                {
                    Debug.LogWarning("[ChatUI] Chat Font not assigned in Inspector!");
                }
            }
        }

        private void OnBtnSendClicked()
        {
            SendMessage();
        }

        private void OnInputSubmit(string text)
        {
            SendMessage();
            // 엔터 후 포커스 유지 여부는 기획에 따라 결정. 여기선 일단 유지.
            _input_Chat.ActivateInputField();
        }

        private void SendMessage()
        {
            if (_input_Chat == null) return;

            string message = _input_Chat.text.Trim();

            // 빈 메시지는 전송하지 않음
            if (string.IsNullOrEmpty(message)) return;

            OnSendButtonClicked?.Invoke(message);

            // 전송 후 즉시 입력 필드 초기화
            _input_Chat.text = "";
        }

        public void ClearInput()
        {
            if (_input_Chat != null)
            {
                _input_Chat.text = "";
            }
        }

        public void AddMessage(ChatData chatData)
        {
            if (_prefab_ChatMessageItem == null || _scroll_MessageList_Content == null) return;

            GameObject newItem = Instantiate(_prefab_ChatMessageItem, _scroll_MessageList_Content);
            newItem.SetActive(true); // 활성화 (Prototype이 비활성화 상태일 수 있음)
            
            // 단순 텍스트 설정 (프리팹 구조에 따라 다름, 여기선 GetComponent<TMP_Text> 가정)
            TMP_Text textComponent = newItem.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                // 폰트 설정 (텍스트 설정 전에)
                if (_chatFont != null)
                {
                    textComponent.font = _chatFont;
                    // Debug.Log($"[ChatUI] Font applied: {_chatFont.name}"); // 로그 과도 스팸 방지
                }
                else
                {
                    Debug.LogWarning("[ChatUI] Chat Font is not assigned!");
                }
                
                // 텍스트 내용 설정
                textComponent.text = $"<b>{chatData.SenderName}</b>: {chatData.MessageContent}";
                
                // 명시적으로 색상과 크기 설정 (폰트 변경 시 리셋될 수 있음)
                textComponent.color = Color.white;
                if (textComponent.fontSize < 1) textComponent.fontSize = 28;
                
                // Material 확인
                if (textComponent.fontSharedMaterial == null)
                {
                    Debug.LogWarning("[ChatUI] Text component has no material! Font may not render.");
                }
                
                textComponent.textWrappingMode = TMPro.TextWrappingModes.Normal;
                textComponent.ForceMeshUpdate(); // 즉시 업데이트
            }
            else
            {
                Debug.LogError("[ChatUI] No TMP_Text found in message item!");
            }
            
            // 스크롤 아래로 이동
            // Canvas update 기다렸다가 이동해야 정확함.
            Canvas.ForceUpdateCanvases();
            _scroll_Rect.verticalNormalizedPosition = 0f;
        }

        public void ToggleChatWindow()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }
    }
}
