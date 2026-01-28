using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// 플레이어 입력 처리 (WASD 이동)
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private TroopManager troopManager;

        [Header("설정")]
        [SerializeField] private float inputDeadzone = 0.1f;

        private void Update()
        {
            HandleMovementInput();
        }

        private void HandleMovementInput()
        {
            if (troopManager == null) return;

            float horizontal = 0f;
            float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
            // New Input System 사용
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed) horizontal += 1f;
                if (keyboard.sKey.isPressed) vertical -= 1f;
                if (keyboard.wKey.isPressed) vertical += 1f;
            }
#else
            // Legacy Input 사용
            horizontal = Input.GetAxisRaw("Horizontal"); // A/D
            vertical = Input.GetAxisRaw("Vertical");     // W/S
#endif

            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

            if (inputDirection.magnitude > inputDeadzone)
            {
                // 이동 명령 전달
                troopManager.CommandMove(inputDirection.normalized);
            }
            else
            {
                // 입력이 없으면 자동 모드로 전환
                foreach (var member in troopManager.TroopMembers)
                {
                    if (member != null)
                    {
                        member.StopPlayerMoveCommand();
                    }
                }
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
