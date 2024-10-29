using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

public static class UIToolkitExtensions
{
    #region GemeralVisualElement
    public static VisualElement GetRoot(this VisualElement visualElement)
    {
        VisualElement root = visualElement;
        while (root.parent != null) root = root.parent;
        return root;
    }

    public static void SetPadding(this VisualElement visualElement, int borderWidth)
    {
        visualElement.style.paddingTop = borderWidth;
        visualElement.style.paddingBottom = borderWidth;
        visualElement.style.paddingLeft = borderWidth;
        visualElement.style.paddingRight = borderWidth;
    }

    public static void SetMargin(this VisualElement visualElement, int borderWidth)
    {
        visualElement.style.marginTop = borderWidth;
        visualElement.style.marginBottom = borderWidth;
        visualElement.style.marginLeft = borderWidth;
        visualElement.style.marginRight = borderWidth;
    }

    public static void SetBorder(this VisualElement visualElement, int borderWidth, int curvature = 0, Color? borderColor = null)
    {
        //size
        visualElement.style.borderTopWidth = borderWidth;
        visualElement.style.borderBottomWidth = borderWidth;
        visualElement.style.borderLeftWidth = borderWidth;
        visualElement.style.borderRightWidth = borderWidth;

        //color
        Color color = borderColor ?? visualElement.style.backgroundColor.value;
        visualElement.style.borderTopColor = color;
        visualElement.style.borderBottomColor = color;
        visualElement.style.borderLeftColor = color;
        visualElement.style.borderRightColor = color;

        //curvature
        visualElement.style.borderTopLeftRadius = curvature;
        visualElement.style.borderTopRightRadius = curvature;
        visualElement.style.borderBottomLeftRadius = curvature;
        visualElement.style.borderBottomRightRadius = curvature;
    }
    #endregion

    public static void EnableMouseDrag(this ScrollView scrollView)
    {
        VisualElement root = scrollView.GetRoot();
        bool dragging = false;
        Vector2 maxMovement = Vector2.zero;
        scrollView.RegisterCallback<PointerDownEvent>((evt) =>
        {
            dragging = true;
            maxMovement = scrollView.contentContainer.contentRect.size - scrollView.contentRect.size;
        });
        root.RegisterCallback<PointerMoveEvent>((evt) =>
        {
            if (!dragging || evt.pointerType != PointerType.mouse) return;
            VisualElement content = scrollView.contentContainer;

            //scroll mode
            Vector3 delta = evt.deltaPosition;
            if (scrollView.mode == ScrollViewMode.Horizontal) delta.y = 0f;
            else if (scrollView.mode == ScrollViewMode.Vertical) delta.x = 0f;

            //clamp drag
            Vector3 position = content.transform.position + delta;
            position.x = Mathf.Clamp(position.x, 0, -maxMovement.x);
            position.y = Mathf.Clamp(position.y, -maxMovement.y, 0);

            content.transform.position = position;
        });
        root.RegisterCallback<PointerUpEvent>((evt) =>
        {
            dragging = false;
        });
    }
    public static void IndicateMoreContent(this ScrollView scrollView, float transparency)
    {
        VisualElement root = scrollView.GetRoot();
        bool dragging = false;
        Vector2 maxMovement = Vector2.zero;
        scrollView.style.borderTopWidth = 10;
        scrollView.style.borderBottomWidth = 10;
        scrollView.style.borderBottomColor = ColorExtensions.TransparentBlack(transparency);
        scrollView.RegisterCallback<PointerDownEvent>((evt) =>
        {
            dragging = true;
            maxMovement = scrollView.contentContainer.contentRect.size - scrollView.contentRect.size;
        });
        root.RegisterCallback<PointerMoveEvent>((evt) =>
        {
            if (!dragging || evt.pointerType != PointerType.mouse) return;

            //scroll mode
            Vector3 delta = evt.deltaPosition;
            if (scrollView.mode == ScrollViewMode.Horizontal) delta.y = 0f;
            else if (scrollView.mode == ScrollViewMode.Vertical) delta.x = 0f;

            if (scrollView.contentContainer.transform.position.y < 0)
            {
                //top shade
                scrollView.style.borderTopColor = ColorExtensions.TransparentBlack(transparency);
            }
            else
            {
                scrollView.style.borderTopColor = ColorExtensions.TransparentBlack(0);
            }

            if (scrollView.contentContainer.transform.position.y > -maxMovement.y)
            {
                //top shade
                scrollView.style.borderBottomColor = ColorExtensions.TransparentBlack(transparency);
            }
            else
            {
                scrollView.style.borderBottomColor = ColorExtensions.TransparentBlack(0);
            }

        });
        root.RegisterCallback<PointerUpEvent>((evt) =>
        {
            dragging = false;
        });
    }
}
