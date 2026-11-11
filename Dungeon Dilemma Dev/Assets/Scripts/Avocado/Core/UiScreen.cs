using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Avocado {
    /// <summary>
    /// UI Screen parent class, all UI Screen behaviors should inherit from <c>UiScreen</c> as it provides all functionality
    /// for reading, interacting with, and responding to data-driven and input-driven events.
    /// </summary>
    /// <author>Hayden Karp</author>
    public abstract class UiScreen: MonoBehaviour {
        struct ScreenEventRegistryItem {
            public string EventName;
            public Func<Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>> RegisterActions;
        }

        struct AvocadoScreenAnimator {
            public Animator Animator;
            public AvocadoAnimatorEventHook EventHook;
        }
        
        protected enum AvocadoMouseEvent {
            MOUSE_ENTER,
            MOUSE_LEAVE,
            MOUSE_DOWN,
            MOUSE_UP
        }

        private ScreenEventRegistryItem[] EVENTS;
        
        [NonSerialized] public bool Initialized;
        
        private readonly Dictionary<string, Queue<Action>> _queuedElementEvents = new Dictionary<string, Queue<Action>>();
        private readonly Dictionary<int, List<Action>> _refreshIntervalActions = new Dictionary<int, List<Action>>();
        
        private readonly List<Action> _lazyRefreshActions = new List<Action>();
        private DateTime _lastLazyRefresh = DateTime.UtcNow;
        
        private List<TMP_Text> _namedTextElementsRaw = new List<TMP_Text>();
        private List<Button> _namedButtonElementsRaw = new List<Button>();
        private List<Slider> _namedSliderElementsRaw = new List<Slider>();
        private List<Image> _namedImageElementsRaw = new List<Image>();

        private Dictionary<string, AvocadoText> _namedTextFields = new Dictionary<string, AvocadoText>();
        private Dictionary<string, AvocadoButton> _namedButtons = new Dictionary<string, AvocadoButton>();
        private Dictionary<string, AvocadoSlider> _namedSliders = new Dictionary<string, AvocadoSlider>();
        private Dictionary<string, AvocadoImage> _namedImages = new Dictionary<string, AvocadoImage>();
        private Dictionary<string, AvocadoScreenAnimator> _namedAnimators = new Dictionary<string, AvocadoScreenAnimator>();
        private Dictionary<string, GameObject> _namedObjects = new Dictionary<string, GameObject>();
        
        private Tuple<List<Action<EventBridge.EventBridgeEvent>>, List<Action<EventBridge.EventBridgeEvent>>> _eventHooks;

        private bool _collapsed;
        protected string ScreenName;
        protected PlayerID OwnerID;
        
        /// <summary>
        /// Initialization hook for screen, will be called when the screen is opened, if the screen is closed and
        /// then opened, it will be called again as every screen that is opened, is a separate clone.
        /// </summary>
        public virtual void Initialize(PlayerID owner) {
            ScreenName = this.gameObject.name;
            OwnerID = owner;
            RegisterEvents();
            BindElementReferences();
            SetupEventHooks();
            StartCoroutine(RefreshHandler());
        }

        /// <summary>
        /// Close hook for the screen, will be called when the screen is closed.
        /// </summary>
        public virtual void Collapse() {
            _collapsed = true;
            RemoveEventHooks();
        }

        public void PostInitialize() {
            Initialized = true;
        }

        

        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnGoldChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<int>> eventBody) {}
        
        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnResistanceChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>> eventBody) {}

        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnLifestealChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>> eventBody) {}

        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnIsAliveChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<bool>> eventBody) {}
        
        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnCurrentHealthChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>> eventBody) {}
        
        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnMovementSpeedChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>> eventBody) {}
        
        /// <summary>
        /// Event Body for an EventBridge event, override this method if you'd like to use this event, otherwise the
        /// event listener will not be registered.
        /// </summary>
        /// <param name="eventBody">This event's data</param>
        protected virtual void OnMaximumHealthChange(EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>> eventBody) {}


        /// <summary>
        /// Execute arbitrary code when a specific named button is pressed
        /// </summary>
        /// <param name="key">Case-insensitive name of button</param>
        /// <param name="func">The code to be executed when a button is pressed</param>
        /// <param name="includeDisabledStateEvents">Whether this event should fire when the button is disabled</param>
        protected void ListenForButtonPress(string key, Action<AvocadoButton> func, bool includeDisabledStateEvents = false) {
            if (_namedButtons.TryGetValue(key.ToLower(), out AvocadoButton button)) {
                button.OnPress += (sender, body) => {
                    func(button);
                };
                
                if (includeDisabledStateEvents) button.OnPressIncludeDisabled += (sender, body) => {
                    func(button);
                };
            }
        }
        /// <summary>
        /// Execute arbitrary code when a specific named slider is interacted with
        /// </summary>
        /// <param name="key">Case-insensitive name of button</param>
        /// <param name="func">The code to be executed when a button is pressed</param>
        /// <param name="includeDisabledStateEvents">Whether this event should fire when the button is disabled</param>
        protected void ListenForSliderInteract(string key, Action<AvocadoSlider> func, bool includeDisabledStateEvents = false) {
            if (_namedSliders.TryGetValue(key.ToLower(), out AvocadoSlider slider)) {
                slider.OnPress += (sender, body) => {
                    func(slider);
                };
                
                if (includeDisabledStateEvents) slider.OnPressIncludeDisabled += (sender, body) => {
                    func(slider);
                };
            }
        }
        
        /// <summary>
        /// Invoke the animator exited event, do NOT touch this unless you know what you're doing.
        /// </summary>
        /// <param name="objectKey">Case-sensitive name of the object associated with the animator</param>
        public void InvokeAnimatorExited(string objectKey) {
            TriggerLazyRefresh();
            if (_queuedElementEvents.TryGetValue(objectKey, out Queue<Action> els) && els.Count > 0) 
                els.Dequeue()();
        }

        /// <summary>
        /// Listen for a button state event update related to the cursor
        /// </summary>
        /// <param name="key">Unique Case-insensitive key for the button</param>
        /// <param name="mouseEvent">The <c>AvocadoButtonEvent</c> event to listen for</param>
        /// <param name="func">The method to be executed when the event happens</param>
        /// <param name="includeDisabledStateEvents">Whether to trigger this event when the button's <c>State</c> has <c>Disabled</c> set to <c>true</c>.</param>
        protected void ListenForButtonMouseEvent(string key, AvocadoMouseEvent mouseEvent, Action<AvocadoButton.AvocadoButtonMouseEventBody> func,
            bool includeDisabledStateEvents = false) {
            if (_namedButtons.TryGetValue(key.ToLower(), out AvocadoButton button)) {
                switch (mouseEvent) {
                    case AvocadoMouseEvent.MOUSE_ENTER:
                        if (includeDisabledStateEvents)
                            button.OnMouseEnterIncludeDisabled += (sender, body) => func(body);
                        else
                            button.OnMouseEnter += (sender, body) => func(body);
                        
                        break;
                    case AvocadoMouseEvent.MOUSE_LEAVE:
                        if (includeDisabledStateEvents)
                            button.OnMouseExitIncludeDisabled += (sender, body) => func(body);
                        else
                            button.OnMouseExit += (sender, body) => func(body);
                        break;
                    case AvocadoMouseEvent.MOUSE_DOWN:
                        if (includeDisabledStateEvents)
                            button.OnMouseDownIncludeDisabled += (sender, body) => func(body);
                        else
                            button.OnMouseDown += (sender, body) => func(body);
                        break;
                    case AvocadoMouseEvent.MOUSE_UP:
                        if (includeDisabledStateEvents)
                            button.OnMouseUpIncludeDisabled += (sender, body) => func(body);
                        else
                            button.OnMouseUp += (sender, body) => func(body);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Listen on a generic element for a state event update related to the cursor
        /// </summary>
        /// <param name="key">Unique Case-insensitive key for the button</param>
        /// <param name="mouseEvent">The <c>AvocadoButtonEvent</c> event to listen for</param>
        /// <param name="func">The method to be executed when the event happens</param>
        protected void ListenForElementMouseEvent(string key, AvocadoMouseEvent mouseEvent, Action<PointerEventData> func) {
            if (_namedObjects.TryGetValue(key.ToLower(), out GameObject obj)) {
                AvocadoPointerEventHook hook = obj.GetComponent<AvocadoPointerEventHook>();

                if (hook == null)
                    hook = obj.AddComponent<AvocadoPointerEventHook>();
                
                switch (mouseEvent) {
                    case AvocadoMouseEvent.MOUSE_ENTER:
                        hook.BindEvents(func);
                        break;
                    case AvocadoMouseEvent.MOUSE_LEAVE:
                        hook.BindEvents(null, func);
                        break;
                    case AvocadoMouseEvent.MOUSE_DOWN:
                        hook.BindEvents(null, null, func);
                        break;
                    case AvocadoMouseEvent.MOUSE_UP:
                        hook.BindEvents(null, null, null, func);
                        break;
                }
            }
        }

        /// <summary>
        /// Get Button object by key
        /// </summary>
        /// <param name="key">Case-sensitive key for the button (the name of the GameObject in most cases)</param>
        /// <returns>The <c>AvocadoButton</c> associated with the <c>key</c> or null</returns>
        protected AvocadoButton GetButtonRef(string key) {
            return _namedButtons.GetValueOrDefault(key);
        }
        
        /// <summary>
        /// Get Text object by key
        /// </summary>
        /// <param name="key">Case-sensitive key for the text field (the name of the GameObject in most cases)</param>
        /// <returns>The <c>AvocadoText</c> associated with the <c>key</c> or null</returns>
        protected AvocadoText GetTextRef(string key) {
            return _namedTextFields.GetValueOrDefault(key);
        }
        /// <summary>
        /// Get Slider object by key
        /// </summary>
        /// <param name="key">Case-sensitive key for the slider (the name of the GameObject in most cases)</param>
        /// <returns>The <c>AvocadoSlider</c> associated with the <c>key</c> or null</returns>
        protected AvocadoSlider GetSliderRef(string key) {
            return _namedSliders.GetValueOrDefault(key);
        }
        /// Get Image object by key
        /// </summary>
        /// <param name="key">Case-sensitive key for the Image (the name of the GameObject in most cases)</param>
        /// <returns>The <c>AvocadoImage</c> associated with the <c>key</c> or null</returns>
        protected AvocadoImage GetImageRef(string key) {
            return _namedImages.GetValueOrDefault(key);
        }
        
        /// <summary>
        /// Get Element (<c>GameObject</c>) by key
        /// </summary>
        /// <param name="key">Case-insensitive key for the element (the name of the GameObject in all cases)</param>
        /// <returns>The <c>GameObject</c> associated with the <c>key</c> or null</returns>
        protected GameObject GetElementRef(string key) {
            return _namedObjects.GetValueOrDefault(key.ToLower());
        }
        
        /// <summary>
        /// Edit Text field by key and new value
        /// </summary>
        /// <param name="key">The case-sensitive key of the TextField (in many cases the name of the GameObject)</param>
        /// <param name="newText">The new text content of the textbox</param>
        protected void EditText(string key, string newText) {
            if (_namedTextFields.TryGetValue(key, out AvocadoText text)) {
                text.Edit(newText);
            }
        }
        
        /// <summary>
        /// Start UI Animation by name and key
        /// </summary>
        /// <param name="controllerName">Animation controller key (in most cases the name of the GameObject + _animator)</param>
        /// <param name="animName">Name of animation to play, <b>NOTE: The name of the animation and the animation state
        /// should match</b></param>
        /// <param name="afterCompletion">An optional callback to run on the completion of the animation</param>
        /// <example>
        /// <code>
        /// AvocadoButton boringBtn = GetButtonRef("boring_button");
        /// 
        /// TriggerUiAnimation("boring_button_animator", "SpinButton", () => {
        ///     Debug.Log("Animation completed");
        /// });
        /// </code>
        /// </example>
        protected void TriggerUiAnimation(string controllerName, string animName, Action afterCompletion = null) {
            if (_namedAnimators.TryGetValue(controllerName, out AvocadoScreenAnimator screenAnimator)) {
                screenAnimator.EventHook.SetTargetState(animName);
                
                screenAnimator.Animator.SetTrigger(animName);
                string elementName = screenAnimator.Animator.gameObject.name.ToLower();
                
                if (afterCompletion != null) {
                    Queue<Action> queuedEvents = _queuedElementEvents.GetValueOrDefault(elementName, new Queue<Action>());
                    _queuedElementEvents.TryAdd(elementName, queuedEvents);
                    
                    queuedEvents.Enqueue(afterCompletion);
                }
            }
        }
        
        private void BindElementReferences() {
            Button[] buttons = GetComponentsInChildren<Button>();

            foreach (Button b in buttons) {
                if (b.name.StartsWith("[DONT_TRACK]")) continue;
                
                this._namedButtonElementsRaw.Add(b);
                this._namedButtons.Add(b.name.ToLower(), new AvocadoButton(b));
            }

            TMP_Text[] textEls = GetComponentsInChildren<TMP_Text>();

            foreach (TMP_Text t in textEls) {
                if (t.name.StartsWith("[DONT_TRACK]")) continue;
                
                this._namedTextElementsRaw.Add(t);
                this._namedTextFields.Add(t.name.ToLower(), new AvocadoText(t));
            }

            Slider[] sliders = GetComponentsInChildren<Slider>();

            foreach (Slider s in sliders) {
                if (s.name.StartsWith("[DONT_TRACK]")) continue;
                
                this._namedSliderElementsRaw.Add(s);
                this._namedSliders.Add(s.name.ToLower(), new AvocadoSlider(s));
            }

            Image[] images = GetComponentsInChildren<Image>();

            foreach (Image i in images) {
                if (i.name.StartsWith("[DONT_TRACK]")) continue;
                
                this._namedImageElementsRaw.Add(i);
                this._namedImages.Add(i.name.ToLower(), new AvocadoImage(i));
            }


            Animator[] animators = GetComponentsInChildren<Animator>();

            foreach (Animator animator in animators) {
                AvocadoAnimatorEventHook hook = animator.GetBehaviour<AvocadoAnimatorEventHook>();
                
                if (hook == null) UiManager.AvoLog("Animator in Screen (" + gameObject.name + " on " + (animator.gameObject.name) + ") does not have the AvocadoAnimatorEventHook attached, this animator will not be registered and will not be able to be used until it has one.", true);
                else {
                    _namedAnimators.Add(animator.gameObject.name + "_animator", new AvocadoScreenAnimator() {
                        Animator = animator,
                        EventHook = hook
                    });
                    
                    hook.BindAvoScreen(this, animator.gameObject.name.ToLower());
                }
            }

            foreach (Transform t in transform.GetComponentsInChildren<Transform>()) {
                if (!t.gameObject.name.Equals("GameObject") && !_namedObjects.ContainsKey(t.name)) _namedObjects.Add(t.gameObject.name.ToLower(), t.gameObject);
            }
        }

        /// <summary>
        /// Populate the <c>EVENTS</c> property of Screen, this will likely be changed in the future in
        /// search of a more memory-efficient solution 
        /// </summary>
        private void RegisterEvents() {
            EVENTS = new[] {
                new ScreenEventRegistryItem() {
                    EventName = "OnGoldChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnGoldChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<int>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<int>) @event.Data
                            });
                        }));
                    }
                },
                new ScreenEventRegistryItem() {
                    EventName = "OnResistanceChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnResistanceChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<float>) @event.Data
                            });
                        }));
                    }
                },
                new ScreenEventRegistryItem() {
                    EventName = "OnLifestealChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnLifestealChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<float>) @event.Data
                            });
                        }));
                    }
                },
                new ScreenEventRegistryItem() {
                    EventName = "OnIsAliveChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnIsAliveChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<bool>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<bool>) @event.Data
                            });
                        }));
                    }
                },
                new ScreenEventRegistryItem() {
                    EventName = "OnCurrentHealthChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnCurrentHealthChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<float>) @event.Data
                            });
                        }));
                    }
                },
                new ScreenEventRegistryItem() {
                    EventName = "OnMovementSpeedChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnMovementSpeedChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<float>) @event.Data
                            });
                        }));
                    }
                },
                new ScreenEventRegistryItem() {
                    EventName = "OnMaximumHealthChange",
                    RegisterActions = () => {
                        return new Tuple<Action<EventBridge.EventBridgeEvent>, Action<EventBridge.EventBridgeEvent>>(EventBridge.OnGoldChange, new Action<EventBridge.EventBridgeEvent>(@event => {
                            OnMaximumHealthChange(new EventBridge.EventBridgeTypedEvent<EventBridge.ValueChangeEventBody<float>>() {
                                Client = @event.Client,
                                Data = (EventBridge.ValueChangeEventBody<float>) @event.Data
                            });
                        }));
                    }
                }
            };
        }

        /// <summary>
        /// Assembles two lists, one with a list of Event Bridge Event references and their associated executions for
        /// the screen.
        /// </summary>
        /// <returns></returns>
        private Tuple<List<Action<EventBridge.EventBridgeEvent>>, List<Action<EventBridge.EventBridgeEvent>>>
            BuildHooks() {
            List<Action<EventBridge.EventBridgeEvent>> eventBridgeEvents = new List<Action<EventBridge.EventBridgeEvent>>();
            List<Action<EventBridge.EventBridgeEvent>> screenEvents = new List<Action<EventBridge.EventBridgeEvent>>();

            if (EVENTS == null) {
                throw new NullReferenceException("[Avocado]: EVENTS array is null when it should be populated before building hooks. " +
                                                 "Check the EVENTS array or check if you're calling BuildHooks before you're meant to.");
            }

            foreach (ScreenEventRegistryItem item  in EVENTS) {
                MethodInfo eventBaseMethod = typeof(UiScreen).GetMethod(item.EventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
                if (eventBaseMethod == null) {
                    throw new ArgumentException("[Avocado]: The specified event key (" + item.EventName + ") is not a method in the base type.");
                }

                MethodInfo implementedMethod = GetType().GetMethod(item.EventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (implementedMethod != null && implementedMethod.DeclaringType != typeof(UiScreen)) {
                     item.RegisterActions().Deconstruct(out Action<EventBridge.EventBridgeEvent> ebAction, out Action<EventBridge.EventBridgeEvent> scrAction);
                    
                     eventBridgeEvents.Add(ebAction);
                     screenEvents.Add(scrAction);
                }
            }
            
            UiManager.AvoLog("Registered " + (eventBridgeEvents.Count) + " EventBridge Event" + (eventBridgeEvents.Count != 1 ? "s" : "") + " for " + gameObject.name.Replace("(Clone)", "") + ".", false);

            return new Tuple<List<Action<EventBridge.EventBridgeEvent>>, List<Action<EventBridge.EventBridgeEvent>>>(
                eventBridgeEvents,
                screenEvents);
        }

        private void SetupEventHooks() {
            if (_eventHooks != null && _eventHooks.Item1.Count > 0) RemoveEventHooks();

            _eventHooks = BuildHooks();
            (List<Action<EventBridge.EventBridgeEvent>> eventBridgeTargets, List<Action<EventBridge.EventBridgeEvent>> screenEvents) = _eventHooks;

            for (int i = 0; i < Math.Min(eventBridgeTargets.Count, screenEvents.Count); i++) {
                eventBridgeTargets[i] += screenEvents[i];
            }
        }

        private void RemoveEventHooks() {
            if (_eventHooks == null) return;
            
            (List<Action<EventBridge.EventBridgeEvent>> eventBridgeTargets, List<Action<EventBridge.EventBridgeEvent>> screenEvents) = _eventHooks;

            for (int i = 0; i < Math.Min(eventBridgeTargets.Count, screenEvents.Count); i++) {
                eventBridgeTargets[i] -= screenEvents[i];
            }
        }

        private IEnumerator RefreshHandler() {
            int tm = 0;
            
            while (!_collapsed) {
                
                if (_refreshIntervalActions.TryGetValue(tm % 1000, out List<Action> actions))
                    foreach (Action action in actions)
                        action();
                
                yield return new WaitForSeconds(0.01f);
                tm += 100;
            }
        }

        /// <summary>
        /// Run arbitrary code on a set interval refresh schedule attached to the screen
        /// </summary>
        /// <param name="interval">Interval in milliseconds between calls of the callback, interval will be rounded to the nearest
        /// differences below 10 will be rounded down</param>
        /// <param name="callback">The action to be called on <c>interval</c> intervals</param>
        protected void OnIntervalRefresh(int interval, Action callback) {
            if (interval % 10 != 0)
                interval -= interval % 10;
            List<Action> ac = _refreshIntervalActions.GetValueOrDefault(interval, new List<Action>());
            
            ac.Add(callback);
            _refreshIntervalActions.TryAdd(interval, ac);
        }
        
        /// <summary>
        /// Run arbitrary code on a non-frequent interval that is called no sooner than every 5 seconds
        /// </summary>
        /// <param name="callback">The lazy refresh code to run</param>
        protected void OnLazyRefresh(Action callback){
            _lazyRefreshActions.Add(callback);
        }
        
        private void TriggerLazyRefresh() {
            if (_lazyRefreshActions.Count == 0 || DateTime.UtcNow.Subtract(_lastLazyRefresh).TotalSeconds < 5)
                return;
            
            foreach (Action ac in _lazyRefreshActions)
                ac();

            _lastLazyRefresh = DateTime.UtcNow;
        }
    }
}