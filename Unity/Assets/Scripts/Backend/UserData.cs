using System;
using System.Numerics;
using UnityEngine;

namespace Backend
{
    [Serializable]
    public class UserData
    {
        // ============================================
        // SERIALIZED FIELDS (Unity JsonUtility compatible)
        // ============================================

        [SerializeField] private int level;
        [SerializeField] private int exp;
        [SerializeField] private string goldString; // BigInteger stored as string
        [SerializeField] private int gem;

        [SerializeField] private int equippedWeaponId;  // -1 = no weapon equipped
        [SerializeField] private int equippedArmorId;   // -1 = no armor equipped

        // ============================================
        // PUBLIC PROPERTIES (Encapsulation + Validation)
        // ============================================

        public int Level
        {
            get => level;
            private set => level = Mathf.Max(1, value); // Minimum level 1
        }

        public int Exp
        {
            get => exp;
            private set => exp = Mathf.Max(0, value); // Non-negative
        }

        public BigInteger Gold
        {
            get
            {
                if (string.IsNullOrEmpty(goldString))
                    return BigInteger.Zero;

                if (BigInteger.TryParse(goldString, out BigInteger result))
                    return result;

                Debug.LogWarning($"Failed to parse Gold: {goldString}. Returning 0.");
                return BigInteger.Zero;
            }
            private set
            {
                goldString = value.ToString();
            }
        }

        public int Gem
        {
            get => gem;
            private set => gem = Mathf.Max(0, value); // Non-negative
        }

        public int? EquippedWeaponId
        {
            get => equippedWeaponId == -1 ? null : (int?)equippedWeaponId;
            set => equippedWeaponId = value ?? -1;
        }

        public int? EquippedArmorId
        {
            get => equippedArmorId == -1 ? null : (int?)equippedArmorId;
            set => equippedArmorId = value ?? -1;
        }

        // ============================================
        // CONSTRUCTORS
        // ============================================

        public UserData()
        {
            level = 1;
            exp = 0;
            goldString = "0";
            gem = 0;
            equippedWeaponId = -1;
            equippedArmorId = -1;
        }

        // Constructor for migration from GameData
        public UserData(GameData oldData)
        {
            level = oldData.Level;
            exp = oldData.Exp;
            goldString = oldData.Gold.ToString(); // Convert int to BigInteger string
            gem = oldData.Gem;
            equippedWeaponId = -1;
            equippedArmorId = -1;
        }

        // ============================================
        // CURRENCY HELPER METHODS
        // ============================================

        /// <summary>
        /// Add gold to the player. Returns true if successful.
        /// </summary>
        public bool AddGold(BigInteger amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot add negative gold: {amount}");
                return false;
            }

            Gold += amount;
            return true;
        }

        /// <summary>
        /// Spend gold. Returns true if player had enough gold.
        /// </summary>
        public bool SpendGold(BigInteger amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot spend negative gold: {amount}");
                return false;
            }

            if (Gold < amount)
            {
                Debug.LogWarning($"Insufficient gold. Required: {amount}, Available: {Gold}");
                return false;
            }

