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

            // TODO: 현재 채널 이름을 관리하는 로직이 필요함. 일단 하드코딩.
            string currentChannel = "GlobalChat"; 

            if (VivoxManager.Instance != null)
            {
                await VivoxManager.Instance.SendChannelMessageAsync(currentChannel, message);
                // 내가 보낸 메시지는 Vivox 이벤트로 다시 들어오므로 여기서 UI 추가할 필요 없음 (Vivox 동작 방식에 따라 다름)
                // 만약 에코가 안 온다면 여기서 AddMessage 호출 필요. 보통은 온다.
                _chatUI.ClearInput();
            }
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
