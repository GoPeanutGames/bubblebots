using UnityEngine;

public class MainController : MonoBehaviour
{
    private GameStateMachine stateMachine = new GameStateMachine();

    void Start()
    {
        stateMachine.PopAll();
        stateMachine.PushState(new GameStateLogin());
       
    }


    public void Update()
    {
        stateMachine.Update(Time.deltaTime);

    }
}
