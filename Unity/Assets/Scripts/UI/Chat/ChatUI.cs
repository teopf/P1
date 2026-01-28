using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Data;
using System.Collections.Generic;

namespace UI.Chat
{
    public class ChatUI : MonoBehaviour
    {
        [Header("Atoms")]
        [SerializeField] private TMP_InputField _input_Chat;
        [SerializeField] private Button _btn_Send;
        
        [Header("Molecules")]
        // 메시지 아이템 프리팹 (Text or TextMeshProUGUI)
        [SerializeField] private GameObject _prefab_ChatMessageItem; 
        
        [Header("Organisms")]
        [SerializeField] private Transform _scroll_MessageList_Content; // Content transform of ScrollRect
        [SerializeField] private ScrollRect _scroll_Rect;

        // Presenter에게 알릴 이벤트
        public event Action<string> OnSendButtonClicked;

        private void Awake()
        {
            // 리스너 등록
            if (_btn_Send != null)
            {
                _btn_Send.onClick.AddListener(OnBtnSendClicked);
            }
            
            if (_input_Chat != null)
            {
                // 엔터키 입력 처리 등
                _input_Chat.onSubmit.AddListener(OnInputSubmit);
            }
        }

        private void OnBtnSendClicked()
        {
            if (_input_Chat != null)
            {
                OnSendButtonClicked?.Invoke(_input_Chat.text);
            }
        }

        private void OnInputSubmit(string text)
        {
             OnSendButtonClicked?.Invoke(text);
             //, 엔터 후 포커스 유지 여부는 기획에 따라 결정. 여기선 일단 유지.
             _input_Chat.ActivateInputField();
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
                textComponent.text = $"<b>{chatData.SenderName}</b>: {chatData.MessageContent}";
            }
            
            // 스크롤 아래로 이동
            // Canvas update 기다렸다가 이동해야 정확함.
            Canvas.ForceUpdateCanvases();
            _scroll_Rect.verticalNormalizedPosition = 0f;
        }

        public void ToggleChatWindow()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
