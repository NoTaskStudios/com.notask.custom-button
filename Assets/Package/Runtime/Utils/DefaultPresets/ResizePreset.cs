using CustomButton;
using CustomButton.Utils;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ResizePreset", menuName = "Custom Button/Presets/ResizePreset")]
public class ResizePreset : CoroutineAnimationPreset
{
    [SerializeField] private Vector2 resizeDirection = Vector2.one;

    private void Awake()
    {
        duration = 0.1f;
        speed = 1f;
        magnitude = 1.2f;
    }

    public override void StartAnimation(CustomButtonBase button)
    {
        RectTransform rectTransform = (RectTransform)button.transform;
        var originalSize = rectTransform.sizeDelta;
        base.StartAnimation(button);

        stopSequence[button] += () => rectTransform.sizeDelta = originalSize;
    }

    protected override IEnumerator AnimationCoroutine(CustomButtonBase button)
    {
        RectTransform rectTransform = (RectTransform)button.transform;
        var originalSize = rectTransform.sizeDelta;
        var targetSize = originalSize + (resizeDirection * magnitude);
        var elapsedTime = 0f;
        float startOffset = curveStart;
        float animationDuration = curveDuration;

        while (elapsedTime < duration || loopAnimation)
        {
            float currentTime = elapsedTime / duration;
            float t = curve.Evaluate((currentTime / animationDuration) + startOffset);
            rectTransform.sizeDelta = originalSize + (targetSize - originalSize) * t;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StopAnimation(button);
    }
}
