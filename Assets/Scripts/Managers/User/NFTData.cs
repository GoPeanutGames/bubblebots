using System;
using System.Collections.Generic;

[Serializable]
public class NFTTrait
{
	public string trait_type;
	public string value;
}

[Serializable]
public class NFTData
{
	public int edition;
	public List<NFTTrait> attributes;
}

[Serializable]
public class NFTFile
{
	public List<NFTData> nfts;
}