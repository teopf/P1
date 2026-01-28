using UnityEngine;
using UnityEditor;
using System.IO;

namespace Game.Data.Editor
{
    /// <summary>
    /// 게임 데이터 샘플 생성 에디터 도구
    /// </summary>
    public class GameDataGenerator
    {
        private const string DATA_PATH = "Assets/GameData";
        private const string SKILL_PATH = DATA_PATH + "/Skills";
        private const string CHAR_SPEC_PATH = DATA_PATH + "/CharacterSpecs";
        private const string CHAR_PATH = DATA_PATH + "/Characters";
        private const string ENEMY_PATH = DATA_PATH + "/Enemies";

        [MenuItem("Tools/Game Data/Generate All Sample Data")]
        public static void GenerateAllSampleData()
        {
            CreateDirectories();
            GenerateSkills();
            GenerateCharacterSpecs();
            GenerateCharacters();
            GenerateEnemies();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[GameDataGenerator] 모든 샘플 데이터 생성 완료!");
        }

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder(DATA_PATH))
                AssetDatabase.CreateFolder("Assets", "GameData");
            if (!AssetDatabase.IsValidFolder(SKILL_PATH))
                AssetDatabase.CreateFolder(DATA_PATH, "Skills");
            if (!AssetDatabase.IsValidFolder(CHAR_SPEC_PATH))
                AssetDatabase.CreateFolder(DATA_PATH, "CharacterSpecs");
            if (!AssetDatabase.IsValidFolder(CHAR_PATH))
                AssetDatabase.CreateFolder(DATA_PATH, "Characters");
            if (!AssetDatabase.IsValidFolder(ENEMY_PATH))
                AssetDatabase.CreateFolder(DATA_PATH, "Enemies");
        }

        #region Skill Generation (9 Skills)
        private static void GenerateSkills()
        {
            // 근접 공격 스킬 3개
            CreateSkill("강타", SkillType.MeleeAttack, 0.4f, 1.0f, 1.5f, 0, 1.5f, "ATK * 1.2", 1.2f, 0);
            CreateSkill("회전 베기", SkillType.MeleeAttack, 0.5f, 1.2f, 3f, 10, 2f, "ATK * 1.5", 1.5f, 0);
            CreateSkill("돌진 공격", SkillType.MeleeAttack, 0.6f, 1.5f, 5f, 15, 3f, "ATK * 2.0", 2.0f, 0);

            // 원거리 공격 스킬 3개
            CreateSkill("화살", SkillType.RangedAttack, 0.3f, 0.8f, 2f, 0, 8f, "ATK * 1.0", 1.0f, 0, 15f);
            CreateSkill("마법 화살", SkillType.RangedAttack, 0.5f, 1.0f, 4f, 20, 10f, "ATK * 1.8", 1.8f, 10, 12f);
            CreateSkill("폭발 화살", SkillType.RangedAttack, 0.7f, 1.3f, 6f, 30, 12f, "ATK * 2.5", 2.5f, 20, 10f);

            // 버프 스킬 2개
            CreateSkill("공격력 증가", SkillType.Buff, 0.5f, 1.0f, 8f, 25, 0, "ATK * 0.3", 0.3f, 0);
            CreateSkill("이동속도 증가", SkillType.Buff, 0.3f, 0.8f, 10f, 20, 0, "ATK * 0", 0f, 0);

            // 회복 스킬 1개
            CreateSkill("치유", SkillType.Heal, 0.5f, 1.2f, 6f, 30, 5f, "ATK * 1.0 + 50", 1.0f, 50);
        }

        private static void CreateSkill(string name, SkillType type, float hitTime, float totalTime, 
            float cooldown, int cost, float range, string formula, float multiplier, float bonus, float projectileSpeed = 0)
        {
            string path = $"{SKILL_PATH}/{name}.asset";
            
            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillName = name;
            skill.skillType = type;
            skill.hitTime = hitTime;
            skill.totalActionTime = totalTime;
            skill.cooldown = cooldown;
            skill.requiredCost = cost;
            skill.range = range;
            skill.rangeShape = RangeShape.Sphere;
            skill.rangeSize = Vector3.one * range;
            skill.damageFormula = formula;
            skill.damageMultiplier = multiplier;
            skill.bonusDamage = bonus;
            skill.projectileSpeed = projectileSpeed;

            AssetDatabase.CreateAsset(skill, path);
            Debug.Log($"[Skill] {name} 생성됨");
        }
        #endregion

        #region Character Spec Generation (15 Specs)
        private static void GenerateCharacterSpecs()
        {
            // 캐릭터 3종 x 5성급 = 15개
            
            // 전사 (Warrior) 5성급
            CreateCharacterSpec("전사_1성", 100, 15, 8, 3f, 10f, LoadSkill("강타"), LoadSkill("공격력 증가"), null);
            CreateCharacterSpec("전사_2성", 150, 22, 12, 3.2f, 12f, LoadSkill("강타"), LoadSkill("회전 베기"), LoadSkill("공격력 증가"));
            CreateCharacterSpec("전사_3성", 220, 32, 18, 3.5f, 14f, LoadSkill("강타"), LoadSkill("회전 베기"), LoadSkill("돌진 공격"));
            CreateCharacterSpec("전사_4성", 310, 45, 26, 3.8f, 16f, LoadSkill("회전 베기"), LoadSkill("돌진 공격"), LoadSkill("공격력 증가"));
            CreateCharacterSpec("전사_5성", 420, 62, 36, 4.0f, 18f, LoadSkill("돌진 공격"), LoadSkill("회전 베기"), LoadSkill("공격력 증가"));

            // 궁수 (Archer) 5성급
            CreateCharacterSpec("궁수_1성", 80, 18, 5, 3.5f, 12f, LoadSkill("화살"), LoadSkill("이동속도 증가"), null);
            CreateCharacterSpec("궁수_2성", 120, 26, 7, 3.7f, 14f, LoadSkill("화살"), LoadSkill("마법 화살"), LoadSkill("이동속도 증가"));
            CreateCharacterSpec("궁수_3성", 170, 37, 10, 4.0f, 16f, LoadSkill("마법 화살"), LoadSkill("폭발 화살"), LoadSkill("화살"));
            CreateCharacterSpec("궁수_4성", 240, 51, 14, 4.3f, 18f, LoadSkill("마법 화살"), LoadSkill("폭발 화살"), LoadSkill("이동속도 증가"));
            CreateCharacterSpec("궁수_5성", 320, 68, 19, 4.5f, 20f, LoadSkill("폭발 화살"), LoadSkill("마법 화살"), LoadSkill("이동속도 증가"));

            // 힐러 (Healer) 5성급
            CreateCharacterSpec("힐러_1성", 90, 12, 6, 2.8f, 10f, LoadSkill("치유"), LoadSkill("공격력 증가"), null);
            CreateCharacterSpec("힐러_2성", 135, 17, 9, 3.0f, 12f, LoadSkill("치유"), LoadSkill("화살"), LoadSkill("공격력 증가"));
            CreateCharacterSpec("힐러_3성", 195, 24, 13, 3.2f, 14f, LoadSkill("치유"), LoadSkill("마법 화살"), LoadSkill("공격력 증가"));
            CreateCharacterSpec("힐러_4성", 275, 33, 18, 3.5f, 16f, LoadSkill("치유"), LoadSkill("마법 화살"), LoadSkill("이동속도 증가"));
            CreateCharacterSpec("힐러_5성", 370, 45, 25, 3.7f, 18f, LoadSkill("치유"), LoadSkill("폭발 화살"), LoadSkill("공격력 증가"));
        }

        private static void CreateCharacterSpec(string name, float hp, float atk, float def, float spd, float detectionRange,
            SkillData skill1, SkillData skill2, SkillData skill3)
        {
            string path = $"{CHAR_SPEC_PATH}/{name}.asset";
            
            CharacterSpecData spec = ScriptableObject.CreateInstance<CharacterSpecData>();
            spec.baseHealth = hp;
            spec.baseAttack = atk;
            spec.baseDefense = def;
            spec.baseMoveSpeed = spd;
            spec.baseAccuracy = 80f;
            spec.baseDodge = 5f;
            spec.baseCritRate = 5f;
            spec.baseCritMultiplier = 1.5f;
            spec.baseAttackSpeed = 1f;
            spec.baseHealthRegen = 1f;
            
            spec.growthHealth = hp * 0.1f;
            spec.growthAttack = atk * 0.1f;
            spec.growthDefense = def * 0.1f;
            spec.growthAccuracy = 0.5f;
            spec.growthDodge = 0.2f;
            spec.growthCritRate = 0.3f;
            spec.growthCritMultiplier = 0.01f;
            spec.growthAttackSpeed = 0.01f;
            spec.growthHealthRegen = 0.5f;
            
            spec.detectionRange = detectionRange;
            spec.skill1 = skill1;
            spec.skill2 = skill2;
            spec.skill3 = skill3;

            AssetDatabase.CreateAsset(spec, path);
            Debug.Log($"[CharacterSpec] {name} 생성됨");
        }
        #endregion

        #region Character Generation (3 Characters)
        private static void GenerateCharacters()
        {
            CreateCharacter("전사", HeroGrade.Epic, 
                LoadCharSpec("전사_1성"), LoadCharSpec("전사_2성"), LoadCharSpec("전사_3성"), 
                LoadCharSpec("전사_4성"), LoadCharSpec("전사_5성"));

            CreateCharacter("궁수", HeroGrade.Rare, 
                LoadCharSpec("궁수_1성"), LoadCharSpec("궁수_2성"), LoadCharSpec("궁수_3성"), 
                LoadCharSpec("궁수_4성"), LoadCharSpec("궁수_5성"));

            CreateCharacter("힐러", HeroGrade.Relic, 
                LoadCharSpec("힐러_1성"), LoadCharSpec("힐러_2성"), LoadCharSpec("힐러_3성"), 
                LoadCharSpec("힐러_4성"), LoadCharSpec("힐러_5성"));
        }

        private static void CreateCharacter(string name, HeroGrade grade, 
            CharacterSpecData s1, CharacterSpecData s2, CharacterSpecData s3, 
            CharacterSpecData s4, CharacterSpecData s5)
        {
            string path = $"{CHAR_PATH}/{name}.asset";
            
            CharacterData character = ScriptableObject.CreateInstance<CharacterData>();
            character.characterName = name;
            character.heroGrade = grade;
            character.star1Spec = s1;
            character.star2Spec = s2;
            character.star3Spec = s3;
            character.star4Spec = s4;
            character.star5Spec = s5;

            AssetDatabase.CreateAsset(character, path);
            Debug.Log($"[Character] {name} 생성됨");
        }
        #endregion

        #region Enemy Generation (5 Enemies)
        private static void GenerateEnemies()
        {
            CreateEnemy("슬라임", 80, 8, 5, 2f);
            CreateEnemy("고블린", 120, 12, 8, 2.5f);
            CreateEnemy("오크", 180, 18, 12, 2.2f);
            CreateEnemy("트롤", 250, 25, 18, 1.8f);
            CreateEnemy("보스 드래곤", 500, 50, 30, 3f);
        }

        private static void CreateEnemy(string name, float hp, float atk, float def, float spd)
        {
            string path = $"{ENEMY_PATH}/{name}.asset";
            
            EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyName = name;
            enemy.baseHealth = hp;
            enemy.baseAttack = atk;
            enemy.baseDefense = def;
            enemy.baseMoveSpeed = spd;
            enemy.baseAccuracy = 70f;
            enemy.baseDodge = 3f;
            enemy.baseCritRate = 5f;
            enemy.baseCritMultiplier = 1.5f;
            enemy.baseAttackSpeed = 1f;
            enemy.baseHealthRegen = 0f;

            AssetDatabase.CreateAsset(enemy, path);
            Debug.Log($"[Enemy] {name} 생성됨");
        }
        #endregion

        #region Helper Methods
        private static SkillData LoadSkill(string name)
        {
            return AssetDatabase.LoadAssetAtPath<SkillData>($"{SKILL_PATH}/{name}.asset");
        }

        private static CharacterSpecData LoadCharSpec(string name)
        {
            return AssetDatabase.LoadAssetAtPath<CharacterSpecData>($"{CHAR_SPEC_PATH}/{name}.asset");
        }
        #endregion
    }
}
