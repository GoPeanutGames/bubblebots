using UnityEngine;

public class GameStateStart : GameState
{
	private AutoLogin _autoLogin;
	private GameScreenLoading _gameScreenLoading;

	private void GoToHome()
	{
		stateMachine.PopState();
		stateMachine.PushState(new GameStateHome());
	}
	
	private void AutoLoginSuccess()
	{
		UserManager.PlayerType = PlayerType.LoggedInUser;
		GoToHome();
	}

	private void AutoLoginFail()
	{
		Debug.Log($"{nameof(GameStateStart)}::{nameof(AutoLoginFail)}");
		UserManager.PlayerType = PlayerType.Guest;
		AnalyticsManager.Instance.InitAnalyticsGuest();
		UserManager.Instance.SetPlayerUserName("Guest user", false);
		GoToHome();
	}
	
	public override string GetGameStateName()
	{
		return "game state start";
	}

	public override void Enter()
	{
		_gameScreenLoading = Screens.Instance.PushScreen<GameScreenLoading>();
		UserManager.Instance.loginManager.TryAutoLogin(AutoLoginSuccess, AutoLoginFail);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gameScreenLoading);
	}
}
