using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomButton
{
    //[ExecuteAlways]
    [RequireComponent(typeof(Image)), ExecuteInEditMode]
    public abstract class CustomButtonBase : MonoBehaviour, ICustomButton
    {
        public RectTransform rectTransform;

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

        public Graphic targetGraphic 
        { 
            get => transition.targetGraphic;
            set
            {
                transition.targetGraphic = value;
                transition.UpdateChildGraphics();
            }
        }
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

        public GraphicTransition transition;

        #region Plus Events

        public Action<SelectionState> onStateChange;

        #endregion

        #region Coroutines

        private Coroutine pointerUpBuffer;
        private WaitForSeconds bufferDelay = new(.2f);

        #endregion

        #region Button Default Events

        private bool isPressed;
        private bool isSelected;
        public Button.ButtonClickedEvent onClick = new();

        #endregion

        #region Built-in
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            onClick.AddListener(OnClick);
            selectionState = SelectionState.Normal;
        }

        private void OnDisable()
        {
            onClick.RemoveListener(OnClick);
            ResetTransitions();
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

        public void OnTransformChildrenChanged()
        {
            ResetTransitions();
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
            SelectionState currentState = _interactable ? selectionState : SelectionState.Disabled;
            transition.UpdateState(currentState);
        }

        private void ResetTransitions()
        {
            transition.ResetTransitions();
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