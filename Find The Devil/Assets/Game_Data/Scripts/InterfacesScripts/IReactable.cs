using System;
using System.Collections;

public interface IReactable
{
    void ReactToHit();
    public bool ReactingToHit();

    IEnumerator ReactWhenEnemyDie();

    IEnumerator ReactWhenNPCDie();
}