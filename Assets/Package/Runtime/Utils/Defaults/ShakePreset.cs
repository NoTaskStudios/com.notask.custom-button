using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "new Shake Preset", menuName = "Custom Button/Presets/new Shake Animation", order = 0)]
    public class ShakePreset : CoroutineAnimationPreset
    {
        private void Awake()
        {
            duration = 0.1f;
            speed = 50f;
            magnitude = 5f;
        }

        public override void StartAnimation(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalPosition = rectTransform.anchoredPosition;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.anchoredPosition = originalPosition;
        }

        protected override IEnumerator AnimationCoroutine(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var elapsedTime = 0f;
            var originalPosition = rectTransform.anchoredPosition;
            while (elapsedTime < duration)
            {
                var x = originalPosition.x + Mathf.Sin(Time.time * speed) * magnitude;
                var y = originalPosition.y + Mathf.Cos(Time.time * speed) * magnitude;

                rectTransform.anchoredPosition = new Vector2(x, y);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
        }
    }
}