using UnityEngine;
using System.Collections;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "new Rotate Preset", menuName = "Custom Button/Presets/Rotate Animation", order = 0)]
    public class RotatePreset : CoroutineAnimationPreset
    {
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;

        public override void StartAnimation(MonoBehaviour button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalRotation = rectTransform.eulerAngles;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.eulerAngles = originalRotation;
        }

        protected override IEnumerator AnimationCoroutine(MonoBehaviour button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalRotation = rectTransform.eulerAngles;
            var targetRotation = originalRotation + (rotationAxis * magnitude);
            var elapsedTime = 0f;
            float startOffset = curveStart;
            float animationDuration = curveDuration;

            while (elapsedTime < duration || loopAnimation)
            {
                float currentTime = elapsedTime / duration;
                float t = curve.Evaluate((currentTime / animationDuration) + startOffset);
                rectTransform.eulerAngles = originalRotation + (targetRotation - originalRotation) * t;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // StopAnimation(button);
        }
    }
}