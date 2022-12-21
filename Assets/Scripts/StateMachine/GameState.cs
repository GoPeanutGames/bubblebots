using System.Threading.Tasks;
using UnityEngine;

public abstract class GameState
{
    protected GameStateMachine stateMachine = null;

    public abstract string GetGameStateName();
    public bool IsStateValid { get => isStateValid; }

    private bool isStateValid = true;

    public GameState() { }
    public virtual void Load() { /*Debug.Log("===== " + GetGameStateName() + " Load");*/ }
    public virtual void Unload() { /*Debug.Log("===== " + ShortName + " UnLoad");*/}
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Enable() {/* Debug.Log("===== " + ShortName + " Enable"); */}
    public virtual void Disable() {/* Debug.Log("===== " + ShortName + " Disable"); */}
    public virtual void Update(float delta) { }

    public void SetStateMachine(GameStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    public void InvalidateState()
    {
        isStateValid = false;
    }
}
