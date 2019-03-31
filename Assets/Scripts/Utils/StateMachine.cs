using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class StateMachine<T>
    {
        public delegate void StateFunc();

        private State _currentState;
        private readonly Dictionary<T, State> _states = new Dictionary<T, State>();

        public void Add(T id, StateFunc enter, StateFunc update, StateFunc leave)
        {
            _states.Add(id, new State(id, enter, update, leave));
        }

        private class State
        {
            public readonly T id;
            public readonly StateFunc enter;
            public readonly StateFunc leave;
            public readonly StateFunc update;

            public State(T id, StateFunc enter, StateFunc update, StateFunc leave)
            {
                this.id = id;
                this.enter = enter;
                this.update = update;
                this.leave = leave;
            }
        }

        public T CurrentState()
        {
            return _currentState.id;
        }

        public void SwitchTo(T id)
        {
            bool hasState = _states.TryGetValue(id, out State newState);
            Debug.Assert(hasState, $"Trying to switch to unknown state {id}");
            Debug.Assert(_currentState == null || !CurrentState().Equals(id),
                $"Trying to switch to {id} but that is already current state.");

            GameDebug.Log($"Switching state: {_currentState?.id.ToString() ?? "null"} -> {id}");

            _currentState?.leave?.Invoke();
            newState.enter?.Invoke();
            _currentState = newState;
        }
    }
}