using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("CREATURE ID")]
    public int CREATURE_ID;


    [Header("Base Settings")]
    public float speed = 3f;
    public float foodSpeed = 2f;
    public float fleeSpeed = 3f;
    public float attackSpeed = 4f;
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
    private bool isOrbitingWander = false;
    private Coroutine wanderOrbitCoroutine = null;

    [Header("Enemy - friend")]

    protected List<int> enemyCreatureIDs = new List<int>();
    protected List<int> friendCreatureIDs = new List<int>();
    protected List<Creature> interested = new List<Creature>();

    public List<Creature> friends = new List<Creature>();

    protected GameObject nearestEnemy;
    protected float nearestEnemyDist;
    protected bool isAttacking = false;

    [Header("Room")]

    public Room currentRoom;

    protected virtual void Start()
    {
        PickWanderTarget();
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    protected virtual void Update()
    {
        CheckNearby();
    }


    // ---------------------- CHECK ------------------------
    public void CheckNearby()
    {
        nearestEnemy = null;
        friends.Clear();

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
            else interested.Add(other);
    }

    // ---------------------- FOOD ACTION ------------------------

    public void foodAction()
    {
        if (nearestFood == null) return;
        if (isEating) return;
        //여기부터 가까이 있는 음식까지의 거리 
        float distance = Vector3.Distance(this.transform.position, nearestFood.transform.position);

        if (distance > 2.0f)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestFood.transform);
            if (rb != null)
                Util.towards(rb, speed, dir);
        }
        else
        {
            isEating = true;
            StartCoroutine(EatFoodRoutine(nearestFood));
        }
    }

    // ---------------------- EatFoodRoutine ------------------------

    IEnumerator EatFoodRoutine(Food foodToEat)
    {
        // 💡 목표 거리 설정
        const float STOP_DISTANCE = 1.5f;
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

            yield return StartCoroutine(OrbitAroundCenter(foodToEat.transform.position, 1.0f, 1.0f));

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
                StartCoroutine(AttackEnemy());
        }
        else
        {
            Util.moveBack(this.transform, speed, dirToEnemy, fleeSpeed);
        }
    }

    public void EnemyAction2()
    {
        if (nearestEnemy == null) return;

        Vector3 dirToEnemy = Util.GetDirectionTo(this.transform, nearestEnemy.transform);

        if (!isAttacking)
            StartCoroutine(AttackEnemy());
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

    protected virtual void PickWanderTarget()
    {
        if (wanderTarget != null) Destroy(wanderTarget.gameObject);

        if (currentRoom == null || currentRoom.roomCollider == null) return;

        // 💡 수정: currentRoom.roomCollider 사용
        Vector3 center = currentRoom.roomCollider.bounds.center;
        Vector3 extents = currentRoom.roomCollider.bounds.extents;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            // ... (potentialTarget 계산 로직은 동일) ...
            Vector3 randDir = Random.insideUnitSphere.normalized;
            Vector3 potentialTarget = transform.position + randDir * wanderDistance;

            // ... (경계 검사 로직은 동일) ...
            Vector3 minBounds = center - extents;
            Vector3 maxBounds = center + extents;

            bool isInsideBounds =
                potentialTarget.x >= minBounds.x && potentialTarget.x <= maxBounds.x &&
                potentialTarget.y >= minBounds.y && potentialTarget.y <= maxBounds.y &&
                potentialTarget.z >= minBounds.z && potentialTarget.z <= maxBounds.z;

            if (isInsideBounds)
            {
                // 💡 핵심 수정: 임시 GameObject를 생성하고 Transform을 할당합니다.
                GameObject targetObject = new GameObject("WanderTarget_" + currentRoom.roomID);
                targetObject.transform.position = potentialTarget;
                wanderTarget = targetObject.transform;
                return; // 유효한 목표 발견, 종료
            }

            attempts++;
        } while (attempts < maxAttempts);

        // 10회 시도 후에도 실패하면, 경계 중심으로 목표 설정
        GameObject fallbackObject = new GameObject("WanderTarget_Fallback_" + currentRoom.roomID);
        fallbackObject.transform.position = center;
        wanderTarget = fallbackObject.transform;
    }

    // ---------------------- WANDER ACTION ------------------------

    // Eater.cs Wander() 함수 수정

    void Wander()
    {
        if (isOrbitingWander) return;

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
        else
            Util.towards(this.transform, speed, dir, 0.5f);


        if (Vector3.Distance(transform.position, wanderTarget.position) < 1f)
        {
            isOrbitingWander = true;
            wanderOrbitCoroutine = StartCoroutine(WanderOrbitRoutine(wanderTarget.position, 1f, 1f));
        }
    }
    // ---------------------- ORBIT ACTION ------------------------

    public IEnumerator OrbitAroundCenter(Vector3 centerPosition, float duration, float orbitRadius = 2f)
    {
        float timer = 0f;

        // 현재 위치에서 중심 위치까지의 방향 벡터
        Vector3 initialDirection = (transform.position - centerPosition).normalized;

        float rotationSpeed = 360f / duration;

        while (timer < duration)
        {
            float angle = rotationSpeed * timer; // (Time.deltaTime 곱하지 않음: 시간 경과에 따른 누적 각도)

            // 회전 벡터 계산
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 rotatedDirection = rotation * initialDirection;

            // 새로운 목표 위치 계산
            Vector3 targetPosition = centerPosition + rotatedDirection * orbitRadius;

            // 💡 수정 2: transform.position 대입 대신 Rigidbody.MovePosition 사용
            if (rb != null)
            {
                // 부드럽게 이동하기 위해 Lerp로 다음 위치 계산 후 물리 이동
                Vector3 nextPos = Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * speed);
                rb.MovePosition(nextPos);
            }
            else
            {
                // 리지드바디 없을 때 (기존 방식)
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Orbit 종료 후에는 자연스러운 연결을 위해 yield break
        yield break;
    }

    IEnumerator WanderOrbitRoutine(Vector3 center, float duration, float radius)
    {
        yield return StartCoroutine(OrbitAroundCenter(center, duration, radius));
        isOrbitingWander = false;
        wanderOrbitCoroutine = null;
        PickWanderTarget(); // 새로운 Wander 목표 설정
    }



}

