//-----------------------------------------------------------------------
// <copyright file="HDCanvas.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class HDCanvas : MonoBehaviour
    {
        private CanvasScaler canvasScaler;
        private Canvas canvas;

        public new bool enabled
        {
            get
            {
                return base.enabled;
            }

            set
            {
                this.CacheComponents();
                base.enabled = value;
                this.canvas.enabled = value;
                this.canvasScaler.enabled = value;
            }
        }
        
        private void Awake()
        {
            this.Setup();
        }

        private void OnEnable()
        {
            this.Setup();
        }

        private void CacheComponents()
        {
            if (this.canvas == null)
            {
                this.canvas = this.GetComponent<Canvas>();
            }
            
            if (this.canvasScaler == null)
            {
                this.canvasScaler = this.GetComponent<CanvasScaler>();
                this.canvasScaler.hideFlags = HideFlags.HideInInspector;
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
            this.CacheComponents();

            // setting up the canvas
            if (this.canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                this.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

            if (!this.canvas.worldCamera)
            {
                this.canvas.worldCamera = Camera.main;
            }

            // setting up the canvas scaler
            if (this.canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                this.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }

            bool isPortrait = Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown;
            bool isLandscape = Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;

            if (isPortrait)
            {
                if (this.canvasScaler.referenceResolution != new Vector2(1080, 1920))
                {
                    this.canvasScaler.referenceResolution = new Vector2(1080, 1920);
                }
            }
            else if (isLandscape)
            {
                if (this.canvasScaler.referenceResolution != new Vector2(1920, 1080))
                {
                    this.canvasScaler.referenceResolution = new Vector2(1920, 1080);
                }
            }
            else
            {
                Debug.LogErrorFormat(this, "Unknown Screen Orientation Type {0}", Screen.orientation);
            }
            
            if (this.canvasScaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                this.canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            }

            if (this.canvasScaler.matchWidthOrHeight != 1)
            {
                this.canvasScaler.matchWidthOrHeight = 1;
            }
        }
    }
}
