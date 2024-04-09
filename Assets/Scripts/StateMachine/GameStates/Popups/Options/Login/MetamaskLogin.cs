using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BubbleBots.Server.Signature;
#if  !UNITY_IOS
using MetaMask;
using MetaMask.Models;
using MetaMask.Unity;
#endif
using UnityEngine;
using UnityEngine.Events;

public class MetamaskLogin
{
#if UNITY_WEBGL
	[DllImport("__Internal")]
	private static extern void Login(bool isDev);

	[DllImport("__Internal")]
	private static extern void RequestSignature(string schema, string address);
#endif
	
#if  !UNITY_IOS
	private static string WalletAddress => MetaMaskUnity.Instance.Wallet.ConnectedAddress.ToLower();
#endif
	private static long ChainId => EnvironmentManager.Instance.IsDevelopment() ? ChainDataReference.SepoliaChainId : ChainDataReference.BlastChainId;
	private static Chain ChainData => EnvironmentManager.Instance.IsDevelopment() ? ChainDataReference.SepoliaChain : ChainDataReference.BlastChain;
	
	private UnityAction<LoginResult> _callbackOnSuccess;
	private UnityAction _callbackOnFail;
	private string _tempAddress;
	private static bool _metamaskInitialised = false;

	public MetamaskLogin()
	{
		GameEventsManager.Instance.AddGlobalListener(OnMetamaskEvent);
		Initialise();
	}
	
	private void Initialise()
	{
#if  !UNITY_IOS
		MetaMaskConfig metaMaskConfig = EnvironmentManager.Instance.GetMetamaskConfig();
		if (!_metamaskInitialised){
			MetaMaskUnity.Instance.Initialize(metaMaskConfig);
			_metamaskInitialised = true;
		}
#endif
	}
	
	private void OnMetamaskEvent(GameEventData data)
	{
		GameEventString metamaskEvent = data as GameEventString;
		if (data.eventName == GameEvents.MetamaskSuccess)
		{
			MetamaskConnectSuccess(metamaskEvent.stringData);
		}
		else if (data.eventName == GameEvents.SignatureSuccess)
		{
			MetamaskSignatureSuccess(metamaskEvent.stringData);
		}
	}
	
	public void MetamaskSignIn(UnityAction<LoginResult> onSuccess, UnityAction onFail)
	{
		_callbackOnSuccess = onSuccess;
		_callbackOnFail = onFail;
		if (Application.isMobilePlatform){
#if  !UNITY_IOS
			MetaMaskUnity.Instance.Wallet.EthereumRequestFailedHandler += MobileConnectMetamaskFailHandler;
			MetaMaskUnity.Instance.Wallet.WalletConnectedHandler += MetamaskMobileConnected;
			MetaMaskUnity.Instance.Wallet.WalletDisconnectedHandler += MobileWalletDisconnected;
			MetaMaskUnity.Instance.Wallet.WalletAuthorizedHandler += MetamaskMobileAuthorized;
			MetaMaskUnity.Instance.Connect();
#endif
		}
		else{
#if !UNITY_EDITOR
			bool isDev = EnvironmentManager.Instance.IsDevelopment();
#if UNITY_WEBGL
			Login(isDev);
#endif
#elif UNITY_EDITOR
            string signature = "0x821ee840b49c4294850eb51319b9ddb85504190ee38f4dec00f81b13b64fbd6a388d75df615de9aaac22adbc6b565134eaefa25e3b09223313932323e48c4aba1b";
			_tempAddress = "0x5d7167477bf3abedb261b4a5a1c150b87e6837a9";
			MetamaskSignatureSuccess(signature);
#endif
		}
	}
	
	private void MetamaskConnectSuccess(string address)
	{
		_tempAddress = address;
		ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Get, (schema) => { MetamaskRequestSignature(schema.ToString()); }, address);
	}

	private void MetamaskConnectFail()
	{
		_callbackOnFail?.Invoke();
	}

	private void MetamaskRequestSignature(string schema)
	{
#if UNITY_WEBGL
		if (Application.isMobilePlatform){
			RequestMobileSignature(schema, _tempAddress);
		}
		else{
			RequestSignature(schema, _tempAddress);
		}
#endif
	}

	private void MetamaskSignatureSuccess(string signature)
	{
		_callbackOnSuccess.Invoke(new LoginResult()
		{
			token = signature,
			web3Info = new PostWeb3Login()
			{
				signature = signature,
				address = _tempAddress
			}
		});
	}

	private void MetamaskSignatureFail()
	{
		_callbackOnFail?.Invoke();
	}

	#region MOBILE ONLY
