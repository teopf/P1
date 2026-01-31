using System.Collections;
using Backend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 백엔드 상태 변화를 토스트 메시지로 표시하는 알림 UI 컴포넌트.
    /// TopPanel 하단에 배치된 NotificationPanel에 부착하여 사용합니다.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("설정")]
        [SerializeField] private float _defaultDuration = 2f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        private Coroutine _currentCoroutine;

        private void OnEnable()
        {
            // BackendManager 이벤트 구독
            if (BackendManager.Instance != null)
            {
                SubscribeEvents();
            }
            else
            {
                // BackendManager가 아직 초기화되지 않은 경우 대기
                StartCoroutine(WaitForBackendManager());
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (BackendManager.Instance != null)
            {
                UnsubscribeEvents();
            }
        }

        /// <summary>
        /// BackendManager 인스턴스가 준비될 때까지 대기합니다.
        /// </summary>
        private IEnumerator WaitForBackendManager()
        {
            while (BackendManager.Instance == null)
            {
                yield return null;
            }
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            var bm = BackendManager.Instance;
            bm.OnInitStart += HandleInitStart;
            bm.OnInitComplete += HandleInitComplete;
            bm.OnLoadStart += HandleLoadStart;
            bm.OnLoadComplete += HandleLoadComplete;
            bm.OnSaveStart += HandleSaveStart;
            bm.OnSaveComplete += HandleSaveComplete;
            bm.OnResetStart += HandleResetStart;
            bm.OnResetComplete += HandleResetComplete;

            // 이미 초기화가 진행 중이면 놓친 OnInitStart를 보상
            if (bm.IsInitializing)
            {
                HandleInitStart();
            }
        }

        private void UnsubscribeEvents()
        {
            var bm = BackendManager.Instance;
            bm.OnInitStart -= HandleInitStart;
            bm.OnInitComplete -= HandleInitComplete;
            bm.OnLoadStart -= HandleLoadStart;
            bm.OnLoadComplete -= HandleLoadComplete;
            bm.OnSaveStart -= HandleSaveStart;
            bm.OnSaveComplete -= HandleSaveComplete;
            bm.OnResetStart -= HandleResetStart;
            bm.OnResetComplete -= HandleResetComplete;
        }

        // ============================================
        // 이벤트 핸들러
        // ============================================

        private void HandleInitStart()
        {
            // 게임 시작 직후 UGS 초기화~로드 완료까지 무한 대기
            ShowMessage("게임 데이터 초기화 및 로드 중...", -1f);
        }

        private void HandleInitComplete(bool success)
        {
            ShowMessage(success ? "게임 데이터 초기화 완료" : "게임 데이터 초기화 실패");
        }

        private void HandleLoadStart()
        {
            // 로드 중 메시지는 완료될 때까지 유지 (duration = -1)
            ShowMessage("유저 데이터 로드 중...", -1f);
        }

        private void HandleLoadComplete(bool success)
        {
            ShowMessage(success ? "유저 데이터 로드 완료" : "유저 데이터 로드 실패");
        }

        private void HandleSaveStart()
        {
            ShowMessage("저장 중...", -1f);
        }

        private void HandleSaveComplete(bool success)
        {
            ShowMessage(success ? "저장 성공" : "저장 실패");
        }

        private void HandleResetStart()
        {
            ShowMessage("데이터 초기화 중...", -1f);
        }

        private void HandleResetComplete(bool success)
        {
            ShowMessage(success ? "데이터 초기화 완료" : "데이터 초기화 실패");
        }

        // ============================================
        // 메시지 표시
        // ============================================

        /// <summary>
        /// 토스트 메시지를 표시합니다.
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="duration">표시 시간(초). -1이면 다음 메시지가 올 때까지 유지.</param>
        public void ShowMessage(string message, float duration = -2f)
        {
            // 기본값 처리 (-2는 기본 duration 사용)
            if (duration <= -2f)
            {
                duration = _defaultDuration;
            }

            // 진행 중인 코루틴 중단
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            _messageText.text = message;
            _canvasGroup.alpha = 1f;

            // duration이 -1이면 무한 대기 (다음 메시지에 의해 교체됨)
            if (duration >= 0f)
            {
                _currentCoroutine = StartCoroutine(FadeOutAfterDelay(duration));
            }
        }

        /// <summary>
        /// 일정 시간 후 페이드 아웃합니다.
        /// </summary>
        private IEnumerator FadeOutAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // 페이드 아웃
            float elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = 1f - (elapsed / _fadeOutDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _currentCoroutine = null;
        }
    }
}
