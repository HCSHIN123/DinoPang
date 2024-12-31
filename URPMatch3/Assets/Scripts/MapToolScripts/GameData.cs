using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
public class GameData : MonoBehaviour
{

    public int[,] boardData = new int[9, 9];


    
    public void InitData()
    {
        boardData = LoadFromJson();
    }
    public void SaveToJson(int[,] data)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "data.json");
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(filePath, json);
        boardData = data;
    }
    public int[,] LoadFromJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "data.json");
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<int[,]>(json);
    }

    
    
    
}
