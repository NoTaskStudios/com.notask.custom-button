using UnityEngine;
namespace CustomButton.Utils 
{
    public abstract class AnimationPreset : ScriptableObject 
    {
        [Min(0.01f)] public float duration = 0.01f;
        [Tooltip("Make sure to verify the AnimationCurve loop type")]public bool loopAnimation = false;
        public float magnitude;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        protected float curveStart => curve[0].time;
        protected float curveEnd => curve.keys[^1].time;
        protected float curveDuration => curveEnd - curveStart;

        public abstract void StartAnimation(MonoBehaviour button);
        public abstract void StopAnimation(MonoBehaviour button);
    }
    
}