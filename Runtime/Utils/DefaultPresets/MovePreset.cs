using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "MovePreset", menuName = "Custom Button/Presets/MovePreset")]
    public class MovePreset : CoroutineAnimationPreset
    {
        [SerializeField] private Vector2 moveDirection = Vector2.one;

        public override void StartAnimation(MonoBehaviour button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalPosition = rectTransform.anchoredPosition;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.anchoredPosition = originalPosition;
        }

        protected override IEnumerator AnimationCoroutine(MonoBehaviour button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalPosition = rectTransform.anchoredPosition;
            var targetPosition = originalPosition + (moveDirection * magnitude);
            var elapsedTime = 0f;
            float startOffset = curveStart;
            float animationDuration = curveDuration;

            while (elapsedTime < duration || loopAnimation)
            {
                float currentTime = elapsedTime / duration;
                float t = curve.Evaluate((currentTime / animationDuration) + startOffset);
                rectTransform.anchoredPosition = originalPosition + (targetPosition - originalPosition) * t;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            StopAnimation(button);
        }
    }
}