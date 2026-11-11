using UnityEngine;

namespace Avocado {
    public class AvocadoAnimatorEventHook : StateMachineBehaviour {
        private UiScreen _screen;
        private string _key;
        private bool _bound;
        private string _targetStateName;

        public void BindAvoScreen(UiScreen screen, string key) {
            this._screen = screen;
            this._key = key;
            _bound = true;
        }

        public void SetTargetState(string targetStateName) {
            this._targetStateName = targetStateName;
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_bound && _targetStateName != null && stateInfo.IsName(_targetStateName)) {
                _screen.InvokeAnimatorExited(_key);
                _targetStateName = null;
            }
        }
    }
}