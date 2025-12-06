using UnityEngine;

public class Convey : Eater
{
    [Header("Convey 설정")]
    public float roomChangeChance = 0.2f; // 50% 확률로 방 이동
    public int howManyAte = 0;
    protected override void Start()
    {
        currentRoom = Util.FindCurrentRoom(transform.position);
        base.Start();
        if (currentRoom != null) currentRoom.OnCreatureEnter(this);
    }

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
        base.UpdateStatusString();
    }

    protected override void foodAction()
    {
        if (nearestFood is Eater eater)
        {
            if (eater.isFull) base.foodAction();
            howManyAte++;
            if (CurrentSentient.Instance != null)
                CurrentSentient.Instance.UpdateSentient(CREATURE_ID, howManyAte);
        }
    }
    protected override void PickWanderTarget()
    {
        if (currentRoom == null) return;

        if (currentRoom.connectedRooms.Count > 0 && Random.value < roomChangeChance)
        {
            int randomIndex = Random.Range(0, currentRoom.connectedRooms.Count);
            Room nextRoom = currentRoom.connectedRooms[randomIndex];
            //방에서 나갔다고 알림
            currentRoom.OnCreatureExit(this);
            //이 생물 자체 변수값 바꿔줌
            currentRoom = nextRoom;
            currentRoom.OnCreatureEnter(this);
            Debug.Log($"{name} decided to move to {currentRoom.roomID}");
        }

        base.PickWanderTarget();
    }
}