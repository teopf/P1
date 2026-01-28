using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    /// <summary>
    /// 캐릭터 머리 위 HP 바
    /// </summary>
    public class HPBar : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;

        [Header("설정")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
        [SerializeField] private float barWidth = 0.8f;  // 0.8m (작게 조정)
        [SerializeField] private float barHeight = 0.1f; // 0.1m (작게 조정)
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        private Camera mainCamera;
        private Transform targetTransform;
        private bool isInitialized = false; // 중복 생성 방지

        private void Awake()
        {
            if (isInitialized) return; // 이미 초기화됨
            
            mainCamera = Camera.main;
            CreateHPBarUI();
            isInitialized = true;
        }

        private void LateUpdate()
        {
            // 카메라를 향하도록 회전 (Billboard)
            if (mainCamera != null && canvas != null)
            {
                canvas.transform.rotation = mainCamera.transform.rotation;
            }

            // 타겟 위치 추적
            if (targetTransform != null)
            {
                transform.position = targetTransform.position + offset;
            }
        }

        /// <summary>
        /// HP 바 UI 생성
        /// </summary>
        private void CreateHPBarUI()
        {
            if (isInitialized && canvas != null)
            {
                Debug.LogWarning("[HPBar] 이미 초기화되었습니다!");
                return;
            }

            RectTransform canvasRect = null;

            // Canvas 생성 (World Space)
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("HPBarCanvas");
                canvasObj.transform.SetParent(transform);
                canvasObj.transform.localPosition = Vector3.zero;

                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 100;

                // Canvas에 Image가 있으면 제거 (투명하게)
                var canvasImage = canvasObj.GetComponent<Image>();
                if (canvasImage != null)
                {
                    Destroy(canvasImage);
                }

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 10; // 픽셀 밀도 낮춤

                canvasRect = canvas.GetComponent<RectTransform>();
                // 80x10 픽셀 크기 (World Space에서 0.8x0.1 유닛)
                canvasRect.sizeDelta = new Vector2(80, 10);
                canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f); // 스케일 축소!
            }

            // Slider 생성
            GameObject sliderObj = new GameObject("HPSlider");
            sliderObj.transform.SetParent(canvas.transform, false);

            hpSlider = sliderObj.AddComponent<Slider>();
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = 1f;
            hpSlider.interactable = false;
            hpSlider.transition = Selectable.Transition.None;
            hpSlider.direction = Slider.Direction.LeftToRight; // ⬅️ 중요! Direction 설정

            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = Vector2.zero;
            sliderRect.anchorMax = Vector2.one;
            sliderRect.sizeDelta = Vector2.zero;
            sliderRect.anchoredPosition = Vector2.zero;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);

            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = backgroundColor;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);

            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0);      // ⬅️ 왼쪽 아래
            fillAreaRect.anchorMax = new Vector2(1, 1);      // ⬅️ 오른쪽 위
            fillAreaRect.pivot = new Vector2(0, 0.5f);       // ⬅️ 왼쪽 중앙 기준
            fillAreaRect.sizeDelta = Vector2.zero;
            fillAreaRect.anchoredPosition = Vector2.zero;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);

            fillImage = fillObj.AddComponent<Image>();
            fillImage.color = fullHealthColor;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);    // ⬅️ 왼쪽 아래
            fillRect.anchorMax = new Vector2(1, 1);    // ⬅️ 오른쪽 위 (값에 따라 조정됨)
            fillRect.pivot = new Vector2(0, 0.5f);     // ⬅️ 왼쪽 중앙 기준
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            // Slider에 fillRect 연결 (중요!)
            hpSlider.fillRect = fillRect;

            if (canvasRect != null)
            {
                Debug.Log($"[HPBar] Slider UI 생성 완료 - Canvas size: {canvasRect.sizeDelta}, Slider value: {hpSlider.value}");
            }
        }

        /// <summary>
        /// HP 바 초기화
        /// </summary>
        public void Initialize(Transform target, Vector3 customOffset = default)
        {
            targetTransform = target;
            
            if (customOffset != default)
            {
                offset = customOffset;
            }

            UpdateHP(1f);
        }

        /// <summary>
        /// HP 업데이트 (0~1 사이 비율)
        /// </summary>
        public void UpdateHP(float healthPercent)
        {
            if (hpSlider == null)
            {
                Debug.LogWarning("[HPBar] hpSlider가 null입니다!");
                return;
            }

            healthPercent = Mathf.Clamp01(healthPercent);
            
            Debug.Log($"[HPBar] UpdateHP 호출 - healthPercent: {healthPercent:F2} ({healthPercent:P0})");
            Debug.Log($"[HPBar] 변경 전 Slider.value: {hpSlider.value}");
            Debug.Log($"[HPBar] Slider.fillRect: {hpSlider.fillRect != null}");
            
            hpSlider.value = healthPercent;
            
            Debug.Log($"[HPBar] 변경 후 Slider.value: {hpSlider.value}");
            Debug.Log($"[HPBar] Slider.direction: {hpSlider.direction}");
            
            if (hpSlider.fillRect != null)
            {
                Debug.Log($"[HPBar] fillRect.anchorMax.x: {hpSlider.fillRect.anchorMax.x}");
            }

            // 체력에 따라 색상 변경
            if (fillImage != null)
            {
                Color newColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
                fillImage.color = newColor;
                Debug.Log($"[HPBar] 색상 변경: {newColor}");
            }
        }

        /// <summary>
        /// HP 바 표시/숨김
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(visible);
            }
        }
    }
}
