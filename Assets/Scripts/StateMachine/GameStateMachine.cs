using System;
using System.Collections.Generic;

public class GameStateMachine
{
    private enum StateMachineAction
    {
        None,
        Push,
        Pop,
        PopAll
    }
    public delegate void StateChangedHandler(GameState state);
    public event StateChangedHandler StateChanged = delegate { };

    protected GameState currentState = null;
    private List<GameState> stateStack = null;

    private List<Tuple<StateMachineAction, GameState>> actionsQueue = new List<Tuple<StateMachineAction, GameState>>();
    private Tuple<StateMachineAction, GameState> currentAction = null;

    public virtual GameState GetCurrentState()
    {
        return currentState;
    }

    public virtual void PushState(GameState state)
    {
        actionsQueue.Add(new Tuple<StateMachineAction, GameState>(StateMachineAction.Push, state));
    }
    public virtual void PopState()
    {
        actionsQueue.Add(new Tuple<StateMachineAction, GameState>(StateMachineAction.Pop, null));
    }
    public virtual void PopAll()
    {
        actionsQueue.Add(new Tuple<StateMachineAction, GameState>(StateMachineAction.PopAll, null));
    }

    private void ExecutePushState(GameState state)
    {
        if (stateStack == null)
        {
            stateStack = new List<GameState>();
        }

        if (stateStack.Count > 0)
        {
            GameState lastState = stateStack[stateStack.Count - 1];
            lastState.Disable();
        }

        stateStack.Add(state);
        currentState = state;
        currentState.SetStateMachine(this);
        currentState.Load();
        currentState.Enter();
        currentState.Enable();
        CrashManager.Instance.SetCustomCrashKey(CrashTypes.State, currentState.GetGameStateName());
        StateChanged(currentState);
    }

    private void ExecutePopState()
    {
        if (stateStack != null)
        {
            if (stateStack.Count > 0)
            {
                int index = stateStack.Count - 1;

                stateStack[index].Disable();
                stateStack[index].Exit();
                stateStack[index].InvalidateState();
                stateStack[index].Unload();

                stateStack.RemoveAt(index);

                currentState = null;
            }
            if (stateStack.Count > 0)
            {
                int index = stateStack.Count - 1;
                stateStack[index].Enable();
                currentState = stateStack[index];
                CrashManager.Instance.SetCustomCrashKey(CrashTypes.State, currentState.GetGameStateName());
            }
        }
    }

    public virtual void ForceClean()
    {
        currentState = null;
        stateStack?.Clear();
    }

    private void ExecutePopAll()
    {
        while (stateStack != null && stateStack.Count > 0)
        {
            int index = stateStack.Count - 1;
            stateStack [index].Disable();
            stateStack[index].Exit();
            stateStack[index].InvalidateState();
            stateStack[index].Unload();
            stateStack.RemoveAt(index);
        }
        currentState = null;
    }

    public virtual void Update(float delta)
    {
        CheckAndHandleNextAction();

        if (currentState != null && currentState.IsStateValid)
        {
            currentState.Update(delta);
        }
    }

    /// <summary>
    /// Extract from queue and execute the wanted action
    /// </summary>
    private void CheckAndHandleNextAction()
    {
        if (actionsQueue.Count > 0 && currentAction == null)
        {
            currentAction = actionsQueue[0];
            actionsQueue.RemoveAt(0);
            switch (currentAction.Item1)
            {
                case StateMachineAction.Push:
                    {
                        ExecutePushState(currentAction.Item2);
                    }
                    break;
                case StateMachineAction.Pop:
                    {
                        ExecutePopState();
                    }
                    break;
                case StateMachineAction.PopAll:
                    {
                        ExecutePopAll();
                    }
                    break;
            }
            currentAction = null;
        }
    }

    public List<GameState> GetFinalStackAfterAllPendingActions()
    {
        List<GameState> stack = new List<GameState>();
        stack.AddRange(stateStack);

        for (int i = 0; i < actionsQueue.Count; i++)
        {
            switch (actionsQueue[i].Item1)
            {
                case StateMachineAction.Push:
                    {
                        stack.Add(actionsQueue[i].Item2);
                    }
                    break;
                case StateMachineAction.PopAll:
                    {
                        stack.Clear();
                    }
                    break;
                case StateMachineAction.Pop:
                    {
                        if (stateStack.Count > 0)
                        {
                            stack.RemoveAt(stateStack.Count - 1);
                        }
                    }
                    break;
            }
        }
        return stack;
    }

    public bool IsNavigating()
    {
        return actionsQueue.Count > 0;
    }
}