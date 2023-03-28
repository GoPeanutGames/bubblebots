public class GameStateForgotPassword : GameState
{
	private GameScreenDarkenedBg _darkenedBg;
	private GamePopupForgotPassword _gamePopupForgotPassword;
	private GameScreenLoading _gameScreenLoading;
	
	public override string GetGameStateName()
	{
		return "game state forgot password";
	}

	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
		_gamePopupForgotPassword = Screens.Instance.PushScreen<GamePopupForgotPassword>();
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
			case ButtonId.LoginResetPassSubmit:
				if (_gamePopupForgotPassword.ResetPassValidation())
				{
					_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
					UserManager.Instance.loginManager.ResetPassword(_gamePopupForgotPassword.GetResetPassInputFieldEmail(), false, ResetPassSuccess, ResetPassFail);
				}
				break;
			case ButtonId.LoginResetPassGoBack:
				stateMachine.PopState();
				break;
		}
	}

	private void ResetPassFail()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
	}

	private void ResetPassSuccess()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		stateMachine.PopState();
		stateMachine.PushState(new GameStateSetNewPassword());
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
		Screens.Instance.PopScreen(_gamePopupForgotPassword);
		Screens.Instance.PopScreen(_darkenedBg);
	}
}
