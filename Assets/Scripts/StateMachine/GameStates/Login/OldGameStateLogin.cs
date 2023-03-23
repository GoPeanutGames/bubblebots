using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;
using UnityEngine;

public class OldGameStateLogin : GameState
{
	private GameScreenLogin _gameScreenLogin;
	private GoogleLogin _googleLogin;
	private AutoLogin _autoLogin;
	private AppleLogin _appleLogin;
	private string _tempEmail;
	private string _tempHashedPass;

	public override string GetGameStateName()
	{
		return "game state login";
	}

	public override void Enter()
	{
		_gameScreenLogin = Screens.Instance.PushScreen<GameScreenLogin>(true);
		_gameScreenLogin.ShowLoading();
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
			case ButtonId.LoginCodeSubmit:
				if (_gameScreenLogin.CodeValidation())
				{
					Submit2FACode();
				}

				break;
			case ButtonId.LoginCodeDidntReceive:
				// SignIn();
				break;
		}
	}

	private void Submit2FACode()
	{
		_gameScreenLogin.ShowLoading();
		string code = _gameScreenLogin.GetLoginInputFieldCode();
		Login2ndStep data = new Login2ndStep()
		{
			email = _tempEmail,
			password = _tempHashedPass,
			twoFaCode = code
		};
		string formData = JsonUtility.ToJson(data);
		ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login2NdStep, formData, TwoFACodeSuccess, TwoFACodeFail);
	}

	private void TwoFACodeSuccess(string success)
	{
		_gameScreenLogin.HideLoading();
		Debug.Log("success: " + success);
		LoginResult result = JsonUtility.FromJson<LoginResult>(success);
		LoginSuccessSetData(result);
	}

	private void TwoFACodeFail(string error)
	{
		_gameScreenLogin.HideLoading();
		Debug.Log("error: " + error);
		_gameScreenLogin.Set2FAError();
	}

	private void GoToMainMenu()
	{
		stateMachine.PopState();
		stateMachine.PushState(new GameStateHome());
	}

	private void LoginSuccessSetData(LoginResult result)
	{
		AnalyticsManager.Instance.InitAnalyticsWithWallet(result.web3Info.address);
		UserManager.PlayerType = PlayerType.LoggedInUser;
		UserManager.Instance.SetJwtToken(result.token);
		UserManager.Instance.SetWalletAddress(result.web3Info.address);
		UserManager.Instance.SetSignature(result.web3Info.signature);
		StoreManager.Instance.InitialiseStore(result.web3Info.address);
		ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.Get, GetPlayerSuccess, result.web3Info.address, GetPlayerFail);
		UserManager.Instance.NftManager.DownloadPLayerNfts();
	}

	private void GetPlayerSuccess(string result)
	{
		GetPlayerDataResult playerData = JsonUtility.FromJson<GetPlayerDataResult>(result);
		UserManager.Instance.SetPlayerUserName(playerData.nickname, false);
		SoundManager.Instance.PlayLoginSuccessSfx();
		GoToMainMenu();
	}

	private void GetPlayerFail(string result)
	{
		Debug.Log("Get player fail: " + result);
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gameScreenLogin);
	}
}