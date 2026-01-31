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

        // ============================================
        // 백엔드 상태 알림 이벤트
        // ============================================

        /// <summary>로드 시작 시 호출</summary>
        public event Action OnLoadStart;
        /// <summary>로드 완료 시 호출 (성공 여부 전달)</summary>
        public event Action<bool> OnLoadComplete;

        /// <summary>저장 시작 시 호출</summary>
        public event Action OnSaveStart;
        /// <summary>저장 완료 시 호출 (성공 여부 전달)</summary>
        public event Action<bool> OnSaveComplete;

        /// <summary>게임 초기화(UGS + 인증 + Vivox) 시작 시 호출</summary>
        public event Action OnInitStart;
        /// <summary>게임 초기화 완료 시 호출 (성공 여부 전달)</summary>
        public event Action<bool> OnInitComplete;

        /// <summary>초기화(삭제) 시작 시 호출</summary>
        public event Action OnResetStart;
        /// <summary>초기화(삭제) 완료 시 호출 (성공 여부 전달)</summary>
        public event Action<bool> OnResetComplete;

        /// <summary>현재 초기화 진행 중인지 여부</summary>
        public bool IsInitializing { get; private set; }

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
            IsInitializing = true;
            OnInitStart?.Invoke();
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

                IsInitializing = false;
                OnInitComplete?.Invoke(true);
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                IsInitializing = false;
                OnInitComplete?.Invoke(false);
                throw;
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                IsInitializing = false;
                OnInitComplete?.Invoke(false);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager Initializaion Failed: {e.Message}");
                IsInitializing = false;
                OnInitComplete?.Invoke(false);
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
            OnSaveStart?.Invoke();
            try
            {
                var dataToSave = new Dictionary<string, object>
                {
                    { key, data }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
                Debug.Log($"BackendManager: Cloud Save 성공 (Key: {key})");
                OnSaveComplete?.Invoke(true);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"BackendManager: Cloud Save 실패 (Key: {key}): {e.Message}");
                OnSaveComplete?.Invoke(false);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager: Cloud Save 알 수 없는 오류 (Key: {key}): {e.Message}");
                OnSaveComplete?.Invoke(false);
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
            OnLoadStart?.Invoke();
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
                        OnLoadComplete?.Invoke(true);
                        return (T)(object)jsonData;
                    }

                    // 그 외의 경우 JSON으로 역직렬화 필요 (상위 레이어에서 처리)
                    OnLoadComplete?.Invoke(true);
                    return (T)(object)jsonData;
                }
                else
                {
                    Debug.Log($"BackendManager: Cloud에 데이터 없음 (Key: {key})");
                    OnLoadComplete?.Invoke(true);
                    return default(T);
                }
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"BackendManager: Cloud Load 실패 (Key: {key}): {e.Message}");
                OnLoadComplete?.Invoke(false);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager: Cloud Load 알 수 없는 오류 (Key: {key}): {e.Message}");
                OnLoadComplete?.Invoke(false);
                throw;
            }
        }

        /// <summary>
        /// Cloud Save의 모든 플레이어 데이터를 삭제합니다.
        /// </summary>
        public async Task DeleteAllData()
        {
            OnResetStart?.Invoke();
            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAllAsync();
                Debug.Log("BackendManager: Cloud Save 모든 데이터 삭제 성공");
                OnResetComplete?.Invoke(true);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"BackendManager: Cloud Save 데이터 삭제 실패: {e.Message}");
                OnResetComplete?.Invoke(false);
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager: Cloud Save 데이터 삭제 알 수 없는 오류: {e.Message}");
                OnResetComplete?.Invoke(false);
                throw;
            }
        }
    }
}
