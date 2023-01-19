using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BubbleBots.Match3.Data
{
    [CreateAssetMenu(fileName = "Levels", menuName = "Bubble Bots/Create Level Design", order = 1)]
    public class LevelDesign : ScriptableObject
    {
        public List<LevelRow> rows;

        public void LoadFromJson(string path)
        {
            //Debug.Log(JsonUtility.ToJson(this));

            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            LevelDesign loaded = new LevelDesign();
            JsonUtility.FromJsonOverwrite(json, loaded);
            rows = loaded.rows;
        }
    }

    [System.Serializable]
    public class LevelRow
    {
        public List<string> row;
        public string this[int i]
        {
            get => row[i];
            set => row[i] = value;
        }
    }
}

