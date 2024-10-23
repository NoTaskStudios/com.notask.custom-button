using UnityEngine;
namespace CustomButton.Utils {
    [CreateAssetMenu(fileName = "new Shake Preset", menuName = "Custom Button/Presets/new Shake Animation", order = 0)]
    public class ShakePreset : AnimationPreset {
        private void Awake() {
            animationStyle = AnimationStyle.Shake;
            
            duration = 0.1f;
            speed = 50f;
            magnitude = 5f;
        }
    }
    
}