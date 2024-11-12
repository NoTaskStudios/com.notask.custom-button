using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "MovePreset", menuName = "Custom Button/Presets/MovePreset")]
    public class MovePreset : CoroutineAnimationPreset
    {
        [SerializeField] bool moveChildren = true;
        [SerializeField] private Vector2 moveDirection = Vector2.one;

        public override void StartAnimation(CustomButtonBase button)
        {
            if (moveChildren) ChildrenAnimation(button);
            else ButtonAnimation(button);
        }

        private void ButtonAnimation(CustomButtonBase button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            var originalPosition = rectTransform.anchoredPosition;
            base.StartAnimation(button);

            stopSequence[button] += () => rectTransform.anchoredPosition = originalPosition;
        }

        private void ChildrenAnimation(CustomButtonBase button)
        {
            List<MoveData> transforms = new();
            for (int i = 0; i < button.transform.childCount; i++)
            {
                RectTransform rectTransform = (RectTransform)button.transform.GetChild(i);
                var originalPosition = rectTransform.anchoredPosition;
                var targetPosition = originalPosition + (moveDirection * magnitude);
                transforms.Add(new(rectTransform, originalPosition, targetPosition));
            }

            base.StartAnimation(button);

            stopSequence[button] += () =>
            {
                for (int i = 0; i < button.transform.childCount; i++)
                {
                    RectTransform rectTransform = transforms[i].rectTransform;
                    rectTransform.anchoredPosition = transforms[i].originalPosition;
                }
            };
        }

        protected override IEnumerator AnimationCoroutine(CustomButtonBase button)
        {
            List<MoveData> transforms = new();
            if (moveChildren)
            {
                for (int i = 0; i < button.transform.childCount; i++)
                {
                    RectTransform rectTransform = (RectTransform)button.transform.GetChild(i);
                    var originalPosition = rectTransform.anchoredPosition;
                    var targetPosition = originalPosition + (moveDirection * magnitude);
                    transforms.Add(new(rectTransform, originalPosition, targetPosition));
                }
            }
            else
            {
                RectTransform rectTransform = (RectTransform)button.transform;
                var originalPosition = rectTransform.anchoredPosition;
                var targetPosition = originalPosition + (moveDirection * magnitude);
                transforms.Add(new(rectTransform, originalPosition, targetPosition));
            }

            var elapsedTime = 0f;
            float startOffset = curveStart;
            float animationDuration = curveDuration;

            while (elapsedTime < duration || loopAnimation)
            {
                float currentTime = elapsedTime / duration;
                float t = curve.Evaluate((currentTime / animationDuration) + startOffset);
                for (int i = 0; i < transforms.Count; i++)
                {
                    MoveData data = transforms[i];
                    data.rectTransform.anchoredPosition = data.originalPosition + (data.targetPosition - data.originalPosition) * t;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            StopAnimation(button);
        }
    }

    public struct MoveData
    {
        public RectTransform rectTransform;
        public Vector2 originalPosition;
        public Vector2 targetPosition;
        public MoveData(RectTransform rectTransform, Vector2 originalPosition, Vector2 targetPosition)
        {
            this.rectTransform = rectTransform;
            this.originalPosition = originalPosition;
            this.targetPosition = targetPosition;
        }
    }
}