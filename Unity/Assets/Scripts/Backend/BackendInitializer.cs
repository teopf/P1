using UnityEngine;

namespace Backend
{
    /// <summary>
    /// 게임 시작 시 BackendManager를 자동으로 초기화하는 클래스
    /// </summary>
    public class BackendInitializer : MonoBehaviour
    {
        [Header("UGS 초기화 설정")]
        [Tooltip("게임 시작 즉시 UGS를 초기화할지 여부")]
        [SerializeField] private bool initializeOnStart = true;

        // 화면에 상태를 표시하기 위한 간단한 GUI 변수 (디버깅용)
        private string statusMessage = "UGS 초기화 대기 중...";

        private async void Start()
        {
            if (!initializeOnStart) return;

            Debug.Log("=== UGS 초기화 시작 ===");

            try
            {
                // BackendManager 초기화
                await BackendManager.Instance.InitializeAsync();
                
                string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                statusMessage = $"✅ UGS 초기화 성공!\nPlayer ID: {playerId}";
                
                Debug.Log("✅ UGS 초기화 성공!");
                Debug.Log($"로그인된 Player ID: {playerId}");
            }
            catch (System.Exception e)
            {
                statusMessage = $"❌ UGS 초기화 실패: {e.Message}";
                Debug.LogError($"❌ UGS 초기화 실패: {e.Message}");
                Debug.LogError($"스택 트레이스: {e.StackTrace}");
            }
        }

        /// <summary>
        /// 수동으로 UGS를 초기화하는 메서드 (버튼 등에서 호출 가능)
        /// </summary>
        public async void InitializeManually()
        {
            statusMessage = "수동 초기화 시작...";
            Debug.Log("수동 초기화 시작...");
            try 
            {
                await BackendManager.Instance.InitializeAsync();
                string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                statusMessage = $"✅ 수동 초기화 성공!\nPlayer ID: {playerId}";
                Debug.Log("수동 초기화 완료!");
            }
            catch(System.Exception e)
            {
                statusMessage = $"❌ 수동 초기화 실패: {e.Message}";
                Debug.LogError(e);
            }
        }


    }
}
