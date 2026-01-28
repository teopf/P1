using UnityEngine;
using Game.Data;

namespace Game.Core
{
    /// <summary>
    /// 유닛 베이스 클래스 (캐릭터 및 적 공통)
    /// </summary>
    public class UnitBase : MonoBehaviour
    {
        [Header("유닛 데이터")]
        [SerializeField] protected CharacterSpecData characterSpec;
        [SerializeField] protected int characterLevel = 1;

        [Header("부대 설정 (플레이어 유닛만)")]
        [SerializeField] protected Vector3 relativePosition = Vector3.zero;

        [Header("현재 상태")]
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float maxHealth;
        [SerializeField] protected float currentCost;
        [SerializeField] protected bool isAlive = true;

        // HP 바
        protected HPBar hpBar;

        protected CharacterController controller;
        protected TroopManager troopManager;
        protected Transform currentTarget; // 개인 목표
        
        // 스킬 쿨타임 관리
        protected float[] skillCooldowns = new float[3];
        
        // 이동 관련
        protected bool isPlayerControlled = false;
        protected Vector3 playerMoveDirection = Vector3.zero;
        protected bool isActionLocked = false;
        protected float actionLockEndTime = 0f;

        // 복원력 설정
        [Header("이동 설정")]
        [SerializeField] protected float returnForceMultiplier = 2f;
        [SerializeField] protected float maxReturnForce = 10f;

        public bool IsAlive => isAlive;
        public Transform CurrentTarget => currentTarget;
        public Vector3 RelativePosition => relativePosition;

        protected virtual void Awake()
        {
            // CharacterController 제거 - 겠침 허용
        }

        protected virtual void Start()
        {
            if (characterSpec != null)
            {
                InitializeStats();
            }

            // HP 바 생성
            CreateHPBar();
        }

        protected virtual void Update()
        {
            if (!isAlive) return;

            UpdateSkillCooldowns();
            UpdateActionLock();
            HandleMovement();
        }

        /// <summary>
        /// 스탯 초기화 (기본 + 성장 * 레벨)
        /// </summary>
        public virtual void InitializeStats()
        {
            if (characterSpec != null)
            {
                maxHealth = characterSpec.baseHealth + characterSpec.growthHealth * characterLevel;
                currentHealth = maxHealth;
                currentCost = 0;
            }
            else
            {
                // 기본값 (characterSpec이 없을 경우)
                maxHealth = 100f;
                currentHealth = maxHealth;
            }

            // HP 바 업데이트
            UpdateHPBar();
        }

        /// <summary>
        /// HP 바 생성
        /// </summary>
        protected virtual void CreateHPBar()
        {
            // 이미 HP 바가 있으면 생성하지 않음
            if (hpBar != null)
            {
                Debug.LogWarning($"[{gameObject.name}] HP 바가 이미 존재합니다!");
                return;
            }

            // 기존에 HPBar GameObject가 있으면 삭제
            Transform existingHPBar = transform.Find("HPBar");
            if (existingHPBar != null)
            {
                Debug.LogWarning($"[{gameObject.name}] 기존 HPBar 삭제");
                Destroy(existingHPBar.gameObject);
            }

            GameObject hpBarObj = new GameObject("HPBar");
            hpBarObj.transform.SetParent(transform);
            hpBarObj.transform.localPosition = Vector3.zero;

            hpBar = hpBarObj.AddComponent<HPBar>();
            hpBar.Initialize(transform, new Vector3(0, 2f, 0));
            UpdateHPBar();
        }

        /// <summary>
        /// HP 바 업데이트
        /// </summary>
        protected void UpdateHPBar()
        {
            if (hpBar == null)
            {
                Debug.LogWarning($"[{gameObject.name}] hpBar가 null입니다!");
                return;
            }
            
            if (maxHealth <= 0)
            {
                Debug.LogWarning($"[{gameObject.name}] maxHealth가 0 이하입니다! maxHealth: {maxHealth}");
                return;
            }

            float healthPercent = currentHealth / maxHealth;
            Debug.Log($"[{gameObject.name}] HP 바 업데이트: currentHealth={currentHealth}, maxHealth={maxHealth}, percent={healthPercent:F2} ({healthPercent:P0})");
            
            hpBar.UpdateHP(healthPercent);
        }

