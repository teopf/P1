using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.InputSystem;

namespace Backend
{ 
    /// <summary>
    /// UGS 백엔드 상태 표시기
    /// - Player ID: 좌측 상단 표시
    /// - Server Log: 별도의 콘솔 윈도우(토글 가능)
    /// </summary>
    public class BackendStatusDisplay : MonoBehaviour
    {
        [Header("Player ID UI")]
        [Tooltip("Player ID 표시 여부")]
        [SerializeField] private bool showPlayerID = true;
        [SerializeField] private int playerIDFontSize = 24;
        [SerializeField] private Color playerIDColor = Color.white;

        [Header("Log Window UI")]
        [Tooltip("로그 윈도우 초기 표시 여부")]
        [SerializeField] private bool showLogWindow = false;
        [SerializeField] private int logFontSize = 18;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        // UI References
        private GameObject _canvasObj;
        private Text _playerIDText;
        private GameObject _logPanelObj;
        private Text _logText;
        
        private string _logData = "";
        private string _currentPlayerID = "Not Signed In";

        void Start()
        {
            CreateUI();
            
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

        private void HandleInput()
        {
            // F1 키로 로그 윈도우 토글 (Legacy Input / New Input 둘 다 대응)
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
                _playerIDText.fontSize = playerIDFontSize;
                _playerIDText.color = playerIDColor;
            }

            // Log Text 업데이트
            if (_logText != null && showLogWindow)
            {
                _logText.text = _logData;
                _logText.fontSize = logFontSize;
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

            // 로그가 너무 길어지면 자르기 (선택)
            if (_logData.Length > 2000)
            {
                _logData = _logData.Substring(_logData.Length - 2000);
            }
        }

        private void CreateUI()
        {
            // 1. Canvas 생성
            _canvasObj = new GameObject("BackendStatusCanvas");
            var canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // 최상단 노출
            _canvasObj.AddComponent<CanvasScaler>();
            _canvasObj.AddComponent<GraphicRaycaster>();
            if (transform.parent != null) _canvasObj.transform.SetParent(transform, false);

            // 2. Player ID Text (좌측 상단)
            CreatePlayerIDText();

            // 3. Log Window (하단 패널)
            CreateLogWindow();

            // 4. Toggle Hint Text (우측 상단)
            CreateToggleHintText();
        }

        private void CreatePlayerIDText()
        {
            var textObj = new GameObject("PlayerIDText");
            textObj.transform.SetParent(_canvasObj.transform, false);
            
            _playerIDText = textObj.AddComponent<Text>();
            _playerIDText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _playerIDText.alignment = TextAnchor.UpperLeft;
            
            // RectTransform 설정 (Top-Left)
            RectTransform rect = _playerIDText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(400, 50);
            
            // 그림자 효과 추가
            textObj.AddComponent<Outline>().effectDistance = new Vector2(1, -1);
        }

        private void CreateLogWindow()
        {
            // Panel 생성
            _logPanelObj = new GameObject("LogPanel");
            _logPanelObj.transform.SetParent(_canvasObj.transform, false);
            
            // 반투명 검은 배경
            Image bg = _logPanelObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // 하단 1/3 차지하도록 설정
            RectTransform rect = _logPanelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0.35f); // 화면 하단 35%
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Log Text
            var logTextObj = new GameObject("LogText");
            logTextObj.transform.SetParent(_logPanelObj.transform, false);
            
            _logText = logTextObj.AddComponent<Text>();
            _logText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _logText.color = Color.green;
            _logText.alignment = TextAnchor.LowerLeft;
            _logText.verticalOverflow = VerticalWrapMode.Truncate;
            
            // 텍스트 영역 설정 (패딩)
            RectTransform textRect = _logText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            _logPanelObj.SetActive(showLogWindow);
        }

        private void CreateToggleHintText()
        {
            var textObj = new GameObject("ToggleHintText");
            // 검은색 패널(_logPanelObj)의 자식으로 설정
            textObj.transform.SetParent(_logPanelObj.transform, false);
            
            var hintText = textObj.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintText.alignment = TextAnchor.UpperRight;
            hintText.text = $"Log: {toggleKey}";
            hintText.fontSize = 20;
            hintText.color = Color.white;
            
            // RectTransform 설정 (Top-Right)
            RectTransform rect = hintText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(200, 50);
            
            // 그림자 효과
            textObj.AddComponent<Outline>().effectDistance = new Vector2(1, -1);
        }
    }
}
