using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements; // Not directly used in the provided snippet, can be removed if not needed elsewhere
using Random = Unity.Mathematics.Random;

public class DevilEnemy : EnemyBase
{
    
    // [SerializeField] private GameObject humanForm; 
    // [SerializeField] private GameObject devilForm;

   
    private Animator humanAnimator;
    private Animator devilAnimator;
    
    private Random _mathRandom; 
    private quaternion _originalDevilRotation; // NEW: Field to store the original rotation

    private bool isDetected = false;

    private void OnEnable()
    {
       //check CharacterReactionHandler.OnReactWhenEnemyDie += CheckDeath;
    }

    private void OnDisable()
    {
        // checkCharacterReactionHandler.OnReactWhenEnemyDie -= CheckDeath;
    }

    protected override void Awake()
    {
       
    }

    public override void Detect()
    {
        if (!isDetected)
        {
            isDetected = true;
           
        }
        
    }

    public override void ReactToHit()
    {
        
    }

    void CheckDeath(GameObject obj) 
    {
       Die();
    }
    protected override void Die()
    {
      
        GameManager.Instance.economyManager.AddCoins(10); 
        //GameManager.Instance.levelManager.UpdateEnemyCount();
    }
    
}