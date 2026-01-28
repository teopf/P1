using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// 투사체 (유도형, 명중 보장)
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Transform target;
        private float speed;
        private float damage;
        private Vector3 lastTargetPosition;
        private bool hasReachedTarget = false;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Transform targetTransform, float projectileSpeed, float projectileDamage)
        {
            target = targetTransform;
            speed = projectileSpeed;
            damage = projectileDamage;
            
            if (target != null)
            {
                lastTargetPosition = target.position;
            }
        }

        private void Update()
        {
            if (hasReachedTarget) return;

            // 타겟이 살아있으면 위치 갱신
            if (target != null)
            {
                lastTargetPosition = target.position;
            }

            // 목표 위치로 이동
            Vector3 direction = (lastTargetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // 목표에 도달했는지 체크
            if (Vector3.Distance(transform.position, lastTargetPosition) < 0.2f)
            {
                OnReachTarget();
            }
        }

        /// <summary>
        /// 목표 도달 시
        /// </summary>
        private void OnReachTarget()
        {
            hasReachedTarget = true;

            // 타겟이 살아있으면 데미지 적용
            if (target != null)
            {
                var unitBase = target.GetComponent<Game.Core.UnitBase>();
                if (unitBase != null)
                {
                    unitBase.TakeDamage(damage);
                }
            }

            // 투사체 제거 (추후 오브젝트 풀링으로 변경)
            Destroy(gameObject);
        }
    }
}
