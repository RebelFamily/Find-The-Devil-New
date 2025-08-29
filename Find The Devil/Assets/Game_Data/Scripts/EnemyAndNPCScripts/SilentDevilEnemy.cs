using UnityEngine;

public class SilentDevilEnemy : EnemyBase
{
    [SerializeField] private UrbanNPC possessedNPC;

    public override void Detect()
    {
     
        if (possessedNPC != null)
        {
            possessedNPC.ShowPossessionEffect(); 
        }
        
    }
    protected override void Die()
    {
        throw new System.NotImplementedException();
    }
    
    public override void ReactToHit() {  }
   
}