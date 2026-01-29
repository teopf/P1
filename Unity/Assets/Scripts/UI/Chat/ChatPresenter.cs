using UnityEngine;
using Backend;
using Data;

namespace UI.Chat
{
    public class ChatPresenter : MonoBehaviour
    {
        [SerializeField] private ChatUI _chatUI;
        private bool _isVivoxEventConnected = false;

        private void OnEnable()
        {
            TryConnectToVivox();
        }

        private void Start()
        {
            if (_chatUI == null)
                _chatUI = GetComponent<ChatUI>();

            if (_chatUI == null)
            {
                Debug.LogError("[ChatPresenter] ChatUI component not found!");
                return;
            }
            
            Debug.Log("[ChatPresenter] ChatUI connected successfully");

            // UI 이벤트 연결
            _chatUI.OnSendButtonClicked += HandleSendButtonClicked;

            // Vivox 연결 시도
            TryConnectToVivox();
        }

        private void TryConnectToVivox()
        {
            if (_isVivoxEventConnected)
            {
                Debug.Log("[ChatPresenter] Already connected to VivoxManager");
                return;
            }

            if (VivoxManager.Instance != null)
            {
                VivoxManager.Instance.OnChannelMessageReceived -= HandleMessageReceived; // 중복 방지
                VivoxManager.Instance.OnChannelMessageReceived += HandleMessageReceived;
                _isVivoxEventConnected = true;
                Debug.Log("[ChatPresenter] ✅ Connected to VivoxManager.OnChannelMessageReceived");
            }
            else
            {
                Debug.LogWarning("[ChatPresenter] VivoxManager.Instance is null! Will retry later.");
            }
        }

        private void OnDisable()
        {
            if (_chatUI != null)
                _chatUI.OnSendButtonClicked -= HandleSendButtonClicked;

            if (VivoxManager.Instance != null)
            {
                VivoxManager.Instance.OnChannelMessageReceived -= HandleMessageReceived;
                _isVivoxEventConnected = false;
            }
        }

        private async void HandleSendButtonClicked(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // Vivox 연결 재확인 (늦게 초기화된 경우 대비)
            TryConnectToVivox();

            // Check if VivoxManager is available
            if (VivoxManager.Instance != null)
            {
                // Online Mode: Send via Vivox
                string currentChannel = "GlobalChat"; // TODO: Dynamic channel
                try
                {
                    await VivoxManager.Instance.SendChannelMessageAsync(currentChannel, message);
                    _chatUI.ClearInput();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Vivox send failed, falling back to local: {e.Message}");
                    // Fallback to local
                    HandleLocalSend(message);
                }
            }
            else
            {
                // Offline/Local Mode: Display message locally
                Debug.Log("ChatPresenter: VivoxManager not available. Using local mode.");
                HandleLocalSend(message);
            }
        }

        private void HandleLocalSend(string message)
        {
            // Local mode: Display the message immediately
            string playerName = "Player"; // TODO: Get from UserData or Settings
            ChatData localData = new ChatData(playerName, message);
            _chatUI.AddMessage(localData);
            _chatUI.ClearInput();
        }

        private void HandleMessageReceived(string sender, string message)
        {
            Debug.Log($"[ChatPresenter] HandleMessageReceived called: {sender}: {message}");
            
            if (_chatUI == null)
            {
                Debug.LogError("[ChatPresenter] _chatUI is NULL! Cannot display message.");
                return;
            }
            
            // 메인 스레드에서 실행되도록 보장 (Unity Event Callback 등은 메인 스레드지만, 혹시 모를 비동기 콜백 대비)
            // VivoxSDK callbacks are generally main thread, but good to be safe or check doc. 
            // Unity Vivox package callbacks are dispatched to the main thread.
            
            ChatData newData = new ChatData(sender, message);
            Debug.Log($"[ChatPresenter] Calling _chatUI.AddMessage for: {sender}: {message}");
            _chatUI.AddMessage(newData);
        }
    }
}
