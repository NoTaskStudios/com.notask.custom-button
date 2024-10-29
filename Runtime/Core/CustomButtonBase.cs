using System;
using System.Collections;
using System.Collections.Generic;
using CustomButton.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomButton
{
    //[ExecuteAlways]
    [RequireComponent(typeof(Image)), ExecuteInEditMode]
    public abstract class CustomButtonBase : MonoBehaviour, ICustomButton
    {
        private RectTransform rectTransform;

        #region Activators

        public bool activeColorTint = true;
        public bool activeSpriteSwap;
        public bool activeAnimation;
        public bool changeChildrenColor;
        public bool childrenColorOpacityOnly;
        public bool applyBlinkHighlighted;
        public bool applyOffsetOnChildren;
        public bool applyInvertColorOnTexts;

        #endregion

        [SerializeField] private bool _interactable = true;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable == value) return;
                _interactable = value;
                UpdateButtonState();
            }
        }

        private bool isPlayingAnimationEvt;
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private SpriteState spriteState;
        [SerializeField] private Color targetColorBlend;
        private Color invertedCurrentColor = Color.white;
        private SelectionState _selectionState;

        public SelectionState selectionState
        {
            get => _selectionState;
            set
            {
                if (_selectionState == value) return;
                _selectionState = value;
                UpdateButtonState();
                onStateChange?.Invoke(_selectionState);
            }
        }

        public SpriteState SpriteState
        {
            get { return spriteState; }
            set { spriteState = value; }
        }

        public ColorBlock blockColors = ColorBlock.defaultColorBlock;

        public ColorBlock BlockColors
        {
            get { return blockColors; }
            set { blockColors = value; }
        }

        public Graphic[] graphics;

        public Graphic TargetGraphic
        {
            get { return targetGraphic ??= GetComponent<Graphic>(); }
            set { targetGraphic = value; }
        }

        #region Plus Events

        public Action<SelectionState> onStateChange;

        #endregion

        #region Coroutines

        private Coroutine colorLerpCoroutine;
        private Coroutine opacityLerpCoroutine;
        private Coroutine animationCoroutine;
        private Coroutine offsetCoroutine;

        #endregion

        #region Animations Presets

        public AnimationPreset animationEventDown;
        public AnimationPreset animationEventUp;
        public AnimationPreset animationEventEnter;
        public AnimationPreset animationEventExit;

        #endregion

        private Vector3 originalPosition;
        private Vector2[] initialPositions = new Vector2[0];
        public Vector2 offsetVectorChildren;
        [Range(0f, 0.4f)] public float durationOffset = 0.1f;

        #region Button Default Events

        private bool isPressed;
        private bool isSelected;
        public Button.ButtonClickedEvent onClick = new();

        #endregion

        #region Built-in
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
            if (targetGraphic == null) targetGraphic = GetComponent<Image>();
        }

        private void OnEnable()
        {
            onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            onClick.RemoveListener(OnClick);
        }

        private void Start()
        {
            GetChildren();
        }

        private void OnDestroy() => onClick.RemoveAllListeners();
        #endregion

        public void ToogleInteractable() => Interactable = !Interactable;

        public virtual void OnClick()
        {

        }

        private void GetChildren() => graphics = GetComponentsInChildren<Graphic>();

        #region Handles Transitions

        #endregion

        public void OnTransformChildrenChanged()
        {
            GetChildren();
            UpdateButtonState();
        }

        #region PointerEvents

        public void OnSubmit(BaseEventData eventData)
        {
            if (!_interactable) return;
            onClick?.Invoke();
        }

        //Goes after PointerUp
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable) return;
            selectionState = SelectionState.Selected;
            isSelected = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
            onClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable) return;
            selectionState = SelectionState.Pressed;
            return;//leaving to check while refactoring
            isPressed = true;
            if (applyOffsetOnChildren && offsetCoroutine == null)
                offsetCoroutine = StartCoroutine(OffsetBalanceDown(offsetVectorChildren, durationOffset));
            UpdateButtonState();
            ExecuteAnimation(animationEventDown);
        }

        //Goes before PointerUp
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactable) return;
            Debug.Log("up");
            selectionState = SelectionState.Normal;
            return;//leaving to check while refactoring
            isPressed = false;
            if (applyOffsetOnChildren)
                offsetCoroutine = StartCoroutine(OffsetBalanceUp(initialPositions, durationOffset));
            UpdateButtonState();
            ExecuteAnimation(animationEventUp);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable || isSelected || selectionState == SelectionState.Pressed) return;
            selectionState = SelectionState.Highlighted;
            return;//leaving to check while refactoring
            if (activeColorTint)
            {
                UpdateColor(blockColors.highlightedColor);
            }

            ExecuteAnimation(animationEventEnter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_interactable || isSelected || selectionState == SelectionState.Pressed) return;
            selectionState = SelectionState.Normal;
            return;//leaving to check while refactoring
            if (applyOffsetOnChildren && initialPositions.Length > 0)
                offsetCoroutine = StartCoroutine(OffsetBalanceUp(initialPositions, durationOffset));
            if (activeColorTint)
            {
                UpdateColor(blockColors.normalColor);
            }

            ExecuteAnimation(animationEventExit);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!_interactable) return;
            isSelected = false;
            selectionState = SelectionState.Normal;
        }
        #endregion

        #region StateTransitions
        protected void UpdateButtonState()
        {
            SelectionState currentState = _interactable ? selectionState : SelectionState.Disabled;
            if (activeColorTint)
                HandleColorTintTransition(currentState);
            if (activeSpriteSwap)
                HandleSpriteSwapTransition(currentState);
            if (activeAnimation)
                HandleAnimationTransition();
        }

        #region ColorTransitions
        private void HandleColorTintTransition(SelectionState state)
        {
            Color color = Color.white;
            switch (state)
            {
                case SelectionState.Normal:
                    color = blockColors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    color = blockColors.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    color = blockColors.pressedColor;
                    break;
                case SelectionState.Selected:
                    color = blockColors.selectedColor;
                    break;
                case SelectionState.Disabled:
                    color = blockColors.disabledColor;
                    break;
            }

            UpdateColor(color);

            if(changeChildrenColor) UpdateChildGraphicsColor(color, childrenColorOpacityOnly);
            InvertColorText(color);
        }

        public void UpdateColor(Color targetColor) => targetGraphic.CrossFadeColor(targetColor, blockColors.fadeDuration, true, true);

        private void UpdateChildGraphicsColor(Color targetColor, bool opacityOnly = false)
        {
            Action<Graphic> crossFade =
                opacityOnly ? (graphic) => graphic.CrossFadeAlpha(targetColor.a, blockColors.fadeDuration, true)
                : (graphic) => graphic.CrossFadeColor(targetColor, blockColors.fadeDuration, true, true);

            for (int i = 0; i < graphics.Length; i++)
                crossFade(graphics[i]);
        }

        public void InvertColorText(Color targetColor)
        {
            // Refactor
            if (!applyInvertColorOnTexts) return;
            var invertedColor = Color.white - targetColor;
            invertedColor.a = targetColor.a;

            UpdateTextsColor(invertedColor);
        }

        private void UpdateTextsColor(Color currentColor)
        {
            var texts = GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
                text.CrossFadeColor(currentColor, blockColors.fadeDuration, true, true);
        }
        #endregion

        #region SpriteTransitions

        private void HandleSpriteSwapTransition(SelectionState state)
        {
            var targetImage = targetGraphic as Image;
            if (targetImage == null) return;

            Sprite sprite = state switch
            {
                SelectionState.Highlighted => spriteState.highlightedSprite,
                SelectionState.Pressed => spriteState.pressedSprite,
                SelectionState.Selected => spriteState.selectedSprite,
                SelectionState.Disabled => spriteState.disabledSprite,
                _ => null
            };

            SetSprite(targetImage, sprite);
        }
        private void SetSprite(Image targetImage, Sprite sprite) =>
            targetImage.overrideSprite = sprite;
        #endregion

        #region AnimationTransitions
        private void HandleAnimationTransition()
        {
            if (!activeAnimation && !isPressed) return;
        }

        private void ExecuteAnimation(AnimationPreset currentAnimation = null)
        {
            if (currentAnimation == null || isPlayingAnimationEvt) return;
            if (activeAnimation && !isPlayingAnimationEvt)
            {
                animationCoroutine = currentAnimation.animationStyle switch
                {
                    AnimationStyle.Shake => StartCoroutine(ShakeAnimation(currentAnimation)),
                    AnimationStyle.Scale => StartCoroutine(ScaleAnimation(currentAnimation)),
                    AnimationStyle.Rotate => StartCoroutine(RotateAnimation(currentAnimation)),
                    _ => animationCoroutine
                };
            }
        }
        #endregion

        #endregion

        #region TODO Refactore

        private IEnumerator OffsetBalanceUp(IReadOnlyList<Vector2> initialPositions, float duration)
        {
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                var t = Mathf.Clamp01(elapsedTime / duration);

                for (var i = 0; i < graphics.Length; i++)
                {
                    var currentGraphic = graphics[i];
                    var currentInitialPosition = initialPositions[i];

                    currentGraphic.rectTransform.anchoredPosition =
                        Vector2.Lerp(currentGraphic.rectTransform.anchoredPosition, currentInitialPosition, t);
                }

                yield return null;
            }

            for (var i = 0; i < graphics.Length; i++)
            {
                graphics[i].rectTransform.anchoredPosition = initialPositions[i];
            }

            offsetCoroutine = null;
        }

        private IEnumerator OffsetBalanceDown(Vector2 offset, float duration)
        {
            initialPositions = new Vector2[graphics.Length];
            for (var i = 0; i < graphics.Length; i++)
            {
                initialPositions[i] = graphics[i].rectTransform.anchoredPosition;
            }

            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                var t = Mathf.Clamp01(elapsedTime / duration);

                for (var i = 0; i < graphics.Length; i++)
                {
                    var currentGraphic = graphics[i];
                    var currentInitialPosition = initialPositions[i];
                    var targetPosition = currentInitialPosition + offset;

                    currentGraphic.rectTransform.anchoredPosition =
                        Vector2.Lerp(currentInitialPosition, targetPosition, t);
                }

                yield return null;
            }

            for (var i = 0; i < graphics.Length; i++)
            {
                graphics[i].rectTransform.anchoredPosition = initialPositions[i] + offset;
            }
        }

        #endregion

        #region Animations IEnumerators

        private IEnumerator ShakeAnimation(AnimationPreset currentAnimation)
        {
            isPlayingAnimationEvt = true;
            var elapsedTime = 0f;
            originalPosition = rectTransform.anchoredPosition;
            while (elapsedTime < currentAnimation.duration)
            {
                var x = originalPosition.x + Mathf.Sin(Time.time * currentAnimation.speed) * currentAnimation.magnitude;
                var y = originalPosition.y + Mathf.Cos(Time.time * currentAnimation.speed) * currentAnimation.magnitude;

                rectTransform.anchoredPosition = new Vector3(x, y, originalPosition.z);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
            animationCoroutine = null;
            isPlayingAnimationEvt = false;
        }

        private IEnumerator ScaleAnimation(AnimationPreset currentAnimation)
        {
            var elapsedTime = 0f;
            var originalScale = rectTransform.localScale;

            while (elapsedTime < currentAnimation.duration)
            {
                var t = elapsedTime / currentAnimation.duration;
                transform.localScale = Vector3.Lerp(Vector3.one * currentAnimation.magnitude, originalScale, t);

                elapsedTime += Time.deltaTime * currentAnimation.speed;
                yield return null;
            }

            animationCoroutine = null;
            rectTransform.localScale = originalScale;
        }

        private IEnumerator RotateAnimation(AnimationPreset currentAnimation)
        {
            var elapsedTime = 0f;
            var originalRotation = rectTransform.rotation;

            while (elapsedTime < currentAnimation.duration)
            {
                var rotationAmount = Mathf.Sin(Time.time * currentAnimation.speed) * currentAnimation.magnitude;
                var rotation = originalRotation * Quaternion.Euler(0f, 0f, rotationAmount);

                rectTransform.rotation = rotation;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            animationCoroutine = null;
            rectTransform.rotation = originalRotation;
        }

        #endregion
    }
    public enum SelectionState
    {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }
}