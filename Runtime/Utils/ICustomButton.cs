using UnityEngine.EventSystems;

namespace CustomButton
{
    public interface ICustomButton : ISubmitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
    {
        public void OnClick();
    }
}