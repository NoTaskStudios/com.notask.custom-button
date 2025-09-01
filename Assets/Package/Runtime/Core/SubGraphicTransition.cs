using UnityEngine;
using UnityEngine.UI;

namespace CustomButton
{
    public class SubGraphicTransition : MonoBehaviour
    {
        [SerializeField] private CustomButtonBase customButton;
        [Space, SerializeField] private StateTransition<SelectionState> Transition;

        private void Reset()
        {
            customButton = GetComponentInParent<CustomButtonBase>();
            Transition = new StateTransition<SelectionState>();
            TryGetComponent(out Transition.targetGraphic);
        }

        private void Awake() => customButton.onStateChange += UpdateStage;

        private void UpdateStage(SelectionState state) => Transition.UpdateState(state);
    }
}