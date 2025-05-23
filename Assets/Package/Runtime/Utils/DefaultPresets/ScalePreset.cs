using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "new Scale Preset", menuName = "Custom Button/Presets/Scale Animation", order = 0)]
    public class ScalePreset : CoroutineAnimationPreset
    {
        [SerializeField] private Vector3 scaleDirection = Vector3.one;

        public override void StartAnimation(MonoBehaviour button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalScale = rectTransform.localScale;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.localScale = originalScale;
        }

        protected override IEnumerator AnimationCoroutine(MonoBehaviour button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalScale = rectTransform.localScale;
            var targetScale = originalScale + (scaleDirection * magnitude);
            var elapsedTime = 0f;
            float startOffset = curveStart;
            float animationDuration = curveDuration;

            while (elapsedTime < duration || loopAnimation)
            {
                float currentTime = elapsedTime / duration;
                float t = curve.Evaluate((currentTime / animationDuration) + startOffset);
                rectTransform.localScale = originalScale + (targetScale - originalScale) * t;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            StopAnimation(button);
        }
    }
}