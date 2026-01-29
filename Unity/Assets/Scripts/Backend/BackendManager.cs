using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
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

        // ============================================
        // CLOUD SAVE METHODS
        // ============================================

        /// <summary>
        /// Cloud Save에 데이터를 저장합니다.
        /// </summary>
        /// <param name="key">저장할 키</param>
        /// <param name="data">저장할 데이터 (JSON으로 직렬화 가능한 객체)</param>
        public async Task SaveDataToCloud(string key, object data)
        {
            try
            {
                var dataToSave = new Dictionary<string, object>
                {
                    { key, data }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
                Debug.Log($"BackendManager: Cloud Save 성공 (Key: {key})");
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"BackendManager: Cloud Save 실패 (Key: {key}): {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager: Cloud Save 알 수 없는 오류 (Key: {key}): {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cloud Save에서 데이터를 불러옵니다.
        /// </summary>
        /// <typeparam name="T">반환할 데이터 타입</typeparam>
        /// <param name="key">불러올 키</param>
        /// <returns>불러온 데이터, 데이터가 없으면 default(T)</returns>
        public async Task<T> LoadDataFromCloud<T>(string key)
        {
            try
            {
                var keysToLoad = new HashSet<string> { key };
                var results = await CloudSaveService.Instance.Data.Player.LoadAsync(keysToLoad);

                if (results.TryGetValue(key, out var item))
                {
                    // GetAsString()을 사용하여 JSON 문자열로 가져옵니다
                    string jsonData = item.Value.GetAsString();
                    Debug.Log($"BackendManager: Cloud Load 성공 (Key: {key})");

                    // T가 string인 경우 직접 반환
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)jsonData;
                    }

                    // 그 외의 경우 JSON으로 역직렬화 필요 (상위 레이어에서 처리)
                    return (T)(object)jsonData;
                }
                else
                {
                    Debug.Log($"BackendManager: Cloud에 데이터 없음 (Key: {key})");
                    return default(T);
                }
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"BackendManager: Cloud Load 실패 (Key: {key}): {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager: Cloud Load 알 수 없는 오류 (Key: {key}): {e.Message}");
                throw;
            }
        }
    }
}
