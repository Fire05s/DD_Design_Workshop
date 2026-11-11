using System;

namespace Avocado {
    /// <summary>
    /// The <c>EventBridge</c> is responsible for piping events to logical UI
    /// elements and triggering other UI system functions
    /// </summary>
    public static class EventBridge {
        private static SyncedStats _client;
        
        public class ValueChangeEventBody<T> {
            public T OldAmount;
            public T NewAmount;
        }
        
        public struct EventBridgeTypedEvent<T> {
            public T Data;
            public SyncedStats Client;
        }

        public struct EventBridgeEvent {
            public object Data;
            public SyncedStats Client;
        }
        
        public static Action<EventBridgeEvent> OnGoldChange;
        public static Action<EventBridgeEvent> OnResistanceChange;
        public static Action<EventBridgeEvent> OnLifestealChange;
        public static Action<EventBridgeEvent> OnIsAliveChange;
        public static Action<EventBridgeEvent> OnCurrentHealthChange;
        public static Action<EventBridgeEvent> OnMovementSpeedChange;
        public static Action<EventBridgeEvent> OnMaximumHealthChange;

        private static EventBridgeEvent AssembleEvent (object data) {
            return new EventBridgeEvent() {
                Data = data,
                Client = _client
            };
        }
        
        public static void Initialize(SyncedStats client) {
            if (!client.isOwner)
                throw new Exception(
                    "Avocado (UI) Event Bridge attempted to initialize with a non-authoritative client");

            _client = client;

            client.Gold.onChangedWithOld += (oldValue, newValue) => OnGoldChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<int>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));
            
            client.Resistance.onChangedWithOld += (oldValue, newValue) => OnResistanceChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<float>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));
            
            client.Lifesteal.onChangedWithOld += (oldValue, newValue) => OnLifestealChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<float>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));
            
            client.IsAlive.onChangedWithOld += (oldValue, newValue) => OnIsAliveChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<bool>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));
            
            client.CurrentHealth.onChangedWithOld += (oldValue, newValue) => OnCurrentHealthChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<float>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));
            
            client.MovementSpeed.onChangedWithOld += (oldValue, newValue) => OnMovementSpeedChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<float>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));
            
            client.MaximumHealth.onChangedWithOld += (oldValue, newValue) => OnMaximumHealthChange?.Invoke(
                AssembleEvent(new ValueChangeEventBody<float>() {
                    OldAmount = oldValue,
                    NewAmount = newValue
                }));

            
            UiManager.Initialize();
        }
    }
}