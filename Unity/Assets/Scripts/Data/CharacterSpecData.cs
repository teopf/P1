using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 캐릭터 스펙 데이터 (성급별)
    /// </summary>
    [CreateAssetMenu(fileName = "New CharacterSpec", menuName = "Game Data/Character Spec")]
    public class CharacterSpecData : ScriptableObject
    {
        [Header("비주얼")]
        [Tooltip("캐릭터 아이콘")]
        public Sprite characterIcon;
        
        [Tooltip("캐릭터 메시 (프리팹)")]
        public GameObject characterMesh;

        [Header("스킬")]
        [Tooltip("스킬 1")]
        public SkillData skill1;
        
        [Tooltip("스킬 2")]
        public SkillData skill2;
        
        [Tooltip("스킬 3")]
        public SkillData skill3;

        [Header("기본 스탯")]
        [Tooltip("기본 체력")]
        public float baseHealth = 100f;
        
        [Tooltip("기본 공격력")]
        public float baseAttack = 10f;
        
        [Tooltip("기본 방어력")]
        public float baseDefense = 5f;
        
        [Tooltip("기본 이동속도")]
        public float baseMoveSpeed = 3f;
        
        [Tooltip("기본 명중")]
        public float baseAccuracy = 80f;
        
        [Tooltip("기본 회피")]
        public float baseDodge = 5f;
        
        [Tooltip("기본 치명타 확률 (%)")]
        public float baseCritRate = 5f;
        
        [Tooltip("기본 치명타 배율")]
        public float baseCritMultiplier = 1.5f;
        
        [Tooltip("기본 공격속도")]
        public float baseAttackSpeed = 1f;
        
        [Tooltip("기본 생명회복")]
        public float baseHealthRegen = 0f;
        
        [Tooltip("기본 치명률 저항 (%)")]
        public float baseCritResist = 0f;
        
        [Tooltip("기본 약점 공격률 (%)")]
        public float baseWeaknessRate = 0f;
        
        [Tooltip("기본 감소율 (%)")]
        public float baseDamageReduction = 0f;
        
        [Tooltip("기본 추가 피해 확률 (%)")]
        public float baseBonusDamageRate = 0f;

        [Header("성장 스탯 (레벨당)")]
        [Tooltip("성장 체력")]
        public float growthHealth = 10f;
        
        [Tooltip("성장 공격력")]
        public float growthAttack = 2f;
        
        [Tooltip("성장 방어력")]
        public float growthDefense = 1f;
        
        [Tooltip("성장 명중")]
        public float growthAccuracy = 0.5f;
        
        [Tooltip("성장 회피")]
        public float growthDodge = 0.2f;
        
        [Tooltip("성장 치명타 확률")]
        public float growthCritRate = 0.3f;
        
        [Tooltip("성장 치명타 배율")]
        public float growthCritMultiplier = 0.01f;
        
        [Tooltip("성장 공격속도")]
        public float growthAttackSpeed = 0.01f;
        
        [Tooltip("성장 생명회복")]
        public float growthHealthRegen = 0.5f;

        [Header("전투 설정")]
        [Tooltip("적 인지 거리 (미터)")]
        public float detectionRange = 10f;
    }
}
