using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Avocado {
    /// <summary>
    /// Generic event hook for Unity elements
    /// </summary>
    public class AvocadoPointerEventHook: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
        private Action<PointerEventData> _onPointerEnter;
        private Action<PointerEventData> _onPointerExit;
        private Action<PointerEventData> _onPointerDown;
        private Action<PointerEventData> _onPointerUp;
        
        public void BindEvents(Action<PointerEventData> onPointerEnter = null, Action<PointerEventData> onPointerExit = null, Action<PointerEventData> onPointerDown = null,
            Action<PointerEventData> onPointerUp = null) {
            
            if (_onPointerEnter == null || onPointerEnter != null)
                _onPointerEnter = onPointerEnter;
            
            if (_onPointerExit == null || onPointerExit != null)
                _onPointerExit = onPointerExit;
            
            if (_onPointerDown == null || onPointerDown != null)
                _onPointerDown = onPointerDown;
            
            if (_onPointerUp == null || onPointerUp != null)
                _onPointerUp = onPointerUp;
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            _onPointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {
            _onPointerExit?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData) {
            _onPointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            _onPointerUp?.Invoke(eventData);
        }
    }
}