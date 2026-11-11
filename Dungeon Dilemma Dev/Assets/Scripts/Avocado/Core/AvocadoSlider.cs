using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Avocado
{
    public class AvocadoSlider
    {
        public struct AvocadoSliderState {
            public bool Disabled {
                get => this._disabled;
                set {
                    _disabled = value;
                    Slider.RawElement.interactable = !value;
                }
            }

            private bool _disabled;
            public AvocadoSlider Slider;
        }
        public struct AvocadoSliderPressEventBody {
            public AvocadoSliderState State;
        }
        
        public struct AvocadoSliderMouseEventBody {
            public AvocadoSliderState State;
            public PointerEventData EventData;
        }
        public AvocadoSliderState State;
        public readonly Slider RawElement;
        public readonly AvocadoPointerEventHook Hook;

        public AvocadoSlider(Slider el, bool wholeNumsOnly=true)
        {
            RawElement = el;
            RawElement.wholeNumbers = true;
            RawElement.onValueChanged.AddListener(delegate {InvokePress();});

            Hook = RawElement.gameObject.AddComponent<AvocadoPointerEventHook>(); 
            Hook.BindEvents(InvokeMouseEnter, InvokeMouseExit, InvokeMouseDown, InvokeMouseUp);

            State = new AvocadoSliderState() {
                Slider = this,
                Disabled = false
            };
        }
        public EventHandler<AvocadoSliderPressEventBody> OnPress = new EventHandler<AvocadoSliderPressEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoSliderPressEventBody> OnPressIncludeDisabled = new EventHandler<AvocadoSliderPressEventBody>((sender, body) => {});
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseEnter = new EventHandler<AvocadoSliderMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseExit = new EventHandler<AvocadoSliderMouseEventBody>((sender, body) => {});
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseEnterIncludeDisabled = new EventHandler<AvocadoSliderMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseExitIncludeDisabled = new EventHandler<AvocadoSliderMouseEventBody>((sender, body) => {});
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseDown = new EventHandler<AvocadoSliderMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseDownIncludeDisabled = new EventHandler<AvocadoSliderMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseUp = new EventHandler<AvocadoSliderMouseEventBody>((sender, body) => {});
        public EventHandler<AvocadoSliderMouseEventBody> OnMouseUpIncludeDisabled = new EventHandler<AvocadoSliderMouseEventBody>((sender, body) => {});
        public void InvokePress() {
            if (!State.Disabled) 
                OnPress.Invoke(this, new AvocadoSliderPressEventBody() {
                    State = this.State
                });
            
            OnPressIncludeDisabled.Invoke(this, new AvocadoSliderPressEventBody() {
                State = this.State
            });
        }

        public void InvokeMouseEnter(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseEnter.Invoke(this, new AvocadoSliderMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseEnterIncludeDisabled.Invoke(this, new AvocadoSliderMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }

        public void InvokeMouseExit(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseExit.Invoke(this, new AvocadoSliderMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseExitIncludeDisabled.Invoke(this, new AvocadoSliderMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }
        
        public void InvokeMouseDown(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseDown.Invoke(this, new AvocadoSliderMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseDownIncludeDisabled.Invoke(this, new AvocadoSliderMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }
        
        public void InvokeMouseUp(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseUp.Invoke(this, new AvocadoSliderMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseUpIncludeDisabled.Invoke(this, new AvocadoSliderMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }
        public void SetNewValue(int newVal)
        {
            if (newVal > RawElement.maxValue) {RawElement.value = RawElement.maxValue;}
            RawElement.value = newVal;
        }
        public void SetActive(bool active)
        {
            if (RawElement == null) return;

            RawElement.gameObject.SetActive(active);
        }

        public void SetNewMax(float newVal)
        {
            RawElement.maxValue = (int)newVal;
        }
    }
}