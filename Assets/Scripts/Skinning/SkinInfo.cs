using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinInfo : MonoBehaviour
{
    public GameTile[] TileSet;

    public Sprite FindSpriteFromKey(string key)
    {
        for (int i = 0; i < TileSet.Length; i++)
        {
            if (TileSet[i].Key == key)
            {
                return TileSet[i].Tile;
            }
        }

        return null;
    }
}
