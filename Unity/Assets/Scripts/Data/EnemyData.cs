using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 적 캐릭터 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Game Data/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("캐릭터 이름")]
        public string enemyName;
        
        [Tooltip("캐릭터 메시 (기본값: Capsule)")]
        public GameObject characterMesh;
        
        [Tooltip("공격 애니메이션")]
        public AnimationClip attackAnimation;

        [Header("기본 스탯")]
        [Tooltip("기본 체력")]
        public float baseHealth = 100f;
        
        [Tooltip("기본 공격력")]
        public float baseAttack = 10f;
        
        [Tooltip("기본 방어력")]
        public float baseDefense = 10f;
        
        [Tooltip("기본 이동속도")]
        public float baseMoveSpeed = 2f;
        
        [Tooltip("기본 명중")]
        public float baseAccuracy = 70f;
        
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
    }
}
