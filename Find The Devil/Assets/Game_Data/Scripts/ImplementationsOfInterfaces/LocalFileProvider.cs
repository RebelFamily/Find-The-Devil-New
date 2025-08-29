using UnityEngine;
using System.IO;

public class LocalFileProvider : IDataProvider
{
    private string savePath;

    public LocalFileProvider()
    {
        savePath = Path.Combine(Application.persistentDataPath, "gamedata.json"); 
     
    }

    public void Save(string key, string data)
    {
        
        string filePath = Path.Combine(Application.persistentDataPath, $"{key}.json");
        File.WriteAllText(filePath, data);
    }

    string IDataProvider.Load(string key, string data)
    {
        return Load(key);
    }

    public string Load(string key)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{key}.json");
        if (File.Exists(filePath))
        {
            string data = File.ReadAllText(filePath);
            return data;
        }
        return null;
    }

    public void Delete(string key)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{key}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public bool HasKey(string key)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"{key}.json");
        return File.Exists(filePath);
    }

    public void Save(string key, int data)
    {
        throw new System.NotImplementedException();
    }

    public int Load(string key, int data)
    {
        throw new System.NotImplementedException();
    }
    
}