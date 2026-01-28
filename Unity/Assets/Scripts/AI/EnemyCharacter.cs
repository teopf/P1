using UnityEngine;
using Game.Data;

namespace Game.AI
{
    /// <summary>
    /// 적 캐릭터
    /// </summary>
    public class EnemyCharacter : Game.Core.UnitBase
    {
        [Header("적 데이터")]
        [SerializeField] private EnemyData enemyData;

        [Header("전투 설정")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 2f;
        
        private float nextAttackTime = 0f;
        private Transform targetPlayer;

        protected override void Start()
        {
            // 적은 EnemyData 사용
            if (enemyData != null)
            {
                maxHealth = enemyData.baseHealth;
                currentHealth = maxHealth;
                isAlive = true;
                gameObject.tag = "Enemy";
            }

            // HP 바 생성
            CreateHPBar();
        }

        protected override void Update()
        {
            if (!isAlive) return;

            UpdateTarget();
            HandleEnemyAI();
        }

        /// <summary>
        /// 타겟 업데이트 (가장 가까운 플레이어 캐릭터)
        /// </summary>
        private void UpdateTarget()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            float minDistance = float.MaxValue;
            Transform closest = null;

            foreach (var player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= detectionRange && distance < minDistance)
                {
                    minDistance = distance;
                    closest = player.transform;
                }
            }

            targetPlayer = closest;
        }

        /// <summary>
        /// 적 AI 로직
        /// </summary>
        private void HandleEnemyAI()
        {
            if (targetPlayer == null) return;

            float distance = Vector3.Distance(transform.position, targetPlayer.position);

            // 공격 범위 내
            if (distance <= attackRange)
            {
                // 공격
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else
            {
                // 접근
                MoveTowardsTarget();
            }
        }

        /// <summary>
        /// 타겟으로 이동
        /// </summary>
        private void MoveTowardsTarget()
        {
            if (targetPlayer == null) return;

            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            float moveSpeed = enemyData != null ? enemyData.baseMoveSpeed : 2f;
            
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// 공격
        /// </summary>
        private void Attack()
        {
            if (targetPlayer == null) return;

            float attack = enemyData != null ? enemyData.baseAttack : 10f;
            
            var playerUnit = targetPlayer.GetComponent<Game.Core.UnitBase>();
            if (playerUnit != null)
            {
                playerUnit.TakeDamage(attack);
                Debug.Log($"[{gameObject.name}] {targetPlayer.name}을(를) 공격! ({attack} 데미지)");
            }
        }

        /// <summary>
        /// EnemyData 설정
        /// </summary>
        public void SetEnemyData(EnemyData data)
        {
            enemyData = data;
            if (enemyData != null)
            {
                maxHealth = enemyData.baseHealth;
                currentHealth = maxHealth;
                gameObject.name = enemyData.enemyName;
                
                // HP 바 업데이트
                UpdateHPBar();
            }
        }
    }
}
