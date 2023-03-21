using System.Collections;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using UnityEngine;
using UnityEngine.Networking;

public class NFTManager : MonoBehaviour
{
	public Sprite guardianBadge;
	public Sprite guardianFrame;
	public Sprite guardianLabel;
	public Sprite guardianBg;
	public Sprite hunterBadge;
	public Sprite hunterFrame;
	public Sprite hunterLabel;
	public Sprite hunterBg;
	public Sprite builderBadge;
	public Sprite builderFrame;
	public Sprite builderLabel;
	public Sprite builderBg;
	
	private NFTFile _nftFile;
	private List<NFTImage> NFTImagesAvailable = new List<NFTImage>();

	private void Awake()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("metadata");
		string text = textAsset.text;
		_nftFile = JsonUtility.FromJson<NFTFile>(text);
	}

	private IEnumerator GetNftImage(NFTImage image)
	{
		using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(image.url))
		{
			yield return uwr.SendWebRequest();
			if (uwr.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(uwr.error);
			}
			else
			{
				var texture = DownloadHandlerTexture.GetContent(uwr);
				Sprite nftImage = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
				image.sprite = nftImage;
				image.loaded = true;
			}
		}
	}

	private void GetNftsSuccess(string result)
	{
		GetPlayerNftsResponse response = JsonUtility.FromJson<GetPlayerNftsResponse>(result);
		foreach (NftDataResponse nftDataResponse in response.images)
		{
			NFTImage nftImage = new NFTImage()
			{
				url = nftDataResponse.name,
				tokenId = nftDataResponse.tokenId,
				loaded = false,
				sprite = null
			};
			NFTImagesAvailable.Add(nftImage);
			StartCoroutine(GetNftImage(nftImage));
		}
	}

	public void DownloadPLayerNfts()
	{
		NFTImagesAvailable = new List<NFTImage>();
		ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.GetOwnedNFTs, GetNftsSuccess, UserManager.Instance.GetPlayerWalletAddress());
	}

	public List<NFTImage> GetAvailableNfts()
	{
		return NFTImagesAvailable;
	}

	public NFTData GetCorrectNFTFromTokenId(int tokenId)
	{
		string token = tokenId.ToString();
		return _nftFile.nfts.Find((nft) => nft.edition == tokenId);
	}
}