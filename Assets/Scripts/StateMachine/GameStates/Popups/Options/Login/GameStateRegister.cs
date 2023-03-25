public class GameStateRegister : GameState
{
	private GamePopupRegister _gamePopupRegister;
	private GameScreenLoading _gameScreenLoading;
	
	public override string GetGameStateName()
	{
		return "game state register";
	}
	
	public override void Enter()
	{
		_gamePopupRegister = Screens.Instance.PushScreen<GamePopupRegister>();
	}

	public override void Enable()
	{
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
	}
    
	private void OnGameEvent(GameEventData gameEvent)
	{
		if (gameEvent.eventName == GameEvents.ButtonTap)
		{
			OnButtonTap(gameEvent);
		}
	}

	private void OnButtonTap(GameEventData data)
	{
		GameEventString buttonTapData = data as GameEventString;
		switch (buttonTapData.stringData)
		{
			case ButtonId.LoginSignUpSubmit:
				if (_gamePopupRegister.SignUpValidation())
				{
					_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
					UserManager.Instance.loginManager.SignUp(_gamePopupRegister.GetSignUpInputFieldEmail(), _gamePopupRegister.GetSignUpInputFieldPass(), EmailPassSignUpSuccess, EmailPassSignUpFail);
				}
				break;
			case ButtonId.LoginSignUpGoBack:
				stateMachine.PopState();
				stateMachine.PushState(new GameStateLogin());
				break;
		}
	}

	private void EmailPassSignUpSuccess()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		stateMachine.PopState();
		stateMachine.PushState(new GameStateTwoFA());
	}

	private void EmailPassSignUpFail()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		_gamePopupRegister.SetSignUpWrongError();
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}
    
	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupRegister);
	}
}