using System;
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

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"BackendManager: Signed in as {AuthenticationService.Instance.PlayerId}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"BackendManager Initializaion Failed: {e.Message}");
                throw;
            }
        }

        public async Task<T> CallCloudFunction<T>(string functionName, object args)
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