        /// <summary>
        /// 스킬 쿨타임 업데이트
        /// </summary>
        protected void UpdateSkillCooldowns()
        {
            for (int i = 0; i < skillCooldowns.Length; i++)
            {
                if (skillCooldowns[i] > 0)
                {
                    skillCooldowns[i] -= Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 액션 락 업데이트
        /// </summary>
        protected void UpdateActionLock()
        {
            if (isActionLocked && Time.time >= actionLockEndTime)
            {
                isActionLocked = false;
            }
        }

        /// <summary>
        /// 이동 처리
        /// </summary>
        protected virtual void HandleMovement()
        {
            if (isActionLocked) return;

            Vector3 moveDirection = Vector3.zero;

            if (isPlayerControlled && playerMoveDirection != Vector3.zero)
            {
                // WASD 이동 명령
                moveDirection = playerMoveDirection;
            }
            else if (troopManager != null)
            {
                // 자동 이동 로직
                moveDirection = CalculateAutoMoveDirection();
            }

            // 실제 이동 적용 (Transform.Translate 사용 - 겠침 허용)
            if (moveDirection != Vector3.zero)
            {
                float moveSpeed = characterSpec != null ? characterSpec.baseMoveSpeed : 3f;
                transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
            }
        }

        /// <summary>
        /// 자동 이동 방향 계산
        /// </summary>
        protected virtual Vector3 CalculateAutoMoveDirection()
        {
            if (troopManager == null) return Vector3.zero;

            // 기준 위치 (Desired Position) 계산
            Vector3 desiredPosition = troopManager.TroopCenter + relativePosition;
            Vector3 toDesired = desiredPosition - transform.position;
            float distanceToDesired = toDesired.magnitude;

            // 복원력 계산 (거리에 비례)
            Vector3 returnForce = toDesired.normalized * Mathf.Min(distanceToDesired * returnForceMultiplier, maxReturnForce);

            // 타겟이 있다면 타겟 방향으로 이동
            Vector3 targetDirection = Vector3.zero;
            if (currentTarget != null)
            {
                targetDirection = (currentTarget.position - transform.position).normalized;
            }

            // 복원력과 타겟 방향을 조합
            return (returnForce + targetDirection).normalized;
        }

        /// <summary>
        /// 부대 매니저 설정
        /// </summary>
        public void SetTroopManager(TroopManager manager)
        {
            troopManager = manager;
        }

        /// <summary>
        /// 상대 위치 설정
        /// </summary>
        public void SetRelativePosition(Vector3 position)
        {
            relativePosition = position;
        }

        /// <summary>
        /// 플레이어 이동 명령 수신
        /// </summary>
        public void OnPlayerMoveCommand(Vector3 direction)
        {
            isPlayerControlled = true;
            playerMoveDirection = direction;
            
            // 모든 행동 중지
            isActionLocked = false;
            currentTarget = null;
        }

        /// <summary>
        /// 플레이어 이동 명령 중지
        /// </summary>
        public void StopPlayerMoveCommand()
        {
            isPlayerControlled = false;
            playerMoveDirection = Vector3.zero;
        }

        /// <summary>
        /// 스킬 강제 사용 (치트용)
        /// </summary>
        public virtual void ForceUseSkill(int skillIndex)
        {
            if (characterSpec == null) return;

            SkillData skill = GetSkill(skillIndex);
            if (skill != null)
            {
                UseSkill(skill, skillIndex);
            }
        }

        /// <summary>
        /// 스킬 사용
        /// </summary>
        protected virtual void UseSkill(SkillData skill, int skillIndex)
        {
            if (skill == null) return;

            // 액션 락 적용
            isActionLocked = true;
            actionLockEndTime = Time.time + skill.totalActionTime;

            // 쿨타임 적용
            skillCooldowns[skillIndex] = skill.cooldown;

            Debug.Log($"[{gameObject.name}] {skill.skillName} 사용!");
        }

        /// <summary>
        /// 스킬 가져오기
        /// </summary>
        protected SkillData GetSkill(int index)
        {
            if (characterSpec == null) return null;

            return index switch
            {
                0 => characterSpec.skill1,
                1 => characterSpec.skill2,
                2 => characterSpec.skill3,
                _ => null
            };
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (!isAlive) return;

            float defense = characterSpec != null ? characterSpec.baseDefense : 0;
            float finalDamage = Mathf.Max(0, damage - defense);

            currentHealth -= finalDamage;
            UpdateHPBar(); // HP 바 업데이트
            
            Debug.Log($"[{gameObject.name}] {finalDamage} 데미지 받음! (남은 체력: {currentHealth})");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        protected virtual void Die()
        {
            isAlive = false;
            Debug.Log($"[{gameObject.name}] 사망!");
            
            if (troopManager != null)
            {
                troopManager.RemoveMember(this);
            }

            // 즉시 제거 (추후 오브젝트 풀링으로 변경 가능)
            Destroy(gameObject);
        }
    }
}
