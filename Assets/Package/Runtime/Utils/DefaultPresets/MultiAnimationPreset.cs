using UnityEngine;

namespace CustomButton.Utils
{
    [CreateAssetMenu(fileName = "MultiAnimationPreset", menuName = "Custom Button/Presets/MultiAnimationPreset")]
    public class MultiAnimationPreset : AnimationPreset
    {
        [Header("Above parameters don't work"),SerializeField, Space(10)] AnimationPreset[] presets;

        public override void StartAnimation(CustomButtonBase button)
        {
            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i] != this) //To prevent stackOverflow
                {
                    presets[i].StartAnimation(button);
                }
            }
        }

        public override void StopAnimation(CustomButtonBase button)
        {
            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i] != this) //To prevent stackOverflow
                {
                    presets[i].StopAnimation(button);
                }
            }
        }
    }
}