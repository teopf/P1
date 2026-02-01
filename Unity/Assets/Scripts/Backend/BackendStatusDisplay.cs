using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.InputSystem;
using TMPro;

namespace Backend
{ 
    /// <summary>
    /// UGS 백엔드 상태 표시기 (HUD_Canvas 통합 버전)
    /// - Player ID: AA1_TopPanel > PlayerInfo_Group > Backend_PID_Text
    /// - Server Log: HUD_Canvas > Backend_LogPanel (F1 토글)
    /// 
    /// 사전 요구사항:
    /// 1. Tools > UI > Generate HUD Layout 실행
    /// 2. Tools > UI > Setup HUD User Data Display 실행
    /// 3. Tools > UI > Setup Backend Status Display 실행
    /// </summary>
    public class BackendStatusDisplay : MonoBehaviour
    {
        [Header("UI 참조 (자동 연결)")]
        [SerializeField] private TextMeshProUGUI _playerIDText;
        [SerializeField] private GameObject _logPanelObj;
        [SerializeField] private TextMeshProUGUI _logText;

        [Header("설정")]
        [SerializeField] private bool showPlayerID = true;
        [SerializeField] private bool showLogWindow = false;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        
        private string _logData = "";
        private string _currentPlayerID = "Not Signed In";

        void Start()
        {
            // UI 요소 자동 찾기
            FindUIReferences();
            
            Log("BackendStatusDisplay Started.");

            if (BackendManager.Instance != null)
            {
                // 주기적 상태 체크 (5초)
                InvokeRepeating(nameof(CheckServiceStatus), 1f, 5f);
            }
            else
            {
                Log("Warning: BackendManager not found in scene.");
            }
        }

        void Update()
        {
            UpdateUI();
            HandleInput();
        }

        /// <summary>
        /// HUD_Canvas에서 UI 요소 자동 찾기
        /// </summary>
        private void FindUIReferences()
        {
            // Backend_PID_Text 찾기 (PlayerID_Text의 자식)
            if (_playerIDText == null)
            {
                Transform pidTrans = transform.root.Find("HUD_Canvas/AA1_TopPanel/PlayerInfo_Group/PlayerID_Text/Backend_PID_Text");
                // 만약 못찾으면 이전 경로(혹시 모를 호환성)도 시도해볼 수 있지만, Setup을 실행하라고 안내하는게 나음
                if (pidTrans == null) pidTrans = GameObject.Find("HUD_Canvas/AA1_TopPanel/PlayerInfo_Group/PlayerID_Text/Backend_PID_Text")?.transform;
                
                if (pidTrans != null)
                {
                    _playerIDText = pidTrans.GetComponent<TextMeshProUGUI>();
                }
            }

            // Backend_LogPanel 찾기
            if (_logPanelObj == null)
            {
                _logPanelObj = GameObject.Find("HUD_Canvas/Backend_LogPanel");
            }

            // LogText 찾기
            if (_logText == null && _logPanelObj != null)
            {
                Transform logTextTransform = _logPanelObj.transform.Find("LogText");
                if (logTextTransform != null)
                {
                    _logText = logTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            // 찾지 못한 경우 경고
            if (_playerIDText == null)
            {
                Debug.LogWarning("[BackendStatusDisplay] Backend_PID_Text를 찾을 수 없습니다. 'Tools > UI > Setup Backend Status Display' 메뉴를 실행해주세요.");
            }
            if (_logPanelObj == null)
            {
                Debug.LogWarning("[BackendStatusDisplay] Backend_LogPanel을 찾을 수 없습니다. 'Tools > UI > Setup Backend Status Display' 메뉴를 실행해주세요.");
            }
        }

        private void HandleInput()
        {
            // F1 키로 로그 윈도우 토글
            bool toggle = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) toggle = true;
#else
            if (Input.GetKeyDown(toggleKey)) toggle = true;
#endif

            if (toggle)
            {
                showLogWindow = !showLogWindow;
                if (_logPanelObj != null) _logPanelObj.SetActive(showLogWindow);
            }
        }

        private void UpdateUI()
        {
            // Player ID Text 업데이트
            if (_playerIDText != null)
            {
                _playerIDText.gameObject.SetActive(showPlayerID);
                _playerIDText.text = $"PID: {_currentPlayerID}";
            }

            // Log Text 업데이트
            if (_logText != null && showLogWindow)
            {
                _logText.text = _logData;
            }
            
            // Log Panel 활성/비활성 동기화
            if (_logPanelObj != null && _logPanelObj.activeSelf != showLogWindow)
            {
                _logPanelObj.SetActive(showLogWindow);
            }
        }

        private void CheckServiceStatus()
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Initialized)
                {
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        string pid = AuthenticationService.Instance.PlayerId;
                        if (_currentPlayerID != pid)
                        {
                            _currentPlayerID = pid;
                            Log($"Login Success: {pid}");
                        }
                    }
                    else
                    {
                        _currentPlayerID = "Not Signed In";
                    }
                }
                else
                {
                    _currentPlayerID = "Initializing...";
                }
            }
            catch (System.Exception e)
            {
                Log($"Error: {e.Message}");
            }
        }

        private void Log(string msg)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string line = $"[{timestamp}] {msg}";
            
            _logData += line + "\n";
            Debug.Log($"[BackendUI] {msg}");

            // 로그가 너무 길어지면 자르기
            if (_logData.Length > 2000)
            {
                _logData = _logData.Substring(_logData.Length - 2000);
            }
        }
    }
}
