using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TMPro는 여기서 필요 없지만, 이전 코드에 남아있어 남겨둡니다.
using UnityEngine.UI; // UnityEngine.UI는 여기서 필요 없지만, 이전 코드에 남아있어 남겨둡니다.

public class Eater : Creature
{
    protected override void Update()
    {
        base.CheckNearby();

        if (nearestEnemy != null)
        {
            base.EnemyAction2();
        }
        else if (nearestFood != null)
        {
            base.foodAction();
        }
        else
        {
            base.Wander();
        }
    }
}

