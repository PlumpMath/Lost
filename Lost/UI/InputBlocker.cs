//-----------------------------------------------------------------------
// <copyright file="InputBlocker.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    public class InputBlocker : MonoBehaviour, IPointerClickHandler
    {
        private RectTransform rectTransform;
        private UnityEvent onClick;
        private Image image;

        public UnityEvent OnClick
        {
            get { return this.onClick; }
        }
        
        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
            this.image = this.GetComponent<Image>();
            this.image.raycastTarget = true;
            this.image.color = Color.clear;
            this.onClick = new UnityEvent();
            this.Setup();

            // if this input blocker doesn't live on a canvas, then it won't work
            if (this.GetComponentInParent<Canvas>() == null)
            {
                Debug.LogError("InputBlocker doesn't live on a Canvas and will not work!", this);
            }
        }

        #if UNITY_EDITOR
        private void Update()
        {
            this.Setup();
        }
        #endif

        private void Setup()
        {
            if (this.rectTransform.anchorMin != Vector2.zero)
            {
                this.rectTransform.anchorMin = Vector2.zero;
            }

            if (this.rectTransform.anchorMax != Vector2.one)
            {
                this.rectTransform.anchorMax = Vector2.one;
            }

            if (this.rectTransform.sizeDelta != Vector2.zero)
            {
                this.rectTransform.sizeDelta = Vector2.zero;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            this.onClick.Invoke();
        }
    }
}
