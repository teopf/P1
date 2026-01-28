using UnityEngine;
using Game.Data;
using System.Linq;

namespace Game.AI
{
    /// <summary>
    /// 플레이어 캐릭터 (전투 AI 포함)
    /// </summary>
    public class PlayerCharacter : Game.Core.UnitBase
    {
        [Header("전투 AI 설정")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float targetUpdateInterval = 0.2f;

        private float nextTargetUpdateTime = 0f;

        protected override void Update()
        {
            base.Update();

            if (!isAlive || isPlayerControlled) return;

            // 타겟 업데이트 (일정 간격)
            if (Time.time >= nextTargetUpdateTime)
            {
                UpdateIndividualTarget();
                nextTargetUpdateTime = Time.time + targetUpdateInterval;
            }

            // 스킬 사용 체크
            TryUseSkills();
        }

        /// <summary>
        /// 개별 타겟 업데이트 (인지 거리 내 가장 가까운 적)
        /// </summary>
        private void UpdateIndividualTarget()
        {
            if (characterSpec == null) return;

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            float minDistance = float.MaxValue;
            Transform closestEnemy = null;

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (distance <= characterSpec.detectionRange && distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }

            currentTarget = closestEnemy;
        }

        /// <summary>
        /// 스킬 사용 시도 (조건 체크)
        /// </summary>
        private void TryUseSkills()
        {
            if (characterSpec == null || isActionLocked) return;

            // 스킬 1, 2, 3 순서로 체크
            for (int i = 0; i < 3; i++)
            {
                SkillData skill = GetSkill(i);
                if (skill == null) continue;

                // 조건 체크
                if (CanUseSkill(skill, i))
                {
                    UseSkill(skill, i);
                    break; // 한 번에 하나의 스킬만 사용
                }
            }
        }

        /// <summary>
        /// 스킬 사용 가능 여부 체크
        /// </summary>
        private bool CanUseSkill(SkillData skill, int skillIndex)
        {
            // 쿨타임 체크
            if (skillCooldowns[skillIndex] > 0) return false;

            // 코스트 체크
            if (currentCost < skill.requiredCost) return false;

            // 타입별 조건 체크
            switch (skill.skillType)
            {
                case SkillType.MeleeAttack:
                case SkillType.RangedAttack:
                case SkillType.Buff:
                    // 사정거리 내 적이 있는지
                    return IsTargetInRange(skill.range);

                case SkillType.Heal:
                    // 사정거리 내 체력이 낮은 아군이 있는지
                    return IsAllyNeedHealInRange(skill.range);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 타겟이 사정거리 내에 있는지
        /// </summary>
        private bool IsTargetInRange(float range)
        {
            if (currentTarget == null) return false;
            return Vector3.Distance(transform.position, currentTarget.position) <= range;
        }

        /// <summary>
        /// 사정거리 내에 치유가 필요한 아군이 있는지
        /// </summary>
        private bool IsAllyNeedHealInRange(float range)
        {
            if (troopManager == null) return false;

            foreach (var ally in troopManager.TroopMembers)
            {
                if (ally == null || ally == this) continue;

                float distance = Vector3.Distance(transform.position, ally.transform.position);
                if (distance <= range)
                {
                    // 체력이 60% 이하인지 체크 (간단히 구현)
                    // 실제로는 ally에서 currentHealth를 public으로 노출하거나 메서드로 확인
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 스킬 사용 오버라이드 (투사체 발사 등 추가)
        /// </summary>
        protected override void UseSkill(SkillData skill, int skillIndex)
        {
            base.UseSkill(skill, skillIndex);

            // 타격 적용 시간 후 효과 발동
            Invoke(nameof(ApplySkillEffect), skill.hitTime);

            void ApplySkillEffect()
            {
                if (skill.skillType == SkillType.MeleeAttack)
                {
                    // 근접 공격: 즉시 데미지 적용
                    ApplyMeleeDamage(skill);
                }
                else if (skill.skillType == SkillType.RangedAttack)
                {
                    // 원거리 공격: 투사체 발사
                    LaunchProjectile(skill);
                }
            }
        }

        /// <summary>
        /// 근접 데미지 적용
        /// </summary>
        private void ApplyMeleeDamage(SkillData skill)
        {
            if (currentTarget == null) return;

            float attack = characterSpec.baseAttack + characterSpec.growthAttack * characterLevel;
            float damage = attack * skill.damageMultiplier + skill.bonusDamage;

            var enemyUnit = currentTarget.GetComponent<Game.Core.UnitBase>();
            if (enemyUnit != null)
            {
                enemyUnit.TakeDamage(damage);
            }
        }

        /// <summary>
        /// 투사체 발사
        /// </summary>
        private void LaunchProjectile(SkillData skill)
        {
            if (currentTarget == null) return;

            // 투사체 생성 (간단히 구현, 추후 오브젝트 풀링 적용)
            GameObject projectileObj = new GameObject($"Projectile_{skill.skillName}");
            projectileObj.transform.position = transform.position + Vector3.up;

            var projectile = projectileObj.AddComponent<Projectile>();
            
            float attack = characterSpec.baseAttack + characterSpec.growthAttack * characterLevel;
            float damage = attack * skill.damageMultiplier + skill.bonusDamage;
            
            projectile.Initialize(currentTarget, skill.projectileSpeed, damage);
        }

        protected override Vector3 CalculateAutoMoveDirection()
        {
            // 부대 타겟 방향 고려
            Vector3 baseDirection = base.CalculateAutoMoveDirection();

            // 스킬 사용 중이면 이동하지 않음
            if (isActionLocked)
            {
                return Vector3.zero;
            }

            // 개인 타겟이 있고 사정거리 밖이면 접근
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);
                
                // 사용 가능한 스킬이 있는지 체크
                bool hasUsableSkill = HasAnyUsableSkill();
                
                if (hasUsableSkill)
                {
                    // 사용 가능한 스킬이 있으면 이동하지 않음 (제자리에서 스킬만 사용)
                    return Vector3.zero;
                }
                else
                {
                    // 사용 가능한 스킬이 없으면 타겟에 접근
                    float minRange = GetMinSkillRange();
                    
                    if (distance > minRange)
                    {
                        // 타겟에 접근
                        Vector3 toTarget = (currentTarget.position - transform.position).normalized;
                        return (baseDirection + toTarget * 2f).normalized;
                    }
                }
            }

            return baseDirection;
        }

        /// <summary>
        /// 사용 가능한 스킬이 하나라도 있는지 체크
        /// </summary>
        private bool HasAnyUsableSkill()
        {
            if (characterSpec == null) return false;

            for (int i = 0; i < 3; i++)
            {
                SkillData skill = GetSkill(i);
                if (skill != null && CanUseSkill(skill, i))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 최소 스킬 사정거리 가져오기
        /// </summary>
        private float GetMinSkillRange()
        {
            float minRange = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                var skill = GetSkill(i);
                if (skill != null && skill.range < minRange)
                {
                    minRange = skill.range;
                }
            }

            return minRange == float.MaxValue ? 2f : minRange;
        }
    }
}
