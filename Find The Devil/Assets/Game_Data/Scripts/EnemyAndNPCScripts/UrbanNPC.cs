using System;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
public class UrbanNPC : MonoBehaviour
{
   
    private Random _mathRandom; 
    private quaternion _originalDevilRotation; // NEW: Field to store the original rotation

    private Animator humanAnimator;
    private Animator skeletonAnimator;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        CharacterReactionHandler.OnReactWhenNPCDie += Die;
    }

    private void OnDisable()
    {
        CharacterReactionHandler.OnReactWhenNPCDie -= Die;
    }


    private  void Die(GameObject obj)
    {
        GameManager.Instance.LevelFail();
    }

    public void ShowPossessionEffect()
    {
       
    }
    
    
}