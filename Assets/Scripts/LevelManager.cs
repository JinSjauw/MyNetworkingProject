using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string levelID;
    [SerializeField] private Transform enviroment;
    [SerializeField] private Transform spawnPoints;
    
    public List<StaticCollider> GetColliders()
    {
        List<StaticCollider> colliderList = new List<StaticCollider>();
        foreach (Transform child in enviroment)
        {
            Collider collider = child.GetComponent<BoxCollider>();
            Vector3 colliderPosition = child.position;
            Vector3 colliderSize = collider.bounds.size;
            StaticCollider staticCollider =
                new StaticCollider(colliderPosition.x, colliderPosition.z, 
                    colliderSize.x, colliderSize.z);
            colliderList.Add(staticCollider);
        }

        return colliderList;
    }

    public List<System.Numerics.Vector2> GetSpawnPoints()
    {
        List<System.Numerics.Vector2> spawnList = new List<System.Numerics.Vector2>();
        foreach (Transform child in spawnPoints)
        {
            Vector3 spawnPoint = child.position;
            System.Numerics.Vector2 spawn = new System.Numerics.Vector2(spawnPoint.x, spawnPoint.z);
            spawnList.Add(spawn);
        }

        return spawnList;
    }

    public void CreateLevelFile()
    {
        //Take all objects in list and save position + rotation + width & height
        //Read Level JSON in Server.
        LevelData levelData = new LevelData(GetColliders(), GetSpawnPoints());
        string json = JsonConvert.SerializeObject(levelData);
        Debug.Log(json);
        string fileName = "Level_"+ levelID + "_Data";

        //File.WriteAllText(@"C:\Users\jinzh\Desktop\Level_0_Data.json", JsonConvert.SerializeObject(levelData));
        using (StreamWriter file = File.CreateText(@"Assets\Data\" + fileName))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, levelData);
        }
        
        JObject jsonObj = JObject.Parse(File.ReadAllText(@"Assets\Data\" + fileName));
        
        Debug.Log(jsonObj.Properties());
        LevelData jsonData = (LevelData)jsonObj.ToObject(typeof(LevelData));
        Debug.Log(jsonData.colliders.Count);
        Debug.Log(jsonData.spawnPoints.Count);
    }

}

[Serializable]
public class LevelData
{
    public List<StaticCollider> colliders;
    public List<System.Numerics.Vector2> spawnPoints;

    public LevelData(List<StaticCollider> _colliders, List<System.Numerics.Vector2> _spawnPoints)
    {
        colliders = _colliders;
        spawnPoints = _spawnPoints;
    }
}

[Serializable]
public class StaticCollider
{
    public float x;
    public float y;

    public float width;
    public float height;

    public StaticCollider(float _x, float _y, float _width, float _height)
    {
        x = _x;
        y = _y;
        width = _width;
        height = _height;
    }
}
