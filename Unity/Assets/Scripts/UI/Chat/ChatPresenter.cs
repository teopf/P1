using UnityEngine;
using Backend;
using Data;

namespace UI.Chat
{
    public class ChatPresenter : MonoBehaviour
    {
        [SerializeField] private ChatUI _chatUI;

        private void Start()
        {
            if (_chatUI == null)
                _chatUI = GetComponent<ChatUI>();

            // UI 이벤트 연결
            _chatUI.OnSendButtonClicked += HandleSendButtonClicked;

            // 백엔드 이벤트 연결 (Vivox)
            if (VivoxManager.Instance != null)
            {
                VivoxManager.Instance.OnChannelMessageReceived += HandleMessageReceived;
            }
        }

        private void OnDestroy()
        {
            if (_chatUI != null)
                _chatUI.OnSendButtonClicked -= HandleSendButtonClicked;

            if (VivoxManager.Instance != null)
            {
                VivoxManager.Instance.OnChannelMessageReceived -= HandleMessageReceived;
            }
        }

        private async void HandleSendButtonClicked(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

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
            string playerName = "Player"; // TODO: Get from GameData or Settings
            ChatData localData = new ChatData(playerName, message);
            _chatUI.AddMessage(localData);
            _chatUI.ClearInput();
        }

        private void HandleMessageReceived(string sender, string message)
        {
            // 메인 스레드에서 실행되도록 보장 (Unity Event Callback 등은 메인 스레드지만, 혹시 모를 비동기 콜백 대비)
            // VivoxSDK callbacks are generally main thread, but good to be safe or check doc. 
            // Unity Vivox package callbacks are dispatched to the main thread.
            
            ChatData newData = new ChatData(sender, message);
            _chatUI.AddMessage(newData);
        }
    }
}
