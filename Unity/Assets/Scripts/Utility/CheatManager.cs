using UnityEngine;
using Game.Core;
using Game.AI;
using Game.Data;
using System.Linq;
using Backend;
using BigInteger = System.Numerics.BigInteger;

namespace Game.Utility
{
    /// <summary>
    /// 치트 명령어 관리자
    /// </summary>
    public class CheatManager : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private TroopManager troopManager;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnArea;

        [Header("데이터 경로")]
        [SerializeField] private string characterDataPath = "Assets/GameData/Characters";
        [SerializeField] private string enemyDataPath = "Assets/GameData/Enemies";

        [Header("기본 프리팹")]
        [SerializeField] private GameObject playerCharacterPrefab;
        [SerializeField] private GameObject enemyCharacterPrefab;

        private CharacterData[] allCharacters;
        private EnemyData[] allEnemies;

        private void Start()
        {
            Debug.Log("[CheatManager] Start 호출됨");
            
            // TroopManager 자동 찾기
            if (troopManager == null)
            {
                troopManager = FindObjectOfType<TroopManager>();
                if (troopManager != null)
                {
                    Debug.Log("[CheatManager] TroopManager 자동 할당됨");
                }
                else
                {
                    Debug.LogWarning("[CheatManager] TroopManager를 찾을 수 없습니다. GameManager가 실행 중인지 확인하세요.");
                }
            }

            // SpawnPoint 자동 설정
            if (playerSpawnPoint == null)
            {
                GameObject spawnObj = new GameObject("PlayerSpawnPoint");
                playerSpawnPoint = spawnObj.transform;
                Debug.Log("[CheatManager] PlayerSpawnPoint 자동 생성됨");
            }

            if (enemySpawnArea == null)
            {
                GameObject enemyObj = new GameObject("EnemySpawnArea");
                enemySpawnArea = enemyObj.transform;
                enemySpawnArea.position = Vector3.forward * 10f;
                Debug.Log("[CheatManager] EnemySpawnArea 자동 생성됨");
            }

            LoadGameData();
            
            // 기본 프리팹이 없으면 생성
            if (playerCharacterPrefab == null)
            {
                playerCharacterPrefab = CreateDefaultPlayerPrefab();
            }
            if (enemyCharacterPrefab == null)
            {
                enemyCharacterPrefab = CreateDefaultEnemyPrefab();
            }
        }

        private void Update()
        {
            // 콘솔 명령어 입력 (New Input System 사용)
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.f5Key.wasPressedThisFrame)  // F1 → F5로 변경
            {
                Debug.Log("[CheatManager] F5 키 감지 - SpawnTroop 호출");
                SpawnTroop();
            }
            if (keyboard.f6Key.wasPressedThisFrame)  // F2 → F6로 변경
            {
                Debug.Log("[CheatManager] F6 키 감지 - SpawnEnemy 호출");
                SpawnEnemy(5);
            }
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                UseAction(0);
            }
            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                UseAction(1);
            }
            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                UseAction(2);
            }
            // UserData 치트 키
            if (keyboard.f7Key.wasPressedThisFrame)
            {
                CheatAddGold();
            }
            if (keyboard.f8Key.wasPressedThisFrame)
            {
                CheatAddGem();
            }
            if (keyboard.f9Key.wasPressedThisFrame)
            {
                CheatAddExp();
            }
