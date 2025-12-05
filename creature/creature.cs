using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("CREATURE ID")]
    public int CREATURE_ID;


    [Header("Base Settings")]
    public float speed = 10f;
    public float foodSpeed = 10f;
    public float fleeSpeed = 10f;
    public float attackSpeed = 10f;
    public float radius = 10f;
    public int runAway = 2;

    protected Rigidbody rb;

    [Header("Food")]
    protected float nearestFoodDist;
    protected Food nearestFood;
    protected bool isEating = false;
    public float eatingDuration = 1.0f;
    //1초에 한입씩 먹음 
    public float damagePerSecond = 1f;
    private Coroutine eatingCoroutine = null;

    [Header("Wander")]
    Transform wanderTarget;
    float wanderTimer = 0f;
    public float wanderInterval = 2f;
    public float wanderDistance = 5f;

    [Header("Enemy - friend")]

    public List<int> enemyCreatureIDs = new List<int>();
    public List<int> friendCreatureIDs = new List<int>();
    public List<Creature> interested = new List<Creature>();

    public List<Creature> friends = new List<Creature>();

    protected GameObject nearestEnemy;
    protected float nearestEnemyDist;
    protected bool isAttacking = false;

    [Header("Room")]

    public Room currentRoom;

    public string creature_statues = null;

    protected virtual void Start()
    {
        PickWanderTarget();
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update() { }


    // ---------------------- CHECK ------------------------
    public void CheckNearby()
    {
        nearestEnemy = null;
        friends.Clear();
        interested.Clear();

        nearestFoodDist = Mathf.Infinity;
        nearestEnemyDist = Mathf.Infinity;
        Collider[] hits;

        if (currentRoom == null || currentRoom.roomCollider == null)
        {
            hits = Physics.OverlapSphere(transform.position, radius);
        }
        else
        {
            //방 안에 있는 모든 생물 대상으로 함
            Vector3 center = currentRoom.roomCollider.bounds.center;
            Vector3 halfExtents = currentRoom.roomCollider.bounds.extents;
            hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity);
        }

        //콜라이더 내부에 들어온 오브젝트들에 따라 행동 결정
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            switch (hit.tag)
            {
                case "Food":
                    UpdateNearestFood(hit);
                    break;
                case "Creature":
                    UpdateNearCreature(hit);
                    break;
            }
        }
    }
    //제일 가까이 있는 음식 확인
    void UpdateNearestFood(Collider hit)
    {
        float dist = Vector3.Distance(transform.position, hit.transform.position);

        if (dist < nearestFoodDist)
        {
            nearestFoodDist = dist;
            nearestFood = hit.GetComponent<Food>();
        }
    }

    void UpdateNearCreature(Collider hit)
    {
        Creature other = hit.GetComponent<Creature>();
        if (other == null) return;
        if (other == this) return;
        if (enemyCreatureIDs.Contains(other.CREATURE_ID))
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            if (dist < nearestEnemyDist)
            {
                nearestEnemyDist = dist;
                nearestEnemy = hit.gameObject;
            }
        }
        else if (friendCreatureIDs.Contains(other.CREATURE_ID))
            if (!friends.Contains(other)) friends.Add(other);
            else { if (!interested.Contains(other)) interested.Add(other); }
    }

    // ---------------------- FOOD ACTION ------------------------

    public void foodAction()
    {
        if (nearestFood == null) return;
        if (isEating) return;
        //여기부터 가까이 있는 음식까지의 거리 
        float distance = Vector3.Distance(this.transform.position, nearestFood.transform.position);

        if (distance > 8)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestFood.transform);
            if (rb != null)
                Util.towards(rb, speed, dir);
        }
        else
        {
            isEating = true;
            creature_statues = "eating";
            StartCoroutine(EatFoodRoutine(nearestFood));
        }
    }

    // ---------------------- EatFoodRoutine ------------------------

    IEnumerator EatFoodRoutine(Food foodToEat)
    {
        // 💡 목표 거리 설정
        const float STOP_DISTANCE = 8f;
        float distance = 0f;

        while (foodToEat != null && foodToEat.foodHealth > 0)
        {
            if (foodToEat == null || !foodToEat.gameObject.activeInHierarchy) break;
            distance = Vector3.Distance(this.transform.position, foodToEat.transform.position);

            // 너무 멀면 일단 가까이감
            if (distance > STOP_DISTANCE)
            {
                Vector3 dir = Util.GetDirectionTo(this.transform, foodToEat.transform);
                Util.towards(rb, speed, dir);
                yield return null;
                continue; // 너무 멀리 있을 때 음식 먹는 행동은 아직 안함
            }

            // 거기까지 갔는데 없을 수도 있으니 또 확인
            if (foodToEat == null) break;

            foodToEat.TakeBite(1);
            yield return new WaitForSeconds(damagePerSecond);
        }


        nearestFood = null;
        isEating = false;
        PickWanderTarget();
    }


    // ---------------------- ENEMY ACTION ------------------------
    public void EnemyAction1()
    {
        if (nearestEnemy == null) return;

        Vector3 dirToEnemy = Util.GetDirectionTo(this.transform, nearestEnemy.transform);

        if (friends.Count >= runAway)
        {
            if (!isAttacking)
            {
                creature_statues = "attacking";
                StartCoroutine(AttackEnemy());
            }

        }
        else
        {
            creature_statues = "flee";
            Util.moveBack(this.transform, speed, dirToEnemy, fleeSpeed);
        }
    }

    public void EnemyAction2()
    {
        if (nearestEnemy == null) return;

        Vector3 dirToEnemy = Util.GetDirectionTo(this.transform, nearestEnemy.transform);

        if (!isAttacking)
        {
            creature_statues = "attacking";
            StartCoroutine(AttackEnemy());
        }
    }

    IEnumerator AttackEnemy()
    {
        isAttacking = true;

        while (nearestEnemy != null &&
               Vector3.Distance(transform.position, nearestEnemy.transform.position) > 3f)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestEnemy.transform);
            Util.towards(rb, attackSpeed, dir);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (nearestEnemy != null)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestEnemy.transform);
            Util.moveBack(rb, speed, dir);
        }

        isAttacking = false;
    }





    // ---------------------- WANDER ACTION ------------------------


    protected void Wander()
    {

        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval)
        {
            wanderTimer = 0f;
            PickWanderTarget();
        }

        Vector3 dir = Util.GetDirectionTo(this.transform, wanderTarget);

        // 💡 수정 1: transform 대신 rb를 전달하여 물리 이동 사용 (벽 뚫기 방지)
        if (rb != null)
            Util.towards(this.rb, speed * 0.5f, dir);
    }


    // ---------------------- PickWanderTarget ------------------------

    protected virtual void PickWanderTarget()
    {
        if (wanderTarget != null) Destroy(wanderTarget.gameObject);

        if (currentRoom == null || currentRoom.roomCollider == null) return;

        Vector3 center = currentRoom.roomCollider.bounds.center;
        Vector3 extents = currentRoom.roomCollider.bounds.extents;

        int attempts = 0;
        const int maxAttempts = 10;

        // 💡 1. 탐색의 기준점(Pivot) 정하기
        Vector3 searchPivot = transform.position; // 기본값: 내 주변 배회

        // 관심 대상이 있고, 60% 확률에 당첨되면 기준점을 바꿈
        if (interested.Count > 0 && Random.value < 0.6f)
        {
            Creature targetCreature = interested[Random.Range(0, interested.Count)];

            if (targetCreature != null)
            {
                searchPivot = targetCreature.transform.position;
            }
        }

        do
        {
            // 💡 2. 기준점(searchPivot) 주변에서 랜덤 위치 생성
            Vector3 randDir = Random.insideUnitSphere.normalized;
            Vector3 potentialTarget = searchPivot + randDir * wanderDistance;

            // 3. 경계 검사 (로직 동일)
            Vector3 minBounds = center - extents;
            Vector3 maxBounds = center + extents;

            bool isInsideBounds =
                potentialTarget.x >= minBounds.x && potentialTarget.x <= maxBounds.x &&
                potentialTarget.y >= minBounds.y && potentialTarget.y <= maxBounds.y &&
                potentialTarget.z >= minBounds.z && potentialTarget.z <= maxBounds.z;

            // 4. (추가) 만약 관심 대상을 보러 가는데, 그 위치가 방 밖이라면?
            // -> 다시 내 주변을 찾도록 searchPivot을 초기화하고 재시도하게 할 수도 있음 (선택 사항)

            if (isInsideBounds)
            {
                GameObject targetObject = new GameObject("WanderTarget_" + currentRoom.roomID);
                targetObject.transform.position = potentialTarget;
                wanderTarget = targetObject.transform;
                return;
            }

            attempts++;
        } while (attempts < maxAttempts);

        // 실패 시 (Fallback)
        GameObject fallbackObject = new GameObject("WanderTarget_Fallback_" + currentRoom.roomID);
        fallbackObject.transform.position = center;
        wanderTarget = fallbackObject.transform;
    }

}

