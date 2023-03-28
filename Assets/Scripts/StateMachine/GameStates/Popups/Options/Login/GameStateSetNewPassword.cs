public class GameStateSetNewPassword : GameState
{
	private GameScreenDarkenedBg _darkenedBg;
	private GamePopupSetNewPassword _gamePopupSetNewPassword;
	private GameScreenLoading _gameScreenLoading;
	
	public override string GetGameStateName()
	{
		return "game state set new password";
	}
	
	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>();
		_gamePopupSetNewPassword = Screens.Instance.PushScreen<GamePopupSetNewPassword>();
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
			case ButtonId.LoginSetNewPassSubmit:
				_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
				if (_gamePopupSetNewPassword.SetNewPassValidation())
				{
					UserManager.Instance.loginManager.SetNewPassword(_gamePopupSetNewPassword.GetSetNewPassInputFieldAuthCode(), _gamePopupSetNewPassword.GetSetNewPassInputFieldPass(), SetNewPassSuccess, SetNewPassFail);
				}
				break;
			case ButtonId.LoginSetNewPassDidntReceiveCode:
				UserManager.Instance.loginManager.ResetPassword("", true, null, null);
				break;
			case ButtonId.LoginSetNewPassGoBack:
				stateMachine.PopState();
				break;
		}
	}
	
	private void SetNewPassSuccess()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		stateMachine.PopState();
	}

	private void SetNewPassFail()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
		_gamePopupSetNewPassword.SetNewPassError();
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
		Screens.Instance.PopScreen(_gamePopupSetNewPassword);
		Screens.Instance.PopScreen(_darkenedBg);
	}
}
