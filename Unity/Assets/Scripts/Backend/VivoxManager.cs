using System;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace Backend
{
    public class VivoxManager : MonoBehaviour
    {
        public static VivoxManager Instance { get; private set; }

        public event Action<string, string> OnChannelMessageReceived; // sender, message

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                await VivoxService.Instance.InitializeAsync();
                Debug.Log("VivoxManager: Vivox Initialized.");
                
                VivoxService.Instance.ChannelMessageReceived += OnVivoxChannelMessageReceived;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"VivoxManager: Initialization Failed - {e.Message}");
                return false;
            }
        }

        private void OnVivoxChannelMessageReceived(VivoxMessage message)
        {
            // 본인 메시지도 올 수 있음. 필요에 따라 필터링 가능.
            OnChannelMessageReceived?.Invoke(message.SenderDisplayName, message.MessageText);
            Debug.Log($"Vivox Message Received from {message.SenderDisplayName}: {message.MessageText}");
        }

        public async Task LoginToVivoxAsync(string displayName)
        {
            try
            {
                LoginOptions options = new LoginOptions();
                options.DisplayName = displayName;
                
                await VivoxService.Instance.LoginAsync(options);
                Debug.Log($"VivoxManager: Logged in as {displayName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"VivoxManager: Login Failed - {e.Message}");
                throw;
            }
        }

        public async Task JoinChannelAsync(string channelName)
        {
            try
            {
                // JoinGroupChannelAsync is the correct API for Vivox 16.x non-positional channels.
                // ChatCapability is optional, defaults to TextAndAudio.
                // ChannelOptions is optional.
                await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextOnly);
                Debug.Log($"VivoxManager: Joined Channel {channelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"VivoxManager: Join Channel Failed - {e.Message}");
                throw;
            }
        }

        public async Task SendChannelMessageAsync(string channelName, string message)
        {
            try
            {
                // Note: ChannelName must match the one used in JoinChannelAsync
                await VivoxService.Instance.SendChannelTextMessageAsync(channelName, message);
                Debug.Log($"VivoxManager: Sent Message to {channelName}: {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"VivoxManager: Send Message Failed - {e.Message}");
                throw;
            }
        }

        private void OnDestroy()
        {
            if (VivoxService.Instance != null)
            {
                VivoxService.Instance.ChannelMessageReceived -= OnVivoxChannelMessageReceived;
            }
        }
    }
}
