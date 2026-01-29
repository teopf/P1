using UnityEngine;
using UnityEngine.UI;
using Backend;
using System.Numerics;
using TMPro;

namespace UI
{
    /// <summary>
    /// HUD에 UserData 정보를 표시하는 컴포넌트
    /// - 골드, 젬, ID, 레벨, 경험치 바
    /// </summary>
    public class HUDUserDataDisplay : MonoBehaviour
    {
        [Header("통화 표시 (Currency)")]
        [SerializeField] private TextMeshProUGUI goldText;      // B17에 할당
        [SerializeField] private TextMeshProUGUI gemText;       // B16에 할당

        [Header("플레이어 정보 (Player Info)")]
        [SerializeField] private TextMeshProUGUI playerIdText;   // ID 표시
        [SerializeField] private TextMeshProUGUI levelText;      // 레벨 표시

        [Header("경험치 바 (Experience Bar)")]
        [SerializeField] private Image expBarFill;               // 경험치 바 (Image의 fillAmount 사용)
        [SerializeField] private TextMeshProUGUI expText;        // 경험치 텍스트 (100/250 형식)

        [Header("포맷 설정")]
        [SerializeField] private string goldFormat = "골드: {0}";
        [SerializeField] private string gemFormat = "젬: {0}";
        [SerializeField] private string playerIdFormat = "ID: {0}";
        [SerializeField] private string levelFormat = "Lv. {0}";
        [SerializeField] private string expFormat = "{0} / {1}";

        [Header("대용량 숫자 표시 (BigInteger)")]
        [SerializeField] private bool useShortFormat = true;     // true: 1.2K, 1.5M 형식

        private void OnEnable()
        {
            // PlayerDataManager 이벤트 구독
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnDataChanged += UpdateAllUI;
                PlayerDataManager.Instance.OnGoldChanged += UpdateGold;
                PlayerDataManager.Instance.OnGemChanged += UpdateGem;
                PlayerDataManager.Instance.OnLevelChanged += UpdateLevel;
                PlayerDataManager.Instance.OnExpChanged += UpdateExp;

                // 초기 UI 업데이트
                UpdateAllUI(PlayerDataManager.Instance.CurrentData);
            }
            else
            {
                Debug.LogWarning("[HUDUserDataDisplay] PlayerDataManager가 없습니다. 씬에 PlayerDataManager가 있는지 확인하세요.");
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnDataChanged -= UpdateAllUI;
                PlayerDataManager.Instance.OnGoldChanged -= UpdateGold;
                PlayerDataManager.Instance.OnGemChanged -= UpdateGem;
                PlayerDataManager.Instance.OnLevelChanged -= UpdateLevel;
                PlayerDataManager.Instance.OnExpChanged -= UpdateExp;
            }
        }

        /// <summary>
        /// 모든 UI 요소를 한 번에 업데이트
        /// </summary>
        private void UpdateAllUI(UserData data)
        {
            if (data == null) return;

            UpdateGold(data.Gold);
            UpdateGem(data.Gem);
            UpdateLevel(data.Level);
            UpdateExp(data.Exp);
            UpdatePlayerId();
        }

        /// <summary>
        /// 골드 표시 업데이트
        /// </summary>
        private void UpdateGold(BigInteger gold)
        {
            if (goldText == null) return;

            string goldStr = useShortFormat ? FormatBigInteger(gold) : gold.ToString("N0");
            goldText.text = string.Format(goldFormat, goldStr);
        }

        /// <summary>
        /// 젬 표시 업데이트
        /// </summary>
        private void UpdateGem(int gem)
        {
            if (gemText == null) return;

            gemText.text = string.Format(gemFormat, gem.ToString("N0"));
        }

        /// <summary>
        /// 레벨 표시 업데이트
        /// </summary>
        private void UpdateLevel(int level)
        {
            if (levelText == null) return;

            levelText.text = string.Format(levelFormat, level);
        }

        /// <summary>
        /// 경험치 표시 업데이트
        /// </summary>
        private void UpdateExp(int exp)
        {
            if (PlayerDataManager.Instance == null) return;

            UserData data = PlayerDataManager.Instance.CurrentData;
            int currentExp = data.Exp;
            int requiredExp = data.GetExpRequiredForNextLevel();

            // 경험치 바 fillAmount 업데이트
            if (expBarFill != null)
            {
                float progress = data.GetLevelProgress();
                expBarFill.fillAmount = progress;
            }

            // 경험치 텍스트 업데이트
            if (expText != null)
            {
                expText.text = string.Format(expFormat, currentExp, requiredExp);
            }
        }

        /// <summary>
        /// 플레이어 ID 표시 업데이트
        /// </summary>
        private void UpdatePlayerId()
        {
            if (playerIdText == null) return;

            // TODO: 실제 플레이어 ID가 UserData에 추가되면 해당 값 사용
            // 현재는 임시로 "Player" 표시
            playerIdText.text = string.Format(playerIdFormat, "Player");
        }

        /// <summary>
        /// BigInteger를 축약 형식으로 변환
        /// 예: 1234 -> 1.2K, 1234567 -> 1.2M
        /// </summary>
        private string FormatBigInteger(BigInteger value)
        {
            if (value < 1000)
            {
                return value.ToString();
            }
            else if (value < 1000000)
            {
                double k = (double)value / 1000.0;
                return k.ToString("0.#") + "K";
            }
            else if (value < 1000000000)
            {
                double m = (double)value / 1000000.0;
                return m.ToString("0.#") + "M";
            }
            else if (value < 1000000000000)
            {
                double b = (double)value / 1000000000.0;
                return b.ToString("0.#") + "B";
            }
            else
            {
                double t = (double)value / 1000000000000.0;
                return t.ToString("0.#") + "T";
            }
        }

        // ============================================
        // 치트/테스트용 메서드 (Inspector에서 호출 가능)
        // ============================================

        [ContextMenu("Test: Add 1000 Gold")]
        private void TestAddGold()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddGold(1000);
            }
        }

        [ContextMenu("Test: Add 100 Gem")]
        private void TestAddGem()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddGem(100);
            }
        }

        [ContextMenu("Test: Add 500 Exp")]
        private void TestAddExp()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddExp(500);
            }
        }

        [ContextMenu("Test: Add Huge Gold (1 Billion)")]
        private void TestAddHugeGold()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddGold(new BigInteger(1000000000));
            }
        }
    }
}
