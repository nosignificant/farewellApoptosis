using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("Base Settings")]
    public float speed = 3f;
    public float foodSpeed = 2f;
    public float fleeSpeed = 3f;
    public float attackSpeed = 4f;
    public float radius = 10f;
    public int runAway = 2;

    //인스펙터 창에서 각각의 생물 아이디 등록 
    public int CREATURE_ID;

    [Header("Food")]
    float nearestFoodDist;
    Food nearestFood;
    bool isEating = false;
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

    public List<int> enemyCreatureIDs = new List<int>();
    public List<int> friendCreatureIDs = new List<int>();
    public List<Creature> friends = new List<Creature>();

    GameObject nearestEnemy;
    float nearestEnemyDist;
    bool isAttacking = false;

    [Header("Room")]

    public Room currentRoom;

    protected virtual void Start()
    {
        PickWanderTarget();
    }

    protected virtual void Update()
    {
        CheckNearby();

        if (nearestEnemy != null)
        {
            EnemyAction();
        }
        else if (nearestFood != null)
        {
            foodAction();
        }
        else
        {
            Wander();
        }
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
            //콜라이더 반경 계산
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
                nearestEnemy = hit.gameObject; // 💡 GameObject 저장
            }
        }
        if (friendCreatureIDs.Contains(other.CREATURE_ID))
            if (!friends.contain(other)) friends.add(other);
    }

    // ---------------------- FOOD ACTION ------------------------

    public void foodAction()
    {
        if (nearestFood == null) return;
        if (isEating) return;

        float distance = Vector3.Distance(this.transform.position, nearestFood.transform.position);

        if (distance > 2.0f)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestFood.transform);
            Util.towards(this.transform, speed, dir, foodSpeed);
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

        while (foodToEat != null && foodToEat.foodHealth > 0)
        {
            //먹을 거 없으면 중단 
            if (foodToEat == null || !foodToEat.gameObject.activeInHierarchy) break;



            // 1. 이동/정지 로직
            if (distance > STOP_DISTANCE)
            {
                float distance = Vector3.Distance(this.transform.position, foodToEat.transform.position);
                Vector3 dir = Util.GetDirectionTo(this.transform, foodToEat.transform);
                Util.towards(this.transform, speed, dir, foodSpeed);
                yield return null; // 1프레임 대기 후
                continue; // 루프 처음으로 돌아감, 밑의 함수 실행 안함 
            }

            yield return StartCoroutine(OrbitAroundCenter(foodToEat.transform.position, 1.0f, 1.0f));

            // 거기까지 갔는데 없을 수도 있으니 또 확인
            if (foodToEat == null) break;

            // 3. 헬스 감소 및 뜸 들이기
            foodToEat.TakeBite(1);
            yield return new WaitForSeconds(damagePerSecond);
        }


        nearestFood = null;
        isEating = false; // 다시 추격 상태로 돌아갈 수 있게 상태 해제
        PickWanderTarget();
    }


    // ---------------------- ENEMY ACTION ------------------------
    public void EnemyAction()
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


    IEnumerator AttackEnemy()
    {
        isAttacking = true;

        while (nearestEnemy != null &&
               Vector3.Distance(transform.position, nearestEnemy.transform.position) > 3f)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestEnemy.transform);
            Util.towards(this.transform, speed, dir, attackSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (nearestEnemy != null)
        {
            Vector3 dir = Util.GetDirectionTo(this.transform, nearestEnemy.transform);
            Util.moveBack(this.transform, speed, dir, 2f);
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
        Util.towards(this.transform, speed, dir, 0.5f);

        if (Vector3.Distance(transform.position, wanderTarget.position) < 1f) // 💡 .position 접근
        {
            // 💡 Orbit 시작 (Vector3를 받도록 오버로드된 OrbitAroundCenter를 호출하는 래퍼 사용)
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
            float angle = rotationSpeed * timer;

            // 회전 벡터 계산
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 rotatedDirection = rotation * initialDirection;

            // 새로운 목표 위치 계산
            Vector3 targetPosition = centerPosition + rotatedDirection * orbitRadius;

            // 오브젝트를 새로운 목표 위치로 이동 (Util 함수를 사용할 경우)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

            timer += Time.deltaTime;
            yield return null;
        }

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

