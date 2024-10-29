using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "new Scale Preset", menuName = "Custom Button/Presets/new Scale Animation", order = 0)]
    public class ScalePreset : CoroutineAnimationPreset
    {
        private void Awake()
        {
            duration = 0.1f;
            speed = 1f;
            magnitude = 1.2f;
        }

        public override void StartAnimation(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalScale = rectTransform.localScale;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.localScale = originalScale;
        }

        protected override IEnumerator AnimationCoroutine(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalScale = rectTransform.localScale;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                var t = elapsedTime / duration;
                rectTransform.localScale = Vector3.Lerp(Vector3.one * magnitude, originalScale, t);

                elapsedTime += Time.deltaTime * speed;
                yield return null;
            }

            rectTransform.localScale = originalScale;
        }
    }
}