using System.Security.Cryptography;
using System.Text;
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
		_googleLogin = new GoogleLogin();
		_appleLogin = new AppleLogin();
		_autoLogin = new AutoLogin();
		_gameScreenLogin.ShowLoading();
	}

	public override void Enable()
	{
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
	}

#if UNITY_IOS
    public override void Update(float delta)
    {
        base.Update(delta);
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _appleLogin.Update();
        }
    }
#endif

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
				if (_gameScreenLogin.SignUpValidation())
				{
					SignUp();
				}

				break;
			case ButtonId.LoginCodeSubmit:
				if (_gameScreenLogin.CodeValidation())
				{
					Submit2FACode();
				}

				break;
			case ButtonId.LoginCodeDidntReceive:
				// SignIn();
				break;
			case ButtonId.LoginResetPassSubmit:
				if (_gameScreenLogin.ResetPassValidation())
				{
					ResetPassword();
				}
				break;
			case ButtonId.LoginSetNewPassSubmit:
				if (_gameScreenLogin.SetNewPassValidation())
				{
					SetNewPassword();
				}

				break;
			case ButtonId.LoginSetNewPassDidntReceiveCode:
				ResetPassword();
				break;
		}
	}

	private void ResetPassword()
	{
		_gameScreenLogin.ShowLoading();
		string email = _gameScreenLogin.GetResetPassInputFieldEmail();
		ResetPassData data = new ResetPassData()
		{
			email = email
		};
		string formData = JsonUtility.ToJson(data);
		ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.ResetPassword, formData, ResetPassSuccess, ResetPassFail);
	}

	private void ResetPassFail(string reason)
	{
		_gameScreenLogin.HideLoading();
	}

	private void ResetPassSuccess(string _)
	{
		_gameScreenLogin.HideLoading();
		_gameScreenLogin.ShowSetNewPassword();
	}

	private void SetNewPassSuccess(string data)
	{
		_gameScreenLogin.HideLoading();
		_gameScreenLogin.ShowSignIn();
	}

	private void SetNewPassFail(string error)
	{
		Debug.LogError("Set new pass fail: " + error);
		_gameScreenLogin.HideLoading();
		_gameScreenLogin.SetNewPassError();
	}

	private void SetNewPassword()
	{
		_gameScreenLogin.ShowLoading();
		string authCode = _gameScreenLogin.GetSetNewPassInputFieldAuthCode();
		string newPass = _gameScreenLogin.GetSetNewPassInputFieldPass();
		var provider = new SHA256Managed();
		var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(newPass));
		string hashString = string.Empty;
		foreach (byte x in hash)
		{
			hashString += $"{x:x2}";
		}

		SetNewPassData data = new SetNewPassData()
		{
			newPassword = hashString,
			token = authCode
		};
		string formData = JsonUtility.ToJson(data);
		ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.SetNewPass, formData, SetNewPassSuccess, SetNewPassFail);
	}

	private void SignUp()
	{
		_gameScreenLogin.ShowLoading();
		string email = _gameScreenLogin.GetSignUpInputFieldEmail();
		string pass = _gameScreenLogin.GetSignUpInputFieldPass();
		var provider = new SHA256Managed();
		var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(pass));
		string hashString = string.Empty;
		foreach (byte x in hash)
		{
			hashString += $"{x:x2}";
		}

		_tempEmail = email;
		_tempHashedPass = hashString;
		EmailPassSignUp data = new EmailPassSignUp()
		{
			email = email,
			password = hashString
		};
		string formData = JsonUtility.ToJson(data);
		ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.EmailPassSignUp, formData, EmailPassSignUpSuccess, EmailPassSignUpFail);
	}

	private void EmailPassSignUpSuccess(string success)
	{
		_gameScreenLogin.HideLoading();
		_gameScreenLogin.Show2FAuth();
	}

	private void EmailPassSignUpFail(string error)
	{
		_gameScreenLogin.HideLoading();
		_gameScreenLogin.SetSignUpWrongError();
		Debug.Log("error: " + error);
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