using UnityEngine;
using UnityEngine.UI;
using Backend;
using System.Collections;
using System.Numerics;
using TMPro;
using UI.Core;
using Vector2 = UnityEngine.Vector2;

namespace UI
{
    /// <summary>
    /// HUD에 UserData 정보를 표시하는 컴포넌트
    /// - 골드, 젬, ID, 레벨, 경험치 바
    /// </summary>
    public class HUDUserDataDisplay : UIBase
    {
        // PlayerDataManager 대기 코루틴 참조
        private Coroutine waitForManagerCoroutine;
        private bool isSubscribed = false;

        [Header("통화 표시 (Currency)")]
        [SerializeField] private TextMeshProUGUI goldText;      // Resource_Info_Group/Text_Gold
        [SerializeField] private TextMeshProUGUI gemText;       // Resource_Info_Group/Text_Gem

        [Header("레이아웃 설정 (Resource Layout)")]

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

        protected override void OnEnable()
        {
            base.OnEnable();

            // 레이아웃 설정 (VerticalLayoutGroup, Raycast 등)
            SetupResourceLayout();

            // PlayerDataManager 이벤트 구독
            if (PlayerDataManager.Instance != null)
            {
                SubscribeToEvents();
            }
            else
            {
                // PlayerDataManager가 아직 초기화되지 않은 경우 대기 후 구독
                Debug.Log("[HUDUserDataDisplay] PlayerDataManager 대기 중...");
                waitForManagerCoroutine = StartCoroutine(WaitForPlayerDataManager());
            }
        }

        /// <summary>
        /// PlayerDataManager가 준비될 때까지 대기하는 코루틴
        /// </summary>
        private IEnumerator WaitForPlayerDataManager()
        {
            // PlayerDataManager.Instance가 생성될 때까지 대기
            while (PlayerDataManager.Instance == null)
            {
                yield return null;
            }

            Debug.Log("[HUDUserDataDisplay] PlayerDataManager 감지, 이벤트 구독 시작");
            SubscribeToEvents();
            waitForManagerCoroutine = null;
        }

        /// <summary>
        /// PlayerDataManager 이벤트 구독 및 초기 UI 업데이트
        /// </summary>
        private void SubscribeToEvents()
        {
            if (isSubscribed) return;

            PlayerDataManager.Instance.OnDataChanged += UpdateAllUI;
            PlayerDataManager.Instance.OnGoldChanged += UpdateGold;
            PlayerDataManager.Instance.OnGemChanged += UpdateGem;
            PlayerDataManager.Instance.OnLevelChanged += UpdateLevel;
            PlayerDataManager.Instance.OnExpChanged += UpdateExp;
            isSubscribed = true;

            // 초기 UI 업데이트
            UpdateAllUI(PlayerDataManager.Instance.CurrentData);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // 대기 코루틴 정리
            if (waitForManagerCoroutine != null)
            {
                StopCoroutine(waitForManagerCoroutine);
                waitForManagerCoroutine = null;
            }

            // 이벤트 구독 해제
            if (isSubscribed && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnDataChanged -= UpdateAllUI;
                PlayerDataManager.Instance.OnGoldChanged -= UpdateGold;
                PlayerDataManager.Instance.OnGemChanged -= UpdateGem;
                PlayerDataManager.Instance.OnLevelChanged -= UpdateLevel;
                PlayerDataManager.Instance.OnExpChanged -= UpdateExp;
            }
            isSubscribed = false;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (goldText == null) Debug.LogWarning($"{name}: Gold Text is not assigned.");
            if (gemText == null) Debug.LogWarning($"{name}: Gem Text is not assigned.");
        }
#endif

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
            if (value < 1000) return value.ToString();
            
            // 단순화된 로직, 필요시 별도 유틸리티로 분리 가능
            string[] suffixes = { "K", "M", "B", "T", "aa", "ab", "ac" };
            int suffixIndex = 0;
            double dValue = (double)value;

            while (dValue >= 1000.0 && suffixIndex < suffixes.Length)
            {
                dValue /= 1000.0;
                suffixIndex++;
            }

            // suffixIndex가 배열 범위를 넘어가는 경우 처리 (K, M, B, T 이후는 그냥 지수표기 등 고려)
            if (suffixIndex > 0 && suffixIndex <= suffixes.Length)
            {
                // 인덱스 보정 (Loop 1회 -> K (idx 0), Loop 2회 -> M (idx 1))
                return dValue.ToString("0.#") + suffixes[suffixIndex - 1];
            }
            
            return value.ToString("N0"); // 범위 초과시 전체 출력
            
            /* 기존 로직 유지 (안전성 위해) - 위 로직이 더 일반적이지만 기존 로직 존중
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
            */
        }

        // ============================================
        // 레이아웃 설정 메서드
        // ============================================

        /// <summary>
        /// 재화 텍스트의 부모에 VerticalLayoutGroup을 설정하고 Raycast를 끔
        /// 에디터/런타임 양쪽에서 호출 가능
        /// </summary>
        [ContextMenu("Setup Resource Layout")]
        private void SetupResourceLayout()
        {
            if (goldText == null || gemText == null) return;

            // goldText와 gemText의 공통 부모(Resource_Info_Group)에 레이아웃 설정
            Transform parent = goldText.transform.parent;
            if (parent == null) return;

            // VerticalLayoutGroup 추가 (이미 있으면 가져옴)
            VerticalLayoutGroup vlg = parent.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = parent.gameObject.AddComponent<VerticalLayoutGroup>();

            vlg.spacing = 10f;
            vlg.childAlignment = TextAnchor.MiddleRight;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // ContentSizeFitter 추가
            ContentSizeFitter csf = parent.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = parent.gameObject.AddComponent<ContentSizeFitter>();

            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Raycast Target Off (단순 표시용 텍스트)
            goldText.raycastTarget = false;
            gemText.raycastTarget = false;

            // CanvasGroup 추가하여 하위 요소 레이캐스트 차단 방지
            CanvasGroup cg = parent.GetComponent<CanvasGroup>();
            if (cg == null) cg = parent.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
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
