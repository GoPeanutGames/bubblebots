using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile", menuName = "Bubble Bots/Create Game Tile", order = 1)]
public class GameTile : ScriptableObject
{
    public Sprite Tile;
    public string Key = "1";
}
