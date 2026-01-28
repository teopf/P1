using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudCode;
using UnityEngine;

namespace Backend
{
    public class BackendManager : MonoBehaviour
    {
        public static BackendManager Instance { get; private set; }

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

        public async Task InitializeAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("BackendManager: Unity Services Initialized.");

                // 인증
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"BackendManager: Signed in as {AuthenticationService.Instance.PlayerId}");
                }

                // Vivox 초기화 (Instantiate if missing)
                if (VivoxManager.Instance == null)
                {
                    GameObject vivoxObj = new GameObject("VivoxManager");
                    vivoxObj.AddComponent<VivoxManager>();
                    // Instance is set in Awake(), which is called immediately on AddComponent.
                }

                // Vivox 초기화 및 로그인 (인증 후 진행)
                if (VivoxManager.Instance != null)
                {
                    bool initSuccess = await VivoxManager.Instance.InitializeAsync();
                    
                    if (initSuccess)
                    {
                        // 닉네임은 일단 PlayerId의 일부나 임시 이름 사용. 실제 게임에서는 PlayerDataManager에서 가져온 닉네임 사용 권장.
                        string nickName = $"User_{AuthenticationService.Instance.PlayerId.Substring(0, 4)}";
                        await VivoxManager.Instance.LoginToVivoxAsync(nickName);
                        
                        // 기본 글로벌 채널 입장
                        await VivoxManager.Instance.JoinChannelAsync("GlobalChat");
                    }
                    else
                    {
                        Debug.LogWarning("BackendManager: Vivox Initialization Failed. Chat will not be available. Please check Project Settings > Services > Vivox.");
                    }
                }
                else
                {
                    Debug.LogError("BackendManager: VivoxManager Instance is still null after creation attempt!");
                }
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                throw;
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager Initializaion Failed: {e.Message}");
                throw;
            }
        }

        public async Task<T> CallCloudFunction<T>(string functionName, Dictionary<string, object> args)
        {
            try
            {
                // Cloud Code 호출
                var result = await CloudCodeService.Instance.CallEndpointAsync<T>(functionName, args);
                return result;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Cloud Code Error ({functionName}): {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unknown Error calling {functionName}: {e.Message}");
                throw;
            }
        }
    }
}
