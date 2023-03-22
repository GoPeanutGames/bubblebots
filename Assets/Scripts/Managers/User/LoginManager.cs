using BubbleBots.Server.Player;
using BubbleBots.Server.Signature;
using UnityEngine;
using UnityEngine.Events;

public class LoginManager : MonoBehaviour
{
	private AutoLogin _autoLogin;
	private UnityAction _callbackOnLoginSuccess;
	private UnityAction _callbackOnLoginFail;

	private void Awake()
	{
		_autoLogin = new AutoLogin();
	}

	private void ClearCallbacks()
	{
		_callbackOnLoginFail = null;
		_callbackOnLoginSuccess = null;
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
		_callbackOnLoginSuccess?.Invoke();
		ClearCallbacks();
	}

	private void GetPlayerFail(string result)
	{
		_callbackOnLoginFail?.Invoke();
		ClearCallbacks();
		Debug.Log("Get player fail: " + result);
	}
	
	private void AutoLoginSuccess(User user)
	{
		LoginResult loginResult = new LoginResult()
		{
			user = user,
			token = UserManager.Instance.GetPlayerJwtToken(),
			web3Info = new PostWeb3Login()
			{
				address = UserManager.Instance.GetPlayerWalletAddress(),
				signature = UserManager.Instance.GetPlayerSignature()
			}
		};
		LoginSuccessSetData(loginResult);
	}
	
	private void AutoLoginFail(string error)
	{
		_callbackOnLoginFail?.Invoke();
		ClearCallbacks();
		Debug.LogError("Auto Login failed with: " + error);
	}
	
	public void TryAutoLogin(UnityAction onSuccess, UnityAction onFail)
	{
		_callbackOnLoginSuccess = onSuccess;
		_callbackOnLoginFail = onFail;
		_autoLogin.TryAutoLogin(AutoLoginSuccess, AutoLoginFail);
	}
}
