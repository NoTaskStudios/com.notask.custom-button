using CustomButton;
using CustomButton.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GraphicTransition
{

    public Graphic targetGraphic;
    public Graphic[] childGraphics;
    public TMP_Text[] childTexts;

    #region Activators

    public bool colorTintTransition = true;
    public bool spriteSwapTransition;
    public bool animationTransition;
    public bool changeChildrenColor;
    public bool changeChildrenAlpha;
    public bool invertColorOnTexts;

    #endregion

    #region  ColorBlocks

    public ColorBlock blockColors = ColorBlock.defaultColorBlock;

    public ColorBlock BlockColors
    {
        get { return blockColors; }
        set { blockColors = value; }
    }

    [SerializeField] private Color targetColorBlend;
    private Color invertedCurrentColor = Color.white;

    #endregion

    #region Sprite States

    public SpriteState spriteState;

    #endregion

    #region Animations Presets

    public AnimationPreset normalAnimation;
    public AnimationPreset highlightedAnimation;
    public AnimationPreset pressedAnimation;
    public AnimationPreset selectedAnimation;
    public AnimationPreset disabledAnimation;

    private AnimationPreset currentAnimation;

    #endregion

    public GraphicTransition (Graphic targetGraphic)
    {
        this.targetGraphic = targetGraphic;
        UpdateChildGraphics();
    }

    public void UpdateChildGraphics()
    {
        if (!targetGraphic)
        {
            childGraphics = new Graphic[0];
            childTexts = new TMP_Text[0];
            return;
        }

        Graphic[] childs = targetGraphic.GetComponentsInChildren<Graphic>();
        List<Graphic> children = new(childs.Length);
        for (int i = 0; i < childs.Length; i++) if (childs[i] != targetGraphic) children.Add(childs[i]);
        childGraphics = children.ToArray();

        TMP_Text[] texts = targetGraphic.GetComponentsInChildren<TMP_Text>();
        List<TMP_Text> childrenTexts = new(texts.Length);
        for (int i = 0; i < texts.Length; i++) childrenTexts.Add(texts[i]);
        childTexts = childrenTexts.ToArray();
    }

    #region StateTransitions
    public void UpdateState(SelectionState currentState)
    {
        ResetTransitions();
        if (colorTintTransition)
            HandleColorTintTransition(currentState);
        if (spriteSwapTransition)
            HandleSpriteSwapTransition(currentState);
        if (animationTransition)
            HandleAnimationTransition(currentState);
    }

    public void ResetTransitions()
    {
        if (!colorTintTransition)
        {
            UpdateColor(Color.white);
            UpdateChildGraphicsColor(Color.white, true, true);
            UpdateTextsColor(Color.white);

        }
        if (!spriteSwapTransition)
        {
            var targetImage = targetGraphic as Image;
            if (targetImage) SetSprite(targetImage, null);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        if (!animationTransition) currentAnimation?.StopAnimation(targetGraphic);
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

    private void UpdateChildGraphicsColor(Color targetColor, bool changeColor, bool useAlpha = false)
    {
        Action<Graphic> crossFade = null;
        if (changeColor) crossFade += (graphic) => graphic.CrossFadeColor(targetColor, blockColors.fadeDuration, true, false);
        if (useAlpha) crossFade += (graphic) => graphic.CrossFadeAlpha(targetColor.a, blockColors.fadeDuration, true);

        for (int i = 0; i < childGraphics.Length; i++)
            if (childGraphics[i] != targetGraphic) crossFade(childGraphics[i]);
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
        var _texts = childTexts;
        foreach (var text in _texts)
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
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

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
        currentAnimation?.StopAnimation(targetGraphic);
        if (animation == null) return;

        currentAnimation = animation;
        animation.StartAnimation(targetGraphic);
    }
    #endregion

    #endregion
}
