using System;
using System.Numerics;
using UnityEngine;

namespace Backend
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public UserData CurrentData { get; private set; }

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

        // 초기 데이터 로드 등을 위한 더미 메서드 (추후 실제 서버 연동 필요)
        public void InitializeData()
        {
            // TODO: 서버에서 실제 데이터를 로드하는 로직으로 대체 예정
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
    }
}
