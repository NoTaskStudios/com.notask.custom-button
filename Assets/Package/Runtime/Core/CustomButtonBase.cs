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
    public abstract class CustomButtonBase : MonoBehaviour, ICustomButton, ISubmitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform rectTransform;
#region Activators
        public bool activeColorTint = true;
        public bool activeSpriteSwap;
        public bool activeAnimation;
        public bool applyOpacityOnChildren;
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
        public SpriteState SpriteState
        {
            get { return spriteState; }
            set { spriteState = value; }
        }
        public ColorBlock blockColors = ColorBlock.defaultColorBlock;
        public ColorBlock BlockColors
        {
            get { return blockColors; }
            set
            {
                blockColors = value;
            }
        }
        public Graphic[] graphics;
        public Graphic TargetGraphic
        {
            get { return targetGraphic = GetComponent<Graphic>(); }
            set { targetGraphic = value; }
        }
        #region Plus Events
        public GameObject[] eventsOnInteractable;
        
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
        public Button.ButtonClickedEvent onClick = new ();
#endregion
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
            onClick.RemoveAllListeners();
        }
        private void Start()
        {
            GetChildren();
        }
        private void OnDestroy() => onClick.RemoveAllListeners();
        public void ToogleInteractable() => Interactable = !Interactable;
        public virtual void OnClick()
        {
            if(!_interactable) return;
        }
        private void GetChildren() => graphics = GetComponentsInChildren<Graphic>();
        private void SetSprite(Image targetImage, Sprite sprite) => targetImage.sprite = sprite == null ? normalSprite : sprite;
        protected void UpdateButtonState()
        {
            if (activeColorTint)
                HandleColorTintTransition();
            if (activeSpriteSwap)
                HandleSpriteSwapTransition();
            if (activeAnimation)
                HandleAnimationTransition();
        }
#region Handles Transitions
        private void HandleColorTintTransition()
        {
            if (isPressed)
            {
                UpdateColorBlock(blockColors.pressedColor);
                //StartColorTintCoroutine(blockColors.pressedColor);
                return;
            }
            SetColorInteractable();
        }
        private void HandleSpriteSwapTransition()
        {
            var targetImage = targetGraphic as Image;
        
            if (isPressed)
                SetSprite(targetImage, spriteState.pressedSprite);
            else if (_interactable)
                SetSprite(targetImage, spriteState.highlightedSprite);
            else
                SetSprite(targetImage, spriteState.disabledSprite);
        }
        private void HandleAnimationTransition()
        {
            if (!activeAnimation && !isPressed) return;
        }
#endregion
        private void StartColorTintCoroutine(Color targetColor)
        {
            UpdateColorBlock(targetColor);
            if(!applyOpacityOnChildren) return;
            opacityLerpCoroutine = StartCoroutine(
                SmoothOpacityByGraphics(
                    graphics,
                    targetColor,
                    blockColors.fadeDuration));
        }
        public void CheckInverterColorText()
        {
            // Refactor
            if (!applyInvertColorOnTexts) return;
            var colors1 = Color.white - targetGraphic.color;
            var colors2 = Color.white - blockColors.normalColor;
            var result = colors1 + colors2;
            targetColorBlend = Color.white - result;
            targetColorBlend.a = 1;
            UpdateTextColor(Color.white - targetColorBlend);
        }
        private void UpdateTextColor(Color currentColor)
        {
            currentColor.a = 1f;
            var texts = GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
                text.color = currentColor;
        }

        public void UpdateColorBlock(Color targetColor)
        {
            targetGraphic.CrossFadeColor(targetColor, blockColors.fadeDuration,true, true);
            CheckInverterColorText();
        }

        public void OnTransformChildrenChanged()
        {
            GetChildren();
            UpdateButtonState();
        }
        public void OnSubmit(BaseEventData eventData)
        {
            if (!_interactable) return;
            onClick?.Invoke();
        }
        public void OnPointerClick(PointerEventData eventData){
            if (!_interactable) return;
            onClick?.Invoke();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if(!_interactable) return;
            isPressed = true;
            if (applyOffsetOnChildren && offsetCoroutine == null)
                offsetCoroutine = StartCoroutine(OffsetBalanceDown(offsetVectorChildren, durationOffset));
            UpdateButtonState();
            ExecuteAnimation(animationEventDown);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if(!_interactable) return;
            isPressed = false;
            if (applyOffsetOnChildren)
                offsetCoroutine = StartCoroutine(OffsetBalanceUp(initialPositions, durationOffset));
            UpdateButtonState();
            ExecuteAnimation(animationEventUp);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!_interactable) return;
            if (activeColorTint)
            {
                UpdateColorBlock(blockColors.highlightedColor);
            }
            ExecuteAnimation(animationEventEnter);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if(!_interactable) return;
            if (applyOffsetOnChildren && initialPositions.Length > 0)
                offsetCoroutine = StartCoroutine(OffsetBalanceUp(initialPositions, durationOffset));
            if (activeColorTint)
            {
                UpdateColorBlock(blockColors.normalColor);
            }
            ExecuteAnimation(animationEventExit);
        }
        private void ExecuteAnimation(AnimationPreset currentAnimation = null)
        {
            if (currentAnimation == null || isPlayingAnimationEvt) return;
            if(activeAnimation && !isPlayingAnimationEvt)
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
        private void SetColorInteractable()
        {
            if (applyOpacityOnChildren)
            {
                Color targetColor;
                targetColor = _interactable ? blockColors.normalColor : blockColors.disabledColor;
                opacityLerpCoroutine = StartCoroutine(
                    SmoothOpacityByColor(
                        graphics,
                        targetColor,
                        blockColors.fadeDuration));
            }
            UpdateColorBlock(_interactable ? blockColors.normalColor : blockColors.disabledColor);
        }
#region Offset Children Animation
        private static IEnumerator SmoothOpacityByGraphics(IReadOnlyList<Graphic> graphics, Color targetColor, float duration)
        {
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);

                for (var i = 1; i < graphics.Count; i++)
                {
                    var currentgraphic = graphics[i];


                    /* Only change alpha */

                    currentgraphic.color = new Color(
                        currentgraphic.color.r,
                        currentgraphic.color.g,
                        currentgraphic.color.b,
                        Mathf.Lerp(currentgraphic.color.a, targetColor.a, t));
                }

                yield return null;
            }
        }
        private IEnumerator SmoothOpacityByColor(IReadOnlyList<Graphic> startColor, Color targetColor, float duration)
        {
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                        
                for (var i = 1; i < graphics.Length; i++)
                {
                    var currentgraphic = graphics[i];

                    /* Only change alpha */

                    currentgraphic.color = new Color(
                        currentgraphic.color.r,
                        currentgraphic.color.g,
                        currentgraphic.color.b,
                        Mathf.Lerp(startColor[i].color.a, targetColor.a, t));
                }
            }
            yield return null;
        }
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

                    currentGraphic.rectTransform.anchoredPosition = Vector2.Lerp(currentGraphic.rectTransform.anchoredPosition, currentInitialPosition, t);
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
                    
                    currentGraphic.rectTransform.anchoredPosition = Vector2.Lerp(currentInitialPosition, targetPosition, t);
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
}