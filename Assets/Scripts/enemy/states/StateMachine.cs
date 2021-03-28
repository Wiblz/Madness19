using UnityEngine;

public abstract class StateMachine : MonoBehaviour {
    public State CurrentState;
    public void SetState(State newState) {
        CurrentState = newState;
        CurrentState.Start();
    }
}
