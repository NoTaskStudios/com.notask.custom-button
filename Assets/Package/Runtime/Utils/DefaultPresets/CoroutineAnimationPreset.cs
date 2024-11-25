using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CustomButton.Utils
{
    public abstract class CoroutineAnimationPreset : AnimationPreset
    {
        protected Dictionary<MonoBehaviour, Action> stopSequence = new();

        public override void StartAnimation(MonoBehaviour button)
        {
            stopSequence ??= new();
            Coroutine coroutine = button.StartCoroutine(AnimationCoroutine(button));

            stopSequence[button] = () => { button.StopCoroutine(coroutine); };
        }

        protected abstract IEnumerator AnimationCoroutine(MonoBehaviour button);

        public override void StopAnimation(MonoBehaviour button)
        {
            if (stopSequence == null || !stopSequence.ContainsKey(button)) return;
            stopSequence[button]?.Invoke();
            stopSequence.Remove(button);
        }
    }
}