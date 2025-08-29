using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Splines;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game Data/Level Data")]
public class LevelData : ScriptableObject, ILevelData
{
    [SerializeField] private int levelId;
    [SerializeField] private LevelType _levelType;
    [SerializeField] private GameObject levelPrefab;
    [SerializeField] private List<EnemySpawnData> enemySpawns;
    [SerializeField] private int numberOfEnemy;
    [SerializeField] private int playerMovementSpeed;
    [SerializeField] private int levelRewardCoins;
  //  [SerializeField] private SplineData _splineData;
    
    public int GetLevelId()
    {
        return levelId;
    }
    
    public List<EnemySpawnData> GetEnemySpawns()
    {
        return enemySpawns;
    }

    public GameObject GetLevelPrefab()
    {
        return levelPrefab;
    }

    public LevelType GetLevelType()
    {
        return _levelType;
    }

    public int GetNumberOfEnemy()
    {
        return numberOfEnemy;
    }

    public int GetLevelRewardCoins()
    {
        return levelRewardCoins;
    }

    public int GetPlayerMovementSpeed()
    {
        return playerMovementSpeed;
    }

    
}

// Helper struct for level spawn data
[System.Serializable]
public struct EnemySpawnData
{
    public EnemyType enemyType;
    public Vector3 position;
    public Quaternion rotation;
}

