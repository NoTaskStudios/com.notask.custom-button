using UnityEngine.EventSystems;

namespace CustomButton
{
    public interface ICustomButton : ISubmitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        public void OnClick();
        public void OnPointerClick(PointerEventData eventData);
        public void OnPointerDown(PointerEventData eventData);
        public void OnPointerUp(PointerEventData eventData);
        public void OnPointerEnter(PointerEventData eventData);
        public void OnPointerExit(PointerEventData eventData);
    }
}