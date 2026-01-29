using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace Backend
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public UserData CurrentData { get; private set; }

        // Cloud Save 관련 상수
        private const string DATA_KEY = "UserData";
        private const string LOCAL_BACKUP_KEY = "UserData_Backup";
        private const string LOCAL_TIMESTAMP_KEY = "UserData_Timestamp";

        // 자동 저장 관련
        private bool isDirty = false;
        private const float AUTO_SAVE_INTERVAL = 180f; // 3분 (180초)

        // 데이터 변경 시 발생하는 이벤트 (변경된 데이터를 인자로 전달)
        public event Action<UserData> OnDataChanged;

        // 세분화된 이벤트 (특정 데이터 변경 시 발생)
        public event Action<int> OnLevelChanged;
        public event Action<int> OnExpChanged;
        public event Action<BigInteger> OnGoldChanged;
        public event Action<int> OnGemChanged;
        public event Action<int?> OnWeaponEquipped;
        public event Action<int?> OnArmorEquipped;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                CurrentData = new UserData(); // 초기 빈 데이터
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 자동 저장 코루틴 시작
            StartCoroutine(AutoSaveCoroutine());
        }

        /// <summary>
        /// 앱이 백그라운드로 전환될 때 호출됨 (모바일 대응)
        /// </summary>
        private async void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isDirty)
            {
                Debug.Log("PlayerDataManager: 앱 일시정지, 데이터 저장 중...");
                await SaveToCloud();
            }
        }

        /// <summary>
        /// 앱 종료 시 호출됨
        /// </summary>
        private async void OnApplicationQuit()
        {
            if (isDirty)
            {
                Debug.Log("PlayerDataManager: 앱 종료, 데이터 저장 중...");
                await SaveToCloud();

                // 저장 완료를 위해 짧은 대기 (비동기 작업 완료 보장)
                await Task.Delay(500);
            }
        }

        // 초기 데이터 로드 (Cloud Save 우선, 실패 시 로컬 백업)
        public async Task InitializeData()
        {
            Debug.Log("PlayerDataManager: 데이터 초기화 시작");

            // 1. 인증 대기
            int retryCount = 0;
            while (!AuthenticationService.Instance.IsSignedIn && retryCount < 50)
            {
                await Task.Delay(100); // 0.1초 대기
                retryCount++;
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("PlayerDataManager: 인증 실패. 로컬 데이터만 사용합니다.");
                LoadFromLocalOrCreateNew();
                return;
            }

            // 2. Cloud Save에서 로드 시도
            try
            {
                if (BackendManager.Instance != null)
                {
                    string jsonData = await BackendManager.Instance.LoadDataFromCloud<string>(DATA_KEY);

                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        UserData loadedData = UserData.FromJson(jsonData);
                        CurrentData = loadedData;

                        // 클라우드 데이터를 로컬에도 백업
                        SaveToLocal();

                        Debug.Log($"PlayerDataManager: 클라우드에서 데이터 로드 성공: {CurrentData}");
                        RefreshUI();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PlayerDataManager: 클라우드 로드 실패: {e.Message}. 로컬 백업 사용.");
            }

            // 3. 클라우드 로드 실패 시 로컬 백업 시도
            LoadFromLocalOrCreateNew();
        }

        /// <summary>
        /// 로컬 백업에서 로드하거나 새 데이터 생성
        /// </summary>
        private void LoadFromLocalOrCreateNew()
        {
            UserData localData = LoadFromLocal();

            if (localData != null)
            {
                CurrentData = localData;
                Debug.Log($"PlayerDataManager: 로컬 백업에서 데이터 로드: {CurrentData}");
            }
            else
            {
                CurrentData = new UserData();
                Debug.Log("PlayerDataManager: 신규 게임 시작. 기본 데이터 생성.");
            }

            RefreshUI();
        }

        // 서버로부터 받은 최신 데이터로 로컬 캐시를 갱신
        public void UpdateData(UserData newData)
        {
            if (newData == null) return;

            CurrentData = newData;
            NotifyDataChanged();
        }

        // 이벤트를 발생시켜 구독 중인 UI들을 갱신
        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(CurrentData);
            Debug.Log($"PlayerDataManager: Data updated. {CurrentData}");

            // 데이터 변경 시 로컬에 즉시 저장 및 dirty 플래그 설정
            SaveToLocal();
            isDirty = true;
        }

        // 강제로 UI 갱신이 필요할 때 호출
        public void RefreshUI()
        {
            NotifyDataChanged();
        }

        // ============================================
        // CONVENIENCE METHODS (Wrapper around UserData)
        // ============================================

        /// <summary>
        /// Add gold and trigger events
        /// </summary>
        public void AddGold(BigInteger amount)
        {
            if (CurrentData.AddGold(amount))
            {
                OnGoldChanged?.Invoke(CurrentData.Gold);
                NotifyDataChanged();
            }
        }

        /// <summary>
        /// Spend gold and trigger events
        /// </summary>
        public bool SpendGold(BigInteger amount)
        {
            if (CurrentData.SpendGold(amount))
            {
                OnGoldChanged?.Invoke(CurrentData.Gold);
                NotifyDataChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add gems and trigger events
        /// </summary>
        public void AddGem(int amount)
        {
            if (CurrentData.AddGem(amount))
            {
                OnGemChanged?.Invoke(CurrentData.Gem);
                NotifyDataChanged();
            }
        }

        /// <summary>
        /// Spend gems and trigger events
        /// </summary>
        public bool SpendGem(int amount)
        {
            if (CurrentData.SpendGem(amount))
            {
                OnGemChanged?.Invoke(CurrentData.Gem);
                NotifyDataChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add experience and trigger events. Handles level-ups automatically.
        /// </summary>
        public void AddExp(int amount)
        {
            int oldLevel = CurrentData.Level;
            int levelsGained = CurrentData.AddExp(amount);

            OnExpChanged?.Invoke(CurrentData.Exp);

            if (levelsGained > 0)
            {
                OnLevelChanged?.Invoke(CurrentData.Level);
                Debug.Log($"Congratulations! Leveled up from {oldLevel} to {CurrentData.Level}!");
            }

            NotifyDataChanged();
        }

        /// <summary>
        /// Set level directly (admin/cheat function)
        /// </summary>
        public void SetLevel(int newLevel)
        {
            CurrentData.SetLevel(newLevel);
            OnLevelChanged?.Invoke(CurrentData.Level);
            OnExpChanged?.Invoke(CurrentData.Exp);
            NotifyDataChanged();
        }

        /// <summary>
        /// Equip weapon and trigger events
        /// </summary>
        public void EquipWeapon(int weaponId)
        {
            CurrentData.EquipWeapon(weaponId);
            OnWeaponEquipped?.Invoke(CurrentData.EquippedWeaponId);
            NotifyDataChanged();
        }

        /// <summary>
        /// Unequip weapon and trigger events
        /// </summary>
        public void UnequipWeapon()
        {
            CurrentData.UnequipWeapon();
            OnWeaponEquipped?.Invoke(null);
            NotifyDataChanged();
        }

        /// <summary>
        /// Equip armor and trigger events
        /// </summary>
        public void EquipArmor(int armorId)
        {
            CurrentData.EquipArmor(armorId);
            OnArmorEquipped?.Invoke(CurrentData.EquippedArmorId);
            NotifyDataChanged();
        }

        /// <summary>
        /// Unequip armor and trigger events
        /// </summary>
        public void UnequipArmor()
        {
            CurrentData.UnequipArmor();
            OnArmorEquipped?.Invoke(null);
            NotifyDataChanged();
        }

        // ============================================
        // SERIALIZATION (for save/load)
        // ============================================

        /// <summary>
        /// Save current data to JSON string
        /// </summary>
        public string SaveToJson()
        {
            return CurrentData.ToJson();
        }

        /// <summary>
        /// Load data from JSON string
        /// </summary>
        public void LoadFromJson(string json)
        {
            UserData loaded = UserData.FromJson(json);
            UpdateData(loaded);
        }

        // ============================================
        // CLOUD SAVE
        // ============================================

        /// <summary>
        /// 클라우드에 데이터를 저장합니다.
        /// </summary>
        public async Task<bool> SaveToCloud()
        {
            if (BackendManager.Instance == null)
            {
                Debug.LogWarning("PlayerDataManager: BackendManager가 없습니다. 클라우드 저장 실패.");
                return false;
            }

            try
            {
                string jsonData = CurrentData.ToJson();
                await BackendManager.Instance.SaveDataToCloud(DATA_KEY, jsonData);

                isDirty = false;
                Debug.Log("PlayerDataManager: 클라우드 저장 성공");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerDataManager: 클라우드 저장 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 자동 저장 코루틴 (3분 간격)
        /// </summary>
        private IEnumerator AutoSaveCoroutine()
        {
            // 초기화가 완료될 때까지 대기
            while (CurrentData == null)
            {
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("PlayerDataManager: 자동 저장 시스템 시작 (3분 간격)");

            while (true)
            {
                yield return new WaitForSeconds(AUTO_SAVE_INTERVAL);

                // 변경사항이 있을 때만 저장
                if (isDirty)
                {
                    Debug.Log("PlayerDataManager: 자동 저장 실행 중...");

                    // Task를 시작하고 완료될 때까지 대기
                    var saveTask = SaveToCloud();
                    yield return new WaitUntil(() => saveTask.IsCompleted);
                }
            }
        }

        // ============================================
        // LOCAL BACKUP (PlayerPrefs)
        // ============================================

        /// <summary>
        /// 로컬 PlayerPrefs에 데이터를 저장합니다 (빠른 백업).
        /// </summary>
        private void SaveToLocal()
        {
            try
            {
                string json = CurrentData.ToJson();
                PlayerPrefs.SetString(LOCAL_BACKUP_KEY, json);
                PlayerPrefs.SetString(LOCAL_TIMESTAMP_KEY, System.DateTime.UtcNow.ToString("o"));
                PlayerPrefs.Save();

                Debug.Log("PlayerDataManager: 로컬 백업 저장 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerDataManager: 로컬 백업 저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 로컬 PlayerPrefs에서 데이터를 불러옵니다.
        /// </summary>
        /// <returns>로드된 UserData, 데이터가 없으면 null</returns>
        private UserData LoadFromLocal()
        {
            try
            {
                if (!PlayerPrefs.HasKey(LOCAL_BACKUP_KEY))
                {
                    Debug.Log("PlayerDataManager: 로컬 백업 없음");
                    return null;
                }

                string json = PlayerPrefs.GetString(LOCAL_BACKUP_KEY);
                UserData loadedData = UserData.FromJson(json);

                if (PlayerPrefs.HasKey(LOCAL_TIMESTAMP_KEY))
                {
                    string timestamp = PlayerPrefs.GetString(LOCAL_TIMESTAMP_KEY);
                    Debug.Log($"PlayerDataManager: 로컬 백업 로드 완료 (저장 시간: {timestamp})");
                }
                else
                {
                    Debug.Log("PlayerDataManager: 로컬 백업 로드 완료");
                }

                return loadedData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"PlayerDataManager: 로컬 백업 로드 실패: {e.Message}");
                return null;
            }
        }
    }
}
