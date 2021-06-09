using System;
using UnityEngine.EventSystems;

namespace XIHBasic
{
    public class MonoTouch : MonoDotBase
    {
        public Action<PointerEventData> onBeginDrag;
        public void OnBeginDrag(BaseEventData eventData)
        {
            onBeginDrag?.Invoke(eventData as PointerEventData);
        }
        public Action<BaseEventData> onCancel;
        public void OnCancel(BaseEventData eventData)
        {
            onCancel?.Invoke(eventData);
        }
        public Action<BaseEventData> onDeselect;
        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect?.Invoke(eventData);
        }
        public Action<PointerEventData> onDrag;
        public void OnDrag(BaseEventData eventData)
        {
            onDrag?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onDrop;
        public void OnDrop(BaseEventData eventData)
        {
            onDrop?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onEndDrag;
        public void OnEndDrag(BaseEventData eventData)
        {
            onEndDrag?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onInitializePotentialDrag;
        public void OnInitializePotentialDrag(BaseEventData eventData)
        {
            onInitializePotentialDrag?.Invoke(eventData as PointerEventData);
        }
        public Action<AxisEventData> onMove;
        public void OnMove(BaseEventData eventData)
        {
            onMove?.Invoke(eventData as AxisEventData);
        }
        public Action<PointerEventData> onPointerClick;
        public void OnPointerClick(BaseEventData eventData)
        {
            onPointerClick?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onPointerDown;
        public void OnPointerDown(BaseEventData eventData)
        {
            onPointerDown?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onPointerEnter;
        public void OnPointerEnter(BaseEventData eventData)
        {
            onPointerEnter?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onPointerExit;
        public void OnPointerExit(BaseEventData eventData)
        {
            onPointerExit?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onPointerUp;
        public void OnPointerUp(BaseEventData eventData)
        {
            onPointerUp?.Invoke(eventData as PointerEventData);
        }
        public Action<PointerEventData> onScroll;
        public void OnScroll(BaseEventData eventData)
        {
            onScroll?.Invoke(eventData as PointerEventData);
        }
        public Action<BaseEventData> onSelect;
        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke(eventData);
        }
        public Action<BaseEventData> onSubmit;
        public void OnSubmit(BaseEventData eventData)
        {
            onSubmit?.Invoke(eventData);
        }
        public Action<BaseEventData> onUpdateSelected;
        public void OnUpdateSelected(BaseEventData eventData)
        {
            onUpdateSelected?.Invoke(eventData);
        }
    }
}
