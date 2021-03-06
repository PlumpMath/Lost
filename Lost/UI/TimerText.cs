//-----------------------------------------------------------------------
// <copyright file="TimerText.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace PullyTabs
{
    using System;
    using UnityEngine;

    #if USE_TEXTMESH_PRO
    using Text = TMPro.TextMeshProUGUI;
    #else
    using Text = UnityEngine.UI.Text;
    #endif

    [RequireComponent(typeof(Text))]
    public class TimerText : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private string finishedText;
        #pragma warning restore 0649

        private DateTime target;
        private Text text;
        private float timer;
        
        public DateTime Target
        {
            get
            {
                return this.target;
            }

            set
            {
                this.target = value.ToUniversalTime();
                this.UpdateText();
            }
        }

        private void Awake()
        {
            this.text = this.GetComponent<Text>();
        }

        private void OnEnable()
        {
            this.UpdateText();
        }

        private void Update()
        {
            this.timer += Time.unscaledDeltaTime;

            if (this.timer < 1.0f)
            {
                return;
            }

            this.timer = 0.0f;

            this.UpdateText();
        }

        private void UpdateText()
        {
            // update text can be called before Awake is called, so this is very necessary, but this will get called again OnEnable
            if (this.text == null)
            {
                return;
            }

            var utcNow = DateTime.UtcNow;

            // seeing if we're finished
            if (utcNow > this.Target)
            {
                this.text.text = string.IsNullOrEmpty(this.finishedText) == false ? this.finishedText : "00:00:00";
            }
            else
            {
                TimeSpan timeLeft = this.Target.Subtract(utcNow);
                this.text.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
            }
        }
    }
}
