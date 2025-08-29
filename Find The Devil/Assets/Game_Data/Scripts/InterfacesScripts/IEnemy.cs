using UnityEngine;

public interface IEnemy
{
  EnemyType GetEnemyType();
  void TakeDamage();
  void Spawn(Vector3 position, Quaternion rotation);
  public void ReactToHit();
}