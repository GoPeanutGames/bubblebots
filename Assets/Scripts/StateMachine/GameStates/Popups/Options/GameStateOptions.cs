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
		_finalAvatar = UserManager.Instance.GetPlayerAvatar();
	}

	public override void Enable()
	{
		_darkenedBg = Screens.Instance.PushScreen<GameScreenDarkenedBg>(true);
		_gamePopupOptions = Screens.Instance.PushScreen<GamePopupOptions>(true);
		_gamePopupOptions.RefreshPlayerUsername();
		_gamePopupOptions.RefreshAuthState();
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
				stateMachine.PushState(new GameStateLogin());
				break;
			case ButtonId.OptionsSignOut:
				UserManager.Instance.loginManager.SignOut(SignOutSuccess);
				break;
			case ButtonId.OptionsChangePicture:
				ChangePicture();
				break;
			case ButtonId.OptionsChangeName:
				stateMachine.PushState(new GameStateChangeNickname());
				break;
			case ButtonId.OptionsManageAccount:
				stateMachine.PushState(new GameStateManageAccount());
				break;
			case ButtonId.OptionsSave:
				SaveSettings();
				Screens.Instance.PopScreen(_darkenedBg);
				stateMachine.PopState();
				break;
		}
	}

	private void SignOutSuccess()
	{
		_gamePopupOptions.RefreshAuthState();
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
		UserManager.Instance.SetPlayerSettings(hintsOn, musicOn);
	}

	public override void Disable()
	{
		Screens.Instance.PopScreen(_gamePopupOptions);
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}
}