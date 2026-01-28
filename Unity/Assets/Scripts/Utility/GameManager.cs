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
            if (autoSetup)
            {
                AutoSetup();
            }
        }

        private void Start()
        {
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
                    GameObject cheatObj = new GameObject("CheatManager");
                    cheatManager = cheatObj.AddComponent<CheatManager>();
                }
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
