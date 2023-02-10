using UnityEngine;

public class MainController : MonoBehaviour, IErrorManager
{
    private readonly GameStateMachine _stateMachine = new();

    void Start()
    {
        _stateMachine.PopAll();
        _stateMachine.PushState(new GameStateLogin());
    }
    public void Update()
    {
        _stateMachine.Update(Time.deltaTime);
    }

    public void DisabledAccountError()
    {
        if (_stateMachine.GetCurrentState().GetType() != typeof(GameStateAccountDisabled))
        {
            _stateMachine.PushState(new GameStateAccountDisabled());
        }
    }

    public void AccessDeniedError()
    {
        if (_stateMachine.GetCurrentState().GetType() != typeof(GameStateLogin))
        {
            _stateMachine.PushState(new GameStateAccessDenied());
        }
    }
}
