using UnityEngine;
using System.Collections;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "new Rotate Preset", menuName = "Custom Button/Presets/new Rotate Animation", order = 0)]
    public class RotatePreset : CoroutineAnimationPreset
    {
        private void Awake()
        {
            duration = 0.2f;
            speed = 50f;
            magnitude = 3f;
        }

        public override void StartAnimation(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalRotation = rectTransform.rotation;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.rotation = originalRotation;
        }

        protected override IEnumerator AnimationCoroutine(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var elapsedTime = 0f;
            var originalRotation = rectTransform.rotation;

            while (elapsedTime < duration)
            {
                var rotationAmount = Mathf.Sin(Time.time * speed) * magnitude;
                var rotation = originalRotation * Quaternion.Euler(0f, 0f, rotationAmount);

                rectTransform.rotation = rotation;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            rectTransform.rotation = originalRotation;
        }
    }
}