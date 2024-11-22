using UnityEngine;
using UnityEngine.UI;

namespace CustomButton
{
    public class SubGraphicTransition : MonoBehaviour
    {
        [SerializeField] private CustomButtonBase customButton;
        [Space,SerializeField] private GraphicTransition transition;

        private void Reset()
        {
            customButton = GetComponentInParent<CustomButtonBase>();
            transition = new GraphicTransition(GetComponent<Graphic>());
        }

        private void Awake() => customButton.onStateChange += UpdateStage;

        private void UpdateStage(SelectionState state) => transition.UpdateState(state);
    }
}