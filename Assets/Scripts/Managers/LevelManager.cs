using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public class LevelInformation
    {
        public int Version { get; }
        public int Width { get; }
        public int Height { get; }
        public string[] AvailableTiles { get;  }

        public LevelInformation(int version, int width, int height, string[] availableTiles)
        {
            Version = version;
            Width = width;
            Height = height;
            AvailableTiles = availableTiles;
        }
    }

    public LevelInformation LoadLevel(string resourceFile)
    {
        TextAsset myLevelData = (TextAsset)Resources.Load("Levels/" + resourceFile);
        if(myLevelData == null)
        {
            return null;
        }

        string[] levelData = myLevelData.text.Split('\n');
        if (levelData.Length == 0 || levelData[0].Trim() != "[BubbleBotsLevel]")
        {
            throw new System.Exception("Level data is not in the correct format");
        }

        if (levelData[1].Trim() != "version=1")
        {
            throw new System.Exception("Level data is in an uncompatible version");
        }

        string line;
        string[] data;
        string[] availableTiles = null;
        int width = 6;
        int height = 8;
        for (int i = 2; i < levelData.Length; i++)
        {
            line = levelData[i].Trim();
            if(string.IsNullOrEmpty(line))
            {
                continue;
            }

            data = line.Split('=');

            if(data.Length != 2)
            {
                throw new System.Exception("Level data seems corrupt (line content: " + line + ")");
            }

            switch(data[0])
            {
                case "dimensions":
                    data = data[1].Split(',');
                    try
                    {
                        width = int.Parse(data[0]);
                        height = int.Parse(data[1]);
                    } catch
                    {
                        throw new System.Exception("Could not parse dimensions: " + line);
                    }

                    break;
                case "available_tiles":
                    availableTiles = data[1].Split(',');
                    break;
                default:
                    Debug.LogWarning("Unknown item in save file: " + data[0]);
                    break;
            }
        }

        return new LevelInformation(1, width, height, availableTiles);
    }
}
