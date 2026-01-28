using UnityEngine;
using Game.Core;

namespace Game.Utility
{
    /// <summary>
    /// 게임 매니저 (초기 설정 및 참조 관리)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("핵심 참조")]
        [SerializeField] private TroopManager troopManager;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private CheatManager cheatManager;

        [Header("자동 생성")]
        [SerializeField] private bool autoSetup = true;

        private void Awake()
        {
            Debug.Log("[GameManager] Awake 호출됨");
            if (autoSetup)
            {
                Debug.Log("[GameManager] AutoSetup 시작");
                AutoSetup();
            }
        }

        private void Start()
        {
            Debug.Log("[GameManager] Start 호출됨");
            SetupReferences();
        }

        /// <summary>
        /// 자동 설정
        /// </summary>
        private void AutoSetup()
        {
            // TroopManager 찾기 또는 생성
            if (troopManager == null)
            {
                troopManager = FindObjectOfType<TroopManager>();
                if (troopManager == null)
                {
                    GameObject troopObj = new GameObject("TroopManager");
                    troopManager = troopObj.AddComponent<TroopManager>();
                }
            }

            // CameraController 찾기 또는 생성
            if (cameraController == null)
            {
                cameraController = Camera.main?.GetComponent<CameraController>();
                if (cameraController == null && Camera.main != null)
                {
                    cameraController = Camera.main.gameObject.AddComponent<CameraController>();
                }
            }

            // InputHandler 찾기 또는 생성
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
                if (inputHandler == null)
                {
                    GameObject inputObj = new GameObject("InputHandler");
                    inputHandler = inputObj.AddComponent<PlayerInputHandler>();
                }
            }

            // CheatManager 찾기 또는 생성
            if (cheatManager == null)
            {
                cheatManager = FindObjectOfType<CheatManager>();
                if (cheatManager == null)
                {
                    Debug.Log("[GameManager] CheatManager를 찾을 수 없어서 새로 생성합니다.");
                    GameObject cheatObj = new GameObject("CheatManager");
                    cheatManager = cheatObj.AddComponent<CheatManager>();
                    Debug.Log("[GameManager] CheatManager 생성 완료!");
                }
                else
                {
                    Debug.Log("[GameManager] 기존 CheatManager를 찾았습니다.");
                }
            }
            else
            {
                Debug.Log("[GameManager] CheatManager가 이미 할당되어 있습니다.");
            }
        }

        /// <summary>
        /// 참조 설정
        /// </summary>
        private void SetupReferences()
        {
            if (cameraController != null && troopManager != null)
            {
                cameraController.SetTroopManager(troopManager);
            }

            if (inputHandler != null && troopManager != null)
            {
                inputHandler.SetTroopManager(troopManager);
            }

            // CheatManager는 Inspector에서 직접 설정
        }
    }
}
