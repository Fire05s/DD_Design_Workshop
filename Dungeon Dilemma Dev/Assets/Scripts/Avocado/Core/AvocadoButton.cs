using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Avocado {
    public class AvocadoButton {
        public AvocadoButtonState State;
        public readonly AvocadoPointerEventHook Hook;
        public readonly Button RawElement;

        public AvocadoButton(Button rawElement) {
            this.RawElement = rawElement;
            rawElement.onClick.AddListener(InvokePress);
            
            Hook = rawElement.AddComponent<AvocadoPointerEventHook>();
            Hook.BindEvents(InvokeMouseEnter, InvokeMouseExit, InvokeMouseDown, InvokeMouseUp);

            this.State = new AvocadoButtonState() {
                Button = this,
                Disabled = false
            };
        }
        
        public struct AvocadoButtonState {
            public bool Disabled {
                get => this._disabled;
                set {
                    _disabled = value;
                    Button.RawElement.interactable = !value;
                }
            }

            private bool _disabled;

            public AvocadoButton Button;
        }
            
        public struct AvocadoButtonPressEventBody {
            public AvocadoButtonState State;
        }
        
        public struct AvocadoButtonMouseEventBody {
            public AvocadoButtonState State;
            public PointerEventData EventData;
        }
            
        public EventHandler<AvocadoButtonPressEventBody> OnPress = new EventHandler<AvocadoButtonPressEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoButtonPressEventBody> OnPressIncludeDisabled = new EventHandler<AvocadoButtonPressEventBody>((sender, body) => {});
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseEnter = new EventHandler<AvocadoButtonMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseExit = new EventHandler<AvocadoButtonMouseEventBody>((sender, body) => {});
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseEnterIncludeDisabled = new EventHandler<AvocadoButtonMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseExitIncludeDisabled = new EventHandler<AvocadoButtonMouseEventBody>((sender, body) => {});
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseDown = new EventHandler<AvocadoButtonMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseDownIncludeDisabled = new EventHandler<AvocadoButtonMouseEventBody>(((sender, body) => {}));
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseUp = new EventHandler<AvocadoButtonMouseEventBody>((sender, body) => {});
        public EventHandler<AvocadoButtonMouseEventBody> OnMouseUpIncludeDisabled = new EventHandler<AvocadoButtonMouseEventBody>((sender, body) => {});

        public void InvokePress() {
            if (!State.Disabled) 
                OnPress.Invoke(this, new AvocadoButtonPressEventBody() {
                    State = this.State
                });
            
            OnPressIncludeDisabled.Invoke(this, new AvocadoButtonPressEventBody() {
                State = this.State
            });
        }

        public void InvokeMouseEnter(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseEnter.Invoke(this, new AvocadoButtonMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseEnterIncludeDisabled.Invoke(this, new AvocadoButtonMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }

        public void InvokeMouseExit(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseExit.Invoke(this, new AvocadoButtonMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseExitIncludeDisabled.Invoke(this, new AvocadoButtonMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }
        
        public void InvokeMouseDown(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseDown.Invoke(this, new AvocadoButtonMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseDownIncludeDisabled.Invoke(this, new AvocadoButtonMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }
        
        public void InvokeMouseUp(PointerEventData eventData) {
            if (!State.Disabled) 
                OnMouseUp.Invoke(this, new AvocadoButtonMouseEventBody() {
                    State = this.State,
                    EventData = eventData
                });
            
            OnMouseUpIncludeDisabled.Invoke(this, new AvocadoButtonMouseEventBody() {
                State = this.State,
                EventData = eventData
            });
        }
    }
}