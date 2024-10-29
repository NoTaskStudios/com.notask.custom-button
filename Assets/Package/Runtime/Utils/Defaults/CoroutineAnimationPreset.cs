using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CustomButton.Utils
{
    public abstract class CoroutineAnimationPreset : AnimationPreset
    {
        protected Dictionary<CustomButtonBase, Action> stopSequence = new();

        public override void StartAnimation(CustomButtonBase button)
        {
            stopSequence ??= new();
            Coroutine coroutine = button.StartCoroutine(AnimationCoroutine(button));

            stopSequence[button] = () => { button.StopCoroutine(coroutine); };
        }

        protected abstract IEnumerator AnimationCoroutine(CustomButtonBase button);

        public override void StopAnimation(CustomButtonBase button)
        {
            if (stopSequence == null || !stopSequence.ContainsKey(button)) return;
            stopSequence[button]?.Invoke();
            stopSequence.Remove(button);
        }
    }
}