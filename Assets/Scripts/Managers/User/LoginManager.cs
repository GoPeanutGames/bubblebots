using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using BubbleBots.Server.Player;
using BubbleBots.Server.Referral;
using BubbleBots.Server.Signature;
using GooglePlayGames;
using UnityEngine;
using UnityEngine.Events;

public class LoginManager : MonoBehaviour
{
    private GoogleLogin _googleLogin;
    private AutoLogin _autoLogin;
    private AppleLogin _appleLogin;
    private MetamaskLogin _metamaskLogin;
    private UnityAction _callbackOnSuccess;
    private UnityAction _callbackOnFail;

    private string _tempAddress;
    private string _tempEmail;
    private string _tempHashedPass;

    private void Awake()
    {
        _googleLogin = new GoogleLogin();
        _appleLogin = new AppleLogin();
        _autoLogin = new AutoLogin();
    }

    private void Start()
    {
        _metamaskLogin = new MetamaskLogin();
    }

#if UNITY_IOS
    public void Update()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _appleLogin.Update();
        }
    }
#endif

    private void ClearCallbacks()
    {
        _callbackOnFail = null;
        _callbackOnSuccess = null;
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
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }

    private void GetPlayerFail(string result)
    {
        _callbackOnFail?.Invoke();
        ClearCallbacks();
        Debug.Log("Get player fail: " + result);
    }

    private void AutoLoginSuccess(User user)
    {
        Debug.Log("Auto Login success ");
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
        Debug.Log($"{nameof(LoginManager)}::{nameof(AutoLoginFail)}");
        _callbackOnFail?.Invoke();
        ClearCallbacks();
        Debug.LogError("Auto Login failed with: " + error);
    }

    public void TryAutoLogin(UnityAction onSuccess, UnityAction onFail)
    {
        Debug.Log($"{nameof(LoginManager)}::{nameof(TryAutoLogin)}");
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        _autoLogin.TryAutoLogin(AutoLoginSuccess, AutoLoginFail);
    }

    public void MetamaskSignIn(UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        _metamaskLogin.MetamaskSignIn(MetamaskLoginSuccess, null);
    }

    private void MetamaskLoginSuccess(LoginResult loginResult)
    {
        LoginSuccessSetData(loginResult);
    }

    public void GoogleSignIn(UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        _googleLogin.StartLogin(LoginSuccessGoogleOrApple, GoogleLoginFail);
    }

    public void AppleSignIn(UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        _appleLogin.StartLogin(LoginSuccessGoogleOrApple, AppleLoginFail);
    }

    private void LoginSuccessGoogleOrApple(LoginResultGoogleOrApple result)
    {
        LoginResult loginResult = new LoginResult()
        {
            token = result.jwt,
            user = result.user,
            web3Info = result.web3Info
        };
        LoginSuccessSetData(loginResult);
    }

    private void GoogleLoginFail(string reason)
    {
        Debug.LogError("Google login fail: " + reason);
        _callbackOnFail?.Invoke();
        ClearCallbacks();
    }

    private void AppleLoginFail(string reason)
    {
        Debug.LogError("Apple login fail: " + reason);
        _callbackOnFail?.Invoke();
        ClearCallbacks();
    }

    public void SignUp(string email, string pass, UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
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

    private void EmailPassSignUpFail(string error)
    {
        _callbackOnFail?.Invoke();
        ClearCallbacks();
        Debug.Log("error: " + error);
    }

    public void SignIn(string email, string pass, bool useExisting, UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        var provider = new SHA256Managed();
        var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(pass));
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += $"{x:x2}";
        }


        EmailPassSignUp data = new EmailPassSignUp()
        {
            email = email,
            password = hashString
        };
        if (useExisting)
        {
            data.email = _tempEmail;
            data.password = _tempHashedPass;
        }
        else
        {
            _tempEmail = email;
            _tempHashedPass = hashString;
        }
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login1StStep, formData, EmailPassSignUpSuccess, SignInFail);
    }

    private void EmailPassSignUpSuccess(string success)
    {
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }

    private void SignInFail(string error)
    {
        _callbackOnFail?.Invoke();
        ClearCallbacks();
        Debug.Log("error: " + error);
    }

    public void Submit2FaCode(string code, UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        Login2ndStep data = new Login2ndStep()
        {
            email = _tempEmail,
            password = _tempHashedPass,
            twoFaCode = code
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Login2NdStep, formData, TwoFaCodeSuccess, TwoFaCodeFail);
    }

    private void TwoFaCodeSuccess(string success)
    {
        Debug.Log("success: " + success);
        LoginResult result = JsonUtility.FromJson<LoginResult>(success);
        LoginSuccessSetData(result);
    }

    private void TwoFaCodeFail(string error)
    {
        Debug.Log("error: " + error);
        _callbackOnFail?.Invoke();
        ClearCallbacks();
    }

    public void ResetPassword(string resetEmail, bool useStored, UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
        string email = resetEmail;
        if (useStored)
        {
            email = _tempEmail;
        }
        else
        {
            _tempEmail = resetEmail;
        }
        ResetPassData data = new ResetPassData()
        {
            email = email
        };
        string formData = JsonUtility.ToJson(data);
        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.ResetPassword, formData, ResetPassSuccess, ResetPassFail);
    }

    private void ResetPassSuccess(string reason)
    {
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }

    private void ResetPassFail(string reason)
    {
        _callbackOnFail?.Invoke();
        ClearCallbacks();
    }

    public void SetNewPassword(string authCode, string newPass, UnityAction onSuccess, UnityAction onFail)
    {
        _callbackOnSuccess = onSuccess;
        _callbackOnFail = onFail;
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

    private void SetNewPassSuccess(string reason)
    {
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }

    private void SetNewPassFail(string error)
    {
        Debug.LogError("Set new pass fail: " + error);
        _callbackOnFail?.Invoke();
        ClearCallbacks();
    }

    public void SignOut(UnityAction successCallback)
    {
        _callbackOnSuccess = successCallback;
        UserManager.ClearPrefs();
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Logout, LogoutSuccess, "", LogoutSuccess);
#if UNITY_ANDROID
		PlayGamesPlatform.Instance.SignOut();
#endif
    }

    private void LogoutSuccess(string data)
    {
        UserManager.PlayerType = PlayerType.Guest;
        UserManager.Instance.GetUserOrSetDefault();
        UserManager.Instance.GetPlayerResources();
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }

    public void AskDeleteAccount(UnityAction onSuccess)
    {
        _callbackOnSuccess = onSuccess;
        ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.AskDelete, AskDeleteSuccess);
    }

    private void AskDeleteSuccess(string data)
    {
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }

    public void DeleteAccount(string code, UnityAction onSuccess)
    {
        _callbackOnSuccess = onSuccess;
        DeleteAccPost data = new DeleteAccPost()
        {
            token = code
        };
        string formData = JsonUtility.ToJson(data);

        ServerManager.Instance.SendLoginDataToServer(SignatureLoginAPI.Delete, formData, DeleteAccountSuccess);
    }

    private void DeleteAccountSuccess(string data)
    {
        UserManager.ClearPrefs();
#if UNITY_ANDROID
		PlayGamesPlatform.Instance.SignOut();
#endif
        UserManager.Instance.GetUserOrSetDefault();
        UserManager.PlayerType = PlayerType.Guest;
        UserManager.Instance.GetPlayerResources();
        _callbackOnSuccess?.Invoke();
        ClearCallbacks();
    }
}

[Serializable]
public class Web3Data
{
    public string address;
    public string signature;
}

[Serializable]
public class CheckWeb3LoginResponse
{
    public bool status;
}