#if  !UNITY_IOS
	
		private void MobileWalletDisconnected(object sender, EventArgs e)
		{
			LogOutMetamask();
		}
			
		private void MobileConnectMetamaskFailHandler(object sender, EventArgs e)
		{
			MetaMaskEthereumRequestFailedEventArgs failedEventArgs = e as MetaMaskEthereumRequestFailedEventArgs;

			MetaMaskUnity.Instance.Wallet.EthereumRequestFailedHandler -= MobileConnectMetamaskFailHandler;

			LogOutMetamask();
		}
		public void LogOutMetamask()
		{
			if (Application.isMobilePlatform){
				MetaMaskUnity.Instance.Wallet.WalletConnectedHandler = null;
				MetaMaskUnity.Instance.Wallet.WalletAuthorizedHandler = null;
				MetaMaskUnity.Instance.Wallet.ChainIdChangedHandler = null;
				MetaMaskUnity.Instance.Wallet.AccountChangedHandler = null;
				MetaMaskUnity.Instance.Wallet.WalletDisconnectedHandler = null;
				MetaMaskUnity.Instance.Wallet.EthereumRequestFailedHandler = null;
				if (MetaMaskUnity.Instance.Wallet.IsConnected){
					MetaMaskUnity.Instance.Disconnect();
				}
				MetaMaskUnity.Instance.Wallet.EndSession(true);
			}
		}

		private void MetamaskMobileConnected(object sender, EventArgs e)
		{
			MetaMaskUnity.Instance.Wallet.WalletConnectedHandler -= MetamaskMobileConnected;
		}

		private void MetamaskMobileAuthorized(object sender, EventArgs e)
		{
			MetaMaskUnity.Instance.Wallet.WalletAuthorizedHandler -= MetamaskMobileAuthorized;
			if (MetaMaskUnity.Instance.Wallet.ChainId != ChainId){
				MetaMaskUnity.Instance.Wallet.ChainIdChangedHandler += OnChainSwitched;
				MetaMaskUnity.Instance.Wallet.AccountChangedHandler += OnAccountChangeHandler;
				SwitchChain(ChainData);
			}
			else{
				MetamaskConnectSuccess(WalletAddress);
			}
		}

		private void OnChainSwitched(object sender, EventArgs e)
		{
			if (MetaMaskUnity.Instance.Wallet.ChainId == ChainId){
				MetamaskConnectSuccess(WalletAddress);
				MetaMaskUnity.Instance.Wallet.ChainIdChangedHandler -= OnChainSwitched;
			}
		}

		private void OnAccountChangeHandler(object sender, EventArgs e)
		{
			if (MetaMaskUnity.Instance.Wallet.ChainId == ChainId){
				MetamaskConnectSuccess(WalletAddress);
				MetaMaskUnity.Instance.Wallet.AccountChangedHandler -= OnAccountChangeHandler;
			}
		}

		private async Task<object> SwitchChain(Chain chain)
		{
			MetaMaskEthereumRequest signatureRequest = new()
			{
				Method = "wallet_addEthereumChain",
				Parameters = new[] { chain }
			};
			return MetaMaskUnity.Instance.Wallet.Request(signatureRequest);
		}

		private async void RequestMobileSignature(string schema, string address)
		{
			MetaMaskEthereumRequest signatureRequest = new()
			{
				Method = "eth_signTypedData_v4",
				Parameters = new[] { address, schema }
			};
			object result = await MetaMaskUnity.Instance.Wallet.Request(signatureRequest);
			MetaMaskUnity.Instance.Wallet.EthereumRequestFailedHandler -= MobileConnectMetamaskFailHandler;
			MetamaskSignatureSuccess(result as string);
		}
#endif
	#endregion
}