#else
            // Legacy Input 사용
            if (Input.GetKeyDown(KeyCode.F5))  // F1 → F5로 변경
            {
                Debug.Log("[CheatManager] F5 키 감지 - SpawnTroop 호출");
                SpawnTroop();
            }
            if (Input.GetKeyDown(KeyCode.F6))  // F2 → F6로 변경
            {
                Debug.Log("[CheatManager] F6 키 감지 - SpawnEnemy 호출");
                SpawnEnemy(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseAction(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseAction(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UseAction(2);
            }
            // UserData 치트 키
            if (Input.GetKeyDown(KeyCode.F7))
            {
                CheatAddGold();
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                CheatAddGem();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                CheatAddExp();
            }
#endif
        }

        /// <summary>
        /// 게임 데이터 로드
        /// </summary>
        private void LoadGameData()
        {
#if UNITY_EDITOR
            allCharacters = UnityEditor.AssetDatabase.FindAssets("t:CharacterData", new[] { characterDataPath })
                .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            allEnemies = UnityEditor.AssetDatabase.FindAssets("t:EnemyData", new[] { enemyDataPath })
                .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            Debug.Log($"[CheatManager] 캐릭터 {allCharacters.Length}개, 적 {allEnemies.Length}개 로드됨");
#endif
        }

        /// <summary>
        /// SpawnTroop: 무작위 10개 캐릭터로 부대 구성
        /// </summary>
        [ContextMenu("SpawnTroop")]
        public void SpawnTroop()
        {
            if (troopManager == null)
            {
                Debug.LogError("[CheatManager] TroopManager가 없습니다!");
                return;
            }

            if (allCharacters == null || allCharacters.Length == 0)
            {
                Debug.LogError("[CheatManager] 캐릭터 데이터가 없습니다!");
                return;
            }

            // 기존 부대 제거
            troopManager.ClearTroop();

            // 무작위 10명 생성
            for (int i = 0; i < 10; i++)
            {
                CharacterData randomChar = allCharacters[Random.Range(0, allCharacters.Length)];
                int randomStar = Random.Range(1, 6);
                
                SpawnCharacter(randomChar.characterName, randomStar, i);
            }

            Debug.Log("[CheatManager] SpawnTroop 완료!");
        }

        /// <summary>
        /// SpawnCharacter: 특정 캐릭터 추가
        /// </summary>
        public void SpawnCharacter(string characterName, int starCount, int formationIndex = -1)
        {
            if (troopManager == null || allCharacters == null) return;

            CharacterData charData = allCharacters.FirstOrDefault(c => c.characterName == characterName);
            if (charData == null)
            {
                Debug.LogWarning($"[CheatManager] '{characterName}' 캐릭터를 찾을 수 없습니다.");
                return;
            }

            CharacterSpecData spec = charData.GetSpecByStarCount(starCount);
            if (spec == null)
            {
                Debug.LogWarning($"[CheatManager] '{characterName}' {starCount}성 스펙을 찾을 수 없습니다.");
                return;
            }

            // 생성 위치 계산
            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            if (formationIndex >= 0)
            {
                // 대형 배치 (2x5 그리드)
                int row = formationIndex / 5;
                int col = formationIndex % 5;
                spawnPos += new Vector3(col * 1.5f - 3f, 0, -row * 1.5f);
            }

            // 캐릭터 생성
            GameObject charObj = Instantiate(playerCharacterPrefab, spawnPos, Quaternion.identity);
            charObj.name = $"{characterName}_{starCount}★";
            charObj.tag = "Player";
            charObj.SetActive(true); // 활성화

            var playerChar = charObj.GetComponent<PlayerCharacter>();
            if (playerChar != null)
            {
                // CharacterSpec 설정 (리플렉션 사용)
                var specField = typeof(Game.Core.UnitBase).GetField("characterSpec", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                specField?.SetValue(playerChar, spec);

                // 레벨 설정
                var levelField = typeof(Game.Core.UnitBase).GetField("characterLevel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                levelField?.SetValue(playerChar, 1);

                // 스탯 초기화 (HP 바 업데이트 포함)
                playerChar.InitializeStats();

                // 상대 위치 설정
                if (formationIndex >= 0)
                {
                    int row = formationIndex / 5;
                    int col = formationIndex % 5;
                    playerChar.SetRelativePosition(new Vector3(col * 1.5f - 3f, 0, -row * 1.5f));
                }

                troopManager.AddMember(playerChar);
            }

            Debug.Log($"[CheatManager] {characterName} {starCount}★ 생성됨");
        }

        /// <summary>
        /// UseAction: 스킬 강제 사용
        /// </summary>
        public void UseAction(int skillNum)
        {
            if (troopManager == null) return;

            troopManager.CommandUseSkill(skillNum);
            Debug.Log($"[CheatManager] 스킬 {skillNum + 1} 강제 사용!");
        }

        /// <summary>
        /// SpawnEnemy: 적 소환
        /// </summary>
        public void SpawnEnemy(int count)
        {
            if (allEnemies == null || allEnemies.Length == 0)
            {
                Debug.LogError("[CheatManager] 적 데이터가 없습니다!");
                return;
            }

            Vector3 spawnCenter = enemySpawnArea != null ? enemySpawnArea.position : Vector3.forward * 10f;

            for (int i = 0; i < count; i++)
            {
                EnemyData randomEnemy = allEnemies[Random.Range(0, allEnemies.Length)];
                
                // 무작위 위치
                Vector3 randomOffset = new Vector3(
                    Random.Range(-5f, 5f),
                    0,
                    Random.Range(-5f, 5f)
                );
                Vector3 spawnPos = spawnCenter + randomOffset;

                // 적 생성
                GameObject enemyObj = Instantiate(enemyCharacterPrefab, spawnPos, Quaternion.identity);
                enemyObj.name = randomEnemy.enemyName;
                enemyObj.tag = "Enemy";
                enemyObj.SetActive(true); // 활성화

                var enemyChar = enemyObj.GetComponent<EnemyCharacter>();
                if (enemyChar != null)
                {
                    enemyChar.SetEnemyData(randomEnemy);
                }
            }

            Debug.Log($"[CheatManager] 적 {count}마리 소환됨!");
        }

        /// <summary>
        /// 기본 플레이어 프리팹 생성
        /// </summary>
        private GameObject CreateDefaultPlayerPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = "PlayerCharacterPrefab";
            // CharacterController 제거 - 겠침 허용
            prefab.AddComponent<PlayerCharacter>();
            // 프리팹은 숨김 처리 (비활성화하지 않음)
            prefab.transform.position = new Vector3(0, -1000, 0);
            DontDestroyOnLoad(prefab);
            return prefab;
        }

        /// <summary>
        /// 기본 적 프리팹 생성
        /// </summary>
        private GameObject CreateDefaultEnemyPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = "EnemyCharacterPrefab";
            prefab.GetComponent<Renderer>().material.color = Color.red;
            // CharacterController 제거 - 겠침 허용
            prefab.AddComponent<EnemyCharacter>();
            // 프리팹은 숨김 처리 (비활성화하지 않음)
            prefab.transform.position = new Vector3(0, -1000, 0);
            DontDestroyOnLoad(prefab);
            return prefab;
        }

        // ============================================
        // UserData 치트 메서드
        // ============================================

        /// <summary>
        /// 치트: 골드 추가 (F7)
        /// </summary>
        [ContextMenu("Cheat: Add 10000 Gold")]
        private void CheatAddGold()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogWarning("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            BigInteger amount = new BigInteger(10000);
            PlayerDataManager.Instance.AddGold(amount);
            Debug.Log($"[CheatManager] 골드 {amount} 추가됨!");
        }

        /// <summary>
        /// 치트: 젬 추가 (F8)
        /// </summary>
        [ContextMenu("Cheat: Add 100 Gem")]
        private void CheatAddGem()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogWarning("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            int amount = 100;
            PlayerDataManager.Instance.AddGem(amount);
            Debug.Log($"[CheatManager] 젬 {amount} 추가됨!");
        }

        /// <summary>
        /// 치트: 경험치 추가 (F9)
        /// </summary>
        [ContextMenu("Cheat: Add 500 Exp")]
        private void CheatAddExp()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogWarning("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            int amount = 500;
            PlayerDataManager.Instance.AddExp(amount);
            Debug.Log($"[CheatManager] 경험치 {amount} 추가됨!");
        }

        /// <summary>
        /// 치트: 레벨 설정
        /// </summary>
        [ContextMenu("Cheat: Set Level 10")]
        private void CheatSetLevel()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogWarning("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            int level = 10;
            PlayerDataManager.Instance.SetLevel(level);
            Debug.Log($"[CheatManager] 레벨 {level}로 설정됨!");
        }

        /// <summary>
        /// 치트: 엄청난 골드 추가 (테스트용)
        /// </summary>
        [ContextMenu("Cheat: Add 1 Billion Gold")]
        private void CheatAddHugeGold()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogWarning("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            BigInteger amount = new BigInteger(1000000000);
            PlayerDataManager.Instance.AddGold(amount);
            Debug.Log($"[CheatManager] 골드 {amount} (1B) 추가됨!");
        }

        // ============================================
        // CLOUD SAVE 테스트 메서드
        // ============================================

        /// <summary>
        /// 테스트: 클라우드에 강제 저장
        /// </summary>
        [ContextMenu("Test: Save to Cloud")]
        private async void TestSaveToCloud()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            Debug.Log("=== 클라우드 저장 테스트 시작 ===");
            bool success = await PlayerDataManager.Instance.SaveToCloud();

            if (success)
            {
                Debug.Log("✅ 클라우드 저장 테스트 성공!");
            }
            else
            {
                Debug.LogError("❌ 클라우드 저장 테스트 실패!");
            }
        }

        /// <summary>
        /// 테스트: 클라우드에서 강제 로드
        /// </summary>
        [ContextMenu("Test: Load from Cloud")]
        private async void TestLoadFromCloud()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[CheatManager] PlayerDataManager가 없습니다!");
                return;
            }

            Debug.Log("=== 클라우드 로드 테스트 시작 ===");
            await PlayerDataManager.Instance.InitializeData();
            Debug.Log($"✅ 클라우드 로드 완료: {PlayerDataManager.Instance.CurrentData}");
        }

        /// <summary>
        /// 테스트: 로컬 백업 삭제 (재테스트용)
        /// </summary>
        [ContextMenu("Test: Clear Local Backup")]
        private void TestClearLocalBackup()
        {
            const string LOCAL_BACKUP_KEY = "UserData_Backup";
            const string LOCAL_TIMESTAMP_KEY = "UserData_Timestamp";

            if (PlayerPrefs.HasKey(LOCAL_BACKUP_KEY))
            {
                PlayerPrefs.DeleteKey(LOCAL_BACKUP_KEY);
                PlayerPrefs.DeleteKey(LOCAL_TIMESTAMP_KEY);
                PlayerPrefs.Save();
                Debug.Log("✅ 로컬 백업 삭제 완료 (재테스트용)");
            }
            else
            {
                Debug.Log("로컬 백업이 없습니다.");
            }
        }
    }
}
