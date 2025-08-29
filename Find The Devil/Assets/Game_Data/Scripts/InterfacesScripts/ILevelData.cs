using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Splines;

public interface ILevelData
{
    int GetLevelId();
    List<EnemySpawnData> GetEnemySpawns();

    GameObject GetLevelPrefab();

    LevelType GetLevelType();

    int GetNumberOfEnemy();

    int GetLevelRewardCoins();

    int GetPlayerMovementSpeed();

}