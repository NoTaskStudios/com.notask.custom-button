using UnityEngine;
namespace CustomButton.Utils {
    [CreateAssetMenu(fileName = "new Rotate Preset", menuName = "Custom Button/Presets/new Rotate Animation", order = 0)]
    public class RotatePreset : AnimationPreset {
        private void Awake() {
            animationStyle = AnimationStyle.Rotate;
            duration = 0.2f;
            speed = 50f;
            magnitude = 3f;
        }
    }
    
}