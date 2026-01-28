using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 스킬 타입 정의
    /// </summary>
    public enum SkillType
    {
        MeleeAttack,    // 근접 공격
        RangedAttack,   // 원거리 공격
        Buff,           // 버프
        Heal            // 회복
    }

    /// <summary>
    /// 범위 모양 타입
    /// </summary>
    public enum RangeShape
    {
        Sphere,
        Box,
        Cylinder
    }

    /// <summary>
    /// 스킬 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Game Data/Skill")]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("스킬 표기 이름")]
        public string skillName;
        
        [Tooltip("스킬 아이콘")]
        public Sprite skillIcon;
        
        [Tooltip("스킬 타입")]
        public SkillType skillType;

        [Header("애니메이션 및 타이밍")]
        [Tooltip("애니메이션 클립")]
        public AnimationClip animationClip;
        
        [Tooltip("타격 적용 시간 (초)")]
        public float hitTime = 0.5f;
        
        [Tooltip("총 액션 시간 (초)")]
        public float totalActionTime = 1f;

        [Header("스킬 조건")]
        [Tooltip("쿨타임 (초)")]
        public float cooldown = 1f;
        
        [Tooltip("필요 코스트")]
        public int requiredCost = 0;
        
        [Tooltip("사정거리 (미터)")]
        public float range = 2f;

        [Header("범위 설정")]
        [Tooltip("범위 모양")]
        public RangeShape rangeShape = RangeShape.Sphere;
        
        [Tooltip("범위 크기")]
        public Vector3 rangeSize = Vector3.one;

        [Header("투사체 (원거리 전용)")]
        [Tooltip("투사체 프리팹")]
        public GameObject projectilePrefab;
        
        [Tooltip("투사체 속도")]
        public float projectileSpeed = 10f;

        [Header("이펙트")]
        [Tooltip("공격 이펙트")]
        public GameObject attackEffect;
        
        [Tooltip("히트 이펙트")]
        public GameObject hitEffect;

        [Header("데미지 계산")]
        [Tooltip("데미지 계산식 (예: ATK * 1.5)")]
        public string damageFormula = "ATK * 1";
        
        [Tooltip("데미지 배율 (간편 설정)")]
        public float damageMultiplier = 1f;
        
        [Tooltip("기본 추가 데미지")]
        public float bonusDamage = 0f;
    }
}
