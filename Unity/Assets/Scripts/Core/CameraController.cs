using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// 카메라 컨트롤러 (부대 중심점 팔로우)
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private TroopManager troopManager;

        [Header("카메라 설정")]
        [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool lookAtTarget = true;

        private void LateUpdate()
        {
            if (troopManager == null) return;

            // 목표 위치 계산
            Vector3 targetPosition = troopManager.TroopCenter + offset;

            // 부드러운 이동
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // 부대 중심점 바라보기
            if (lookAtTarget)
            {
                transform.LookAt(troopManager.TroopCenter);
            }
        }

        /// <summary>
        /// TroopManager 설정
        /// </summary>
        public void SetTroopManager(TroopManager manager)
        {
            troopManager = manager;
        }
    }
}
