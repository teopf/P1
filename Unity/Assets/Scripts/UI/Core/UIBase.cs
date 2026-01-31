using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace UI.Core
{
    /// <summary>
    /// 모든 UI 컴포넌트의 기본 클래스입니다.
    /// Unity UGUI의 UIBehaviour를 상속받아 라이프사이클을 관리합니다.
    /// </summary>
    public abstract class UIBase : UIBehaviour
    {
        private RectTransform _rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            // 초기화 로직
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 활성화 시 로직
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // 비활성화 시 로직
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 파괴 시 로직
        }

        /// <summary>
        /// UI를 표시합니다. 필요한 경우 애니메이션을 수행할 수 있습니다.
        /// </summary>
        public virtual void Show(bool immediate = false)
        {
            gameObject.SetActive(true);
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = 1f;
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// UI를 숨깁니다.
        /// </summary>
        public virtual void Hide(bool immediate = false)
        {
             if (CanvasGroup != null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 특정 트랜스폼 하위의 컴포넌트를 이름으로 찾습니다.
        /// </summary>
        protected T FindChildComponent<T>(string childName) where T : Component
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == childName)
                {
                    return child.GetComponent<T>();
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 트랜스폼 하위의 오브젝트를 이름으로 찾습니다.
        /// </summary>
        protected GameObject FindChildObject(string childName)
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == childName)
                {
                    return child.gameObject;
                }
            }
            return null;
        }
    }
}
