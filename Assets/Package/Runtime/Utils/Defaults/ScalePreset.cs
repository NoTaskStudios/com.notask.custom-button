using UnityEngine;
namespace CustomButton.Utils {
    [CreateAssetMenu(fileName = "new Scale Preset", menuName = "Custom Button/Presets/new Scale Animation", order = 0)]
    public class ScalePreset : AnimationPreset {
        private void Awake() {
            animationStyle = AnimationStyle.Scale;
            duration = 0.1f;
            speed = 1f;
            magnitude = 1.2f;
        }
    }
    
}