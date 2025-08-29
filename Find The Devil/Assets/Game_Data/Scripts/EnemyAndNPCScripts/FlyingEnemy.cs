using UnityEngine;

public class FlyingObjectEnemy : EnemyBase
{
    public override void ReactToHit()
    {
        throw new System.NotImplementedException();
    }

    public override void Detect(){ }
    protected override void Die()
    {
        throw new System.NotImplementedException();
    }
    
}