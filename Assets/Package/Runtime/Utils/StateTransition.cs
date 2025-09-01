using CustomButton;
using CustomButton.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomButton
{
    [Serializable]
    public class StateTransition<T> where T : Enum
    {
        public Graphic targetGraphic;

        #region Activators
        public bool colorTintTransition = true;
        public bool spriteSwapTransition;
        public bool animationTransition;
        #endregion

        public float fadeDuration = .1f;

        public List<string> stateNames = new();
        public List<T> stateValues = new();
        public List<GraphicState> states = new();

        private GraphicState currentState;
        private AnimationPreset currentAnimation;

        public StateTransition()
        {
            SetupStates();
        }

        public void SetupStates()
        {
            var values = Enum.GetValues(typeof(T));
            foreach (var state in values)
            {
                stateNames.Add(state.ToString());
                stateValues.Add((T)state);
                states.Add(new());
            }
        }

        public void UpdateState(T state)
        {
            ResetTransitions();
            var stateIndex = stateValues.IndexOf(state);
            if (stateIndex < 0) return;
            currentState = states[stateIndex];
            if (colorTintTransition)
            {
                currentState.ColorTransition(targetGraphic, fadeDuration);
            }
            if (spriteSwapTransition)
                currentState.SpriteTransition(targetGraphic as Image);
            if (animationTransition)
            {
                if (!Application.isPlaying) return;
                currentAnimation?.StopAnimation(targetGraphic);
                currentAnimation = currentState.AnimationTransition(targetGraphic);
            }
        }
        public void ResetTransitions()
        {
            if (!colorTintTransition)
                GraphicState.TransitionToColor(targetGraphic, Color.white, 0, true);
            if (!spriteSwapTransition)
                GraphicState.TransitionToSprite(targetGraphic as Image, null);

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!animationTransition) currentAnimation?.StopAnimation(targetGraphic);
        }
    }
}