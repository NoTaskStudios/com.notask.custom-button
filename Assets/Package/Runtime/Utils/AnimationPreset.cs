using UnityEngine;
namespace CustomButton.Utils {
    public class AnimationPreset : ScriptableObject {
        public AnimationStyle animationStyle;

        [Range(0.1f,1f)] public float duration;
        [Range(0.1f,99f)] public float speed;
        [Range(1.1f,50f)] public float magnitude;
    }
    
}