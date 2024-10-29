using UnityEngine;
namespace CustomButton.Utils 
{
    public abstract class AnimationPreset : ScriptableObject 
    {
        [Range(0.1f,1f)] public float duration;
        [Range(0.1f,99f)] public float speed;
        [Range(1.1f,50f)] public float magnitude;
        public AnimationCurve curve = AnimationCurve.Constant(0, 1, 1);

        public abstract void StartAnimation(CustomButtonBase button);
        public abstract void StopAnimation(CustomButtonBase button);
    }
    
}