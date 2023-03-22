using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using UnityEngine;

public class GameStateOptions : GameState
{
	private GamePopupOptions _gamePopupOptions;
	private GameScreenDarkenedBg _darkenedBg;

	private AvatarInformation _finalAvatar;

	public override string GetGameStateName()
	{
		return "Game state options";
	}

	public override void Enter()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>(true);
		_gamePopupOptions = Screens.Instance.PushScreen<GamePopupOptions>();
		_gamePopupOptions.StartOpen();
		_finalAvatar = UserManager.Instance.GetPlayerAvatar();
		Screens.Instance.BringToFront<GamePopupOptions>();
	}

	public override void Enable()
	{
		_gamePopupOptions.RefreshPlayerUsername();
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
	}

	private void OnGameEvent(GameEventData data)
	{
		if (data.eventName == GameEvents.ButtonTap)
		{
			OnButtonTap(data);
		}
	}

	private void OnButtonTap(GameEventData data)
	{
		GameEventString customButtonData = data as GameEventString;
		switch (customButtonData.stringData)
		{
			case ButtonId.OptionsClose:
				Screens.Instance.PopScreen(_darkenedBg);
				stateMachine.PopState();
				break;
			case ButtonId.OptionsSyncProgress:
				stateMachine.PopState();
				stateMachine.PushState(new GameStateLogin());
				break;
			case ButtonId.OptionsChangePicture:
				ChangePicture();
				break;
			case ButtonId.OptionsChangeName:
				stateMachine.PushState(new GameStateChangeNickname());
				break;
			case ButtonId.OptionsSave:
				SaveSettings();
				stateMachine.PopState();
				break;
		}
	}

	private void GetNextPicture()
	{
		List<NFTImage> images = new List<NFTImage>(UserManager.Instance.NftManager.GetAvailableNfts());
		if (_finalAvatar.isNft)
		{
			int nftIndex = images.FindIndex((image) => image.tokenId == _finalAvatar.id);
			if (images.Count <= nftIndex + 1)
			{
				_finalAvatar.isNft = false;
				_finalAvatar.id = 0;
			}
			else
			{
				_finalAvatar.id = images[nftIndex + 1].tokenId;
			}
		}
		else
		{
			_finalAvatar.id++;
			if (_finalAvatar.id == 3 && images.Count > 0)
			{
				_finalAvatar.isNft = true;
				_finalAvatar.id = images[0].tokenId;
			}
			else if (_finalAvatar.id == 3)
			{
				_finalAvatar.id = 0;
			}
		}
	}

	private void ChangePicture()
	{
		GetNextPicture();
		_gamePopupOptions.SetPlayerAvatar(_finalAvatar);
	}

	private void SaveSettings()
	{
		UserManager.Instance.ChangePlayerAvatar(_finalAvatar.id, _finalAvatar.isNft);
		if (_finalAvatar.isNft)
		{
			SetDefaultNftData data = new SetDefaultNftData()
			{
				tokenId = _finalAvatar.id,
				address = UserManager.Instance.GetPlayerWalletAddress(),
				signature = UserManager.Instance.GetPlayerSignature()
			};
			string formData = JsonUtility.ToJson(data);
			ServerManager.Instance.SendPlayerDataToServer(PlayerAPI.SetDefaultNFT, formData, null, null);
		}

		bool musicOn = _gamePopupOptions.GetFinalMusicValue();
		bool hintsOn = _gamePopupOptions.GetFinalHintsValue();
		if (musicOn)
		{
			SoundManager.Instance.UnMute();
		}
		else
		{
			SoundManager.Instance.Mute();
		}

		UserManager.Instance.SetPlayerHints(hintsOn);
	}

	private void Logout()
	{
		//TODO:
// 		UserManager.ClearPrefs();
// 		ServerManager.Instance.GetLoginSignatureDataFromServer(SignatureLoginAPI.Logout, LogoutSuccess, "", LogoutSuccess);
// #if UNITY_ANDROID
// 		PlayGamesPlatform.Instance.SignOut();
// #endif
	}

	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		Screens.Instance.PopScreen(_gamePopupOptions);
	}
}