using UnityEngine;
using System.Collections;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "new Shake Preset", menuName = "Custom Button/Presets/Shake Animation", order = 0)]
    public class ShakePreset : CoroutineAnimationPreset
    {

        [Min(0.1f)] public float speed;
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

            StopAnimation(button);
        }
    }
}