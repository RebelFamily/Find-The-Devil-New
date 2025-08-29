using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IEnemy
{
    [SerializeField] protected int scoreValue = 10; 

    protected string EnemyID;
    public EnemyType _EnemyType; 
  
    public bool enemyIsAProp;
    [SerializeField] protected RuntimeAnimatorController baseAnimator;

    
    
    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(EnemyID))
        {
            EnemyID = gameObject.name;
        }

       

    }

    protected abstract void Die();

    protected void AwardScore()
    {
        
    }


    public EnemyType GetEnemyType()
    {
        return _EnemyType;
    }

    public void TakeDamage()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
    }

    public abstract void ReactToHit();
    

    public abstract void Detect();
}