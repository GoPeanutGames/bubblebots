using System.Collections;
using System.Collections.Generic;
using BubbleBots.Server.Player;
using BubbleBots.User;
using UnityEngine;
using UnityEngine.Networking;

public class NFTManager : MonoBehaviour
{
	private List<NFTImage> NFTImagesAvailable = new List<NFTImage>();

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
		ServerManager.Instance.GetPlayerDataFromServer(PlayerAPI.GetOwnedNFTs, GetNftsSuccess, UserManager.Instance.GetPlayerWalletAddress());
	}

	public List<NFTImage> GetAvailableNfts()
	{
		return NFTImagesAvailable;
	}
}