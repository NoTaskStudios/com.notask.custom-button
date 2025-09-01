using CustomButton.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomButton
{
    [Serializable]
    public class GraphicState
    {
        public Color color = Color.white;
        public Sprite sprite;
        public AnimationPreset animation;

        public void ColorTransition(Graphic targetGraphic, float fadeDuration = .1f, bool useAlpha = true) =>
            TransitionToColor(targetGraphic, color, fadeDuration, useAlpha);

        public void SpriteTransition(Image targetImage) => TransitionToSprite(targetImage, sprite);

        public AnimationPreset AnimationTransition(Graphic targetGraphic)
        {
            animation?.StartAnimation(targetGraphic);
            return animation;
        }

        public static void TransitionToColor(Graphic targetGraphic, Color fadeColor, float fadeDuration = .1f, bool useAlpha = true) =>
            targetGraphic?.CrossFadeColor(fadeColor, fadeDuration, fadeDuration > 0, useAlpha);

        public static void TransitionToSprite(Image targetImage, Sprite sprite)
        {
            if (ReferenceEquals(targetImage, null)) return;
            targetImage.overrideSprite = sprite;
        }
    }
}