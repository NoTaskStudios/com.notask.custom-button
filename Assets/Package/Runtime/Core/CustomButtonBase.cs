using System;
using System.Collections;
using System.Collections.Generic;
using CustomButton.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CustomButton
{
    //[ExecuteAlways]
    [RequireComponent(typeof(Image)), ExecuteInEditMode]
    public abstract class CustomButtonBase : MonoBehaviour, ICustomButton
    {
        private RectTransform rectTransform;

        #region Activators

        public bool colorTintTransition = true;
        public bool spriteSwapTransition;
        public bool animationTransition;
        public bool changeChildrenColor;
        [FormerlySerializedAs("childrenColorOpacityOnly")] public bool changeChildrenAlpha;
        public bool invertColorOnTexts;

        #endregion

        [SerializeField] private bool _interactable = true;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable == value) return;
                _interactable = value;
                selectionState = _interactable ? SelectionState.Normal : SelectionState.Disabled;
            }
        }

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

        private Coroutine pointerUpBuffer;
        private WaitForSeconds bufferDelay = new(.2f);

        #endregion

        #region Animations Presets

        public AnimationPreset normalAnimation;
        public AnimationPreset highlightedAnimation;
        public AnimationPreset pressedAnimation;
        public AnimationPreset selectedAnimation;
        public AnimationPreset disabledAnimation;

        private AnimationPreset currentAnimation;

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
            currentAnimation?.StopAnimation(this);
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
            if(pointerUpBuffer != null)
            {
                StopCoroutine(pointerUpBuffer);
                pointerUpBuffer = null;
            }
        }

        private void GetChildren() => graphics = GetComponentsInChildren<Graphic>();

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
        }

        //Goes before PointerUp
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactable) return;
            pointerUpBuffer = StartCoroutine(PointerUpBuffer());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable || isSelected || selectionState == SelectionState.Pressed) return;
            selectionState = SelectionState.Highlighted;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_interactable || isSelected || selectionState == SelectionState.Pressed) return;
            selectionState = SelectionState.Normal;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!_interactable) return;
            isSelected = false;
            selectionState = SelectionState.Normal;
        }
        #endregion

        #region Pointer up buffer

        private IEnumerator PointerUpBuffer()
        {
            yield return bufferDelay;
            selectionState = SelectionState.Normal;
            pointerUpBuffer = null;
        }
        #endregion

        #region StateTransitions
        public void UpdateButtonState()
        {
            ResetTransitions();
            SelectionState currentState = _interactable ? selectionState : SelectionState.Disabled;
            if (colorTintTransition)
                HandleColorTintTransition(currentState);
            if (spriteSwapTransition)
                HandleSpriteSwapTransition(currentState);
            if (animationTransition)
                HandleAnimationTransition(currentState);
        }

        private void ResetTransitions()
        {
            targetGraphic?.CrossFadeColor(Color.white, 0, true, true);
            var targetImage = targetGraphic as Image;
            if (targetImage) SetSprite(targetImage, null);
            currentAnimation?.StopAnimation(this);
            var texts = GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
                text.CrossFadeColor(Color.white, blockColors.fadeDuration, true, true);
        }

        #region ColorTransitions
        private void HandleColorTintTransition(SelectionState state)
        {
            Color color = state switch
            {
                SelectionState.Normal => blockColors.normalColor,
                SelectionState.Highlighted => blockColors.highlightedColor,
                SelectionState.Pressed => blockColors.pressedColor,
                SelectionState.Selected => blockColors.selectedColor,
                SelectionState.Disabled => blockColors.disabledColor,
                _ => Color.white
            };

            UpdateColor(color);

            UpdateChildGraphicsColor(color, changeChildrenColor, changeChildrenAlpha);
            InvertColorText(color);
        }

        private void UpdateColor(Color targetColor) => targetGraphic?.CrossFadeColor(targetColor, blockColors.fadeDuration, true, true);

        private void UpdateChildGraphicsColor(Color targetColor,bool changeColor, bool useAlpha = false)
        {
            Action<Graphic> crossFade = null;
            if(changeColor) crossFade += (graphic) => graphic.CrossFadeColor(targetColor, blockColors.fadeDuration, true, false);
            if(useAlpha) crossFade += (graphic) => graphic.CrossFadeAlpha(targetColor.a, blockColors.fadeDuration, true);

            for (int i = 0; i < graphics.Length; i++)
                if(graphics[i] != targetGraphic) crossFade(graphics[i]);
        }

        private void InvertColorText(Color targetColor)
        {
            // Refactor
            if (!invertColorOnTexts) return;
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
        private void HandleAnimationTransition(SelectionState state)
        {
            AnimationPreset animation = state switch
            {
                SelectionState.Normal => normalAnimation,
                SelectionState.Highlighted => highlightedAnimation,
                SelectionState.Pressed => pressedAnimation,
                SelectionState.Selected => selectedAnimation,
                SelectionState.Disabled => disabledAnimation,
                _ => null
            };
            ExecuteAnimation(animation);
        }

        private void ExecuteAnimation(AnimationPreset animation = null)
        {
            currentAnimation?.StopAnimation(this);
            if (animation == null) return;

            currentAnimation = animation;
            animation.StartAnimation(this);
        }
        #endregion

        #endregion

        #region TODO Refactore

        /*private IEnumerator OffsetBalanceUp(IReadOnlyList<Vector2> initialPositions, float duration)
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
        }*/

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