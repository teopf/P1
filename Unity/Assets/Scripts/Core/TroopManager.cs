using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core
{
    /// <summary>
    /// 부대 관리 매니저
    /// 부대원들의 위치를 관리하고 부대 중심점(Troop Center)을 계산
    /// </summary>
    public class TroopManager : MonoBehaviour
    {
        [Header("부대 설정")]
        [SerializeField] private int maxTroopSize = 10;
        
        [Header("디버그")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color troopCenterColor = Color.yellow;

        private List<UnitBase> troopMembers = new List<UnitBase>();
        private Vector3 troopCenter;
        private Transform troopTargetEnemy; // 부대 공동 목표

        public Vector3 TroopCenter => troopCenter;
        public Transform TroopTarget => troopTargetEnemy;
        public List<UnitBase> TroopMembers => troopMembers;

        private void Update()
        {
            CalculateTroopCenter();
            UpdateTroopTarget();
        }

        /// <summary>
        /// 부대 중심점 계산 (모든 부대원의 평균 위치)
        /// </summary>
        private void CalculateTroopCenter()
        {
            if (troopMembers.Count == 0)
            {
                troopCenter = transform.position;
                return;
            }

            Vector3 sum = Vector3.zero;
            int activeCount = 0;

            foreach (var member in troopMembers)
            {
                if (member != null && member.IsAlive)
                {
                    sum += member.transform.position;
                    activeCount++;
                }
            }

            troopCenter = activeCount > 0 ? sum / activeCount : transform.position;
        }

        /// <summary>
        /// 부대 타겟 업데이트 (중심점에서 가장 가까운 적)
        /// </summary>
        private void UpdateTroopTarget()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            if (enemies.Length == 0)
            {
                troopTargetEnemy = null;
                return;
            }

            float minDistance = float.MaxValue;
            Transform closestEnemy = null;

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(troopCenter, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }

            troopTargetEnemy = closestEnemy;
        }

        /// <summary>
        /// 부대원 추가
        /// </summary>
        public bool AddMember(UnitBase unit)
        {
            if (troopMembers.Count >= maxTroopSize)
            {
                Debug.LogWarning("[TroopManager] 부대가 가득 찼습니다.");
                return false;
            }

            if (!troopMembers.Contains(unit))
            {
                troopMembers.Add(unit);
                unit.SetTroopManager(this);
                Debug.Log($"[TroopManager] {unit.name} 부대에 추가됨");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 부대원 제거
        /// </summary>
        public void RemoveMember(UnitBase unit)
        {
            if (troopMembers.Contains(unit))
            {
                troopMembers.Remove(unit);
                Debug.Log($"[TroopManager] {unit.name} 부대에서 제거됨");
            }
        }

        /// <summary>
        /// 모든 부대원 제거
        /// </summary>
        public void ClearTroop()
        {
            foreach (var member in troopMembers.ToList())
            {
                if (member != null)
                {
                    Destroy(member.gameObject);
                }
            }
            troopMembers.Clear();
            Debug.Log("[TroopManager] 부대가 초기화되었습니다.");
        }

        /// <summary>
        /// 모든 부대원에게 이동 명령
        /// </summary>
        public void CommandMove(Vector3 direction)
        {
            foreach (var member in troopMembers)
            {
                if (member != null && member.IsAlive)
                {
                    member.OnPlayerMoveCommand(direction);
                }
            }
        }

        /// <summary>
        /// 모든 부대원에게 스킬 강제 사용 명령
        /// </summary>
        public void CommandUseSkill(int skillIndex)
        {
            foreach (var member in troopMembers)
            {
                if (member != null && member.IsAlive)
                {
                    member.ForceUseSkill(skillIndex);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // 부대 중심점 표시
            Gizmos.color = troopCenterColor;
            Gizmos.DrawWireSphere(troopCenter, 0.5f);
            
            // 부대원과 중심점 연결선
            Gizmos.color = Color.green;
            foreach (var member in troopMembers)
            {
                if (member != null)
                {
                    Gizmos.DrawLine(troopCenter, member.transform.position);
                }
            }

            // 부대 타겟 표시
            if (troopTargetEnemy != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(troopCenter, troopTargetEnemy.position);
            }
        }
    }
}
