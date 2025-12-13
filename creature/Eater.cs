using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eater : Creature
{
    protected override void Start()
    {
        if (currentRoom != null) currentRoom.OnCreatureEnter(this);
        base.Start();
    }
    public bool isFull = false;
    protected override void Update()
    {
        base.CheckNearby();

        if (nearestEnemy != null)
        {
            base.EnemyAction2();
        }
        else if (nearestFood != null)
        {
            foodAction();
        }
        else
        {
            base.Wander();
        }
        base.UpdateStatusString();
    }

    protected override void foodAction()
    {
        base.foodAction();
        isFull = true;
    }
}

