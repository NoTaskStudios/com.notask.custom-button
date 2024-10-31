using UnityEngine;
namespace CustomButton.Utils 
{
    public abstract class AnimationPreset : ScriptableObject 
    {
        [Min(0.1f)] public float duration;
        [Min(0.1f)] public float speed;
        public float magnitude;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        protected float curveStart => curve[0].time;
        protected float curveEnd => curve.keys[^1].time;
        protected float curveDuration => curveEnd - curveStart;

        public abstract void StartAnimation(CustomButtonBase button);
        public abstract void StopAnimation(CustomButtonBase button);
    }
    
}