            Gold -= amount;
            return true;
        }

        /// <summary>
        /// Add gems to the player. Returns true if successful.
        /// </summary>
        public bool AddGem(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot add negative gems: {amount}");
                return false;
            }

            Gem += amount;
            return true;
        }

        /// <summary>
        /// Spend gems. Returns true if player had enough gems.
        /// </summary>
        public bool SpendGem(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot spend negative gems: {amount}");
                return false;
            }

            if (Gem < amount)
            {
                Debug.LogWarning($"Insufficient gems. Required: {amount}, Available: {Gem}");
                return false;
            }

            Gem -= amount;
            return true;
        }

        // ============================================
        // EXPERIENCE & LEVEL HELPER METHODS
        // ============================================

        /// <summary>
        /// Add experience. Returns number of levels gained (0 if no level up).
        /// </summary>
        public int AddExp(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot add negative exp: {amount}");
                return 0;
            }

            Exp += amount;

            // Check for level ups
            int levelsGained = 0;
            while (Exp >= GetExpRequiredForNextLevel())
            {
                Exp -= GetExpRequiredForNextLevel();
                Level++;
                levelsGained++;

                Debug.Log($"Level Up! New level: {Level}");

                // Safety check to prevent infinite loop
                if (levelsGained > 100)
                {
                    Debug.LogError("Level up loop exceeded 100 iterations. Breaking.");
                    break;
                }
            }

            return levelsGained;
        }

        /// <summary>
        /// Set level directly (for admin/cheat purposes). Resets exp to 0.
        /// </summary>
        public void SetLevel(int newLevel)
        {
            Level = newLevel;
            Exp = 0;
            Debug.Log($"Level set to: {Level}");
        }

        /// <summary>
        /// Calculate EXP required to reach the next level.
        /// Formula: 100 * Level^1.5 (standard RPG curve)
        /// </summary>
        public int GetExpRequiredForNextLevel()
        {
            return Mathf.RoundToInt(100 * Mathf.Pow(Level, 1.5f));
        }

        /// <summary>
        /// Get progress percentage to next level (0.0 to 1.0)
        /// </summary>
        public float GetLevelProgress()
        {
            int required = GetExpRequiredForNextLevel();
            if (required <= 0) return 1.0f;
            return Mathf.Clamp01((float)Exp / required);
        }

        // ============================================
        // EQUIPMENT HELPER METHODS
        // ============================================

        /// <summary>
        /// Equip a weapon by ID
        /// </summary>
        public void EquipWeapon(int weaponId)
        {
            if (weaponId < 0)
            {
                Debug.LogWarning($"Invalid weapon ID: {weaponId}");
                return;
            }

            EquippedWeaponId = weaponId;
            Debug.Log($"Equipped weapon: {weaponId}");
        }

        /// <summary>
        /// Unequip current weapon
        /// </summary>
        public void UnequipWeapon()
        {
            EquippedWeaponId = null;
            Debug.Log("Weapon unequipped");
        }

        /// <summary>
        /// Equip armor by ID
        /// </summary>
        public void EquipArmor(int armorId)
        {
            if (armorId < 0)
            {
                Debug.LogWarning($"Invalid armor ID: {armorId}");
                return;
            }

            EquippedArmorId = armorId;
            Debug.Log($"Equipped armor: {armorId}");
        }

        /// <summary>
        /// Unequip current armor
        /// </summary>
        public void UnequipArmor()
        {
            EquippedArmorId = null;
            Debug.Log("Armor unequipped");
        }

        /// <summary>
        /// Check if player has any weapon equipped
        /// </summary>
        public bool HasWeaponEquipped()
        {
            return EquippedWeaponId.HasValue;
        }

        /// <summary>
        /// Check if player has any armor equipped
        /// </summary>
        public bool HasArmorEquipped()
        {
            return EquippedArmorId.HasValue;
        }

        // ============================================
        // SERIALIZATION METHODS
        // ============================================

        /// <summary>
        /// Convert to JSON string using Unity's JsonUtility
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true); // prettyPrint = true
        }

        /// <summary>
        /// Create UserData from JSON string
        /// </summary>
        public static UserData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("Cannot deserialize empty JSON. Returning new UserData.");
                return new UserData();
            }

            try
            {
                return JsonUtility.FromJson<UserData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize UserData: {e.Message}");
                return new UserData();
            }
        }

        // ============================================
        // UTILITY METHODS
        // ============================================

        /// <summary>
        /// Create a deep copy of this UserData
        /// </summary>
        public UserData Clone()
        {
            string json = ToJson();
            return FromJson(json);
        }

        /// <summary>
        /// Debug string representation
        /// </summary>
        public override string ToString()
        {
            return $"UserData[Level:{Level}, Exp:{Exp}/{GetExpRequiredForNextLevel()}, " +
                   $"Gold:{Gold}, Gem:{Gem}, " +
                   $"Weapon:{EquippedWeaponId?.ToString() ?? "None"}, " +
                   $"Armor:{EquippedArmorId?.ToString() ?? "None"}]";
        }
    }
}
