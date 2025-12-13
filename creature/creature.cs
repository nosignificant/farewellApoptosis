using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("CREATURE ID")]
    public int CREATURE_ID;

    [Header("Base Settings")]
    public float speed;
    public float foodSpeed;
    public float fleeSpeed;
    public float attackSpeed;
    public float radius = 10f;
    public int runAway = 2;
    public int MAX_HP;
    public int currentHP;

    protected Rigidbody rb;

    [Header("Food")]
    protected float nearestFoodDist;
    protected Creature nearestFood;
    protected bool isEating = false;
    public float eatingDuration = 1.0f;
    //1초에 한입씩 먹음 
    public float damagePerSecond = 1f;
    [Header("Wander")]
    GameObject wanderTarget;
    float wanderTimer = 0f;
    public float wanderInterval = 2f;
    public float wanderDistance = 5f;

    [Header("Enemy - friend")]

    public List<int> enemyCreatureIDs = new List<int>();
    public List<int> friendCreatureIDs = new List<int>();
    public List<int> foodCreatureIDs = new List<int>();
    public List<int> interestedCreatureIDs = new List<int>();

    public List<Creature> friends = new List<Creature>();
    public List<Creature> interested = new List<Creature>();


    protected Creature nearestEnemy;
    protected float nearestEnemyDist;
    protected bool isAttacking = false;

    [Header("Room")]

    public Room currentRoom;

    [Header("UI Settings")]
    public GameObject uiPrefab;
    public float uiOffsetY = 2.0f;
    private CreatureHUD myHUD;

    public string statues = null;

    public enum CreatureState
    {
        Wandering,
        Eating,
        Attacking,
        Fleeing
    }


    protected virtual void Start()
    {
        PickWanderTarget();
        CreateHUD();
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentHP = MAX_HP;
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
            UpdateNearCreature(hit);
        }
    }

    protected void UpdateNearCreature(Collider hit)
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
                nearestEnemy = other;
            }
        }
        else if (friendCreatureIDs.Contains(other.CREATURE_ID))
        {
            if (!friends.Contains(other)) friends.Add(other);

        }
        else if (interestedCreatureIDs.Contains(other.CREATURE_ID))
        {
            if (!interested.Contains(other)) interested.Add(other);

        }
        else if (foodCreatureIDs.Contains(other.CREATURE_ID))
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            if (dist < nearestEnemyDist)
            {
                nearestEnemyDist = dist;
                nearestFood = other;
            }
        }
    }

    // ---------------------- FOOD ACTION ------------------------

    protected virtual void foodAction()
    {
        if (nearestFood == null || isEating) return;

        isEating = true;

        // 💡 공통 코루틴 실행: "음식한테 가서 -> 먹어라(TakeBite)"
        StartCoroutine(ChaseAndInteractRoutine(nearestFood.gameObject, 8f, () =>
        {
            nearestFood.TakeDamage(1);
        }));
    }

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{name} took {damage} damage. HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        Destroy(gameObject);
        if (currentRoom != null)
        {
            currentRoom.OnCreatureExit(this);
        }
    }


    // ---------------------- ENEMY ACTION ------------------------
    public void EnemyAction1()
    {
        if (nearestEnemy == null) return;
        isAttacking = true;

        Vector3 dirToEnemy = Util.GetDirectionTo(this.transform, nearestEnemy.transform);

        if (friends.Count >= runAway)
        {
            StartCoroutine(ChaseAndInteractRoutine(nearestEnemy.gameObject, 3f, () =>
            {
                nearestEnemy.TakeDamage(1);
            }));

        }
        else
        {
            Util.moveBack(this.transform, speed, dirToEnemy, fleeSpeed);
        }
    }

    public void EnemyAction2()
    {
        if (nearestEnemy == null) return;

        StartCoroutine(ChaseAndInteractRoutine(nearestEnemy.gameObject, 3f, () =>
                {
                    nearestEnemy.TakeDamage(1);
                }));
    }
    protected IEnumerator ChaseAndInteractRoutine(GameObject target, float stopDist, System.Action onReachAction)
    {
        while (target != null && target.activeInHierarchy)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);

            // 1. 거리가 멀면 추격
            if (distance > stopDist)
            {
                Vector3 dir = Util.GetDirectionTo(transform, target.transform);
                Util.towards(rb, speed, dir); // 이동 속도는 상황에 따라 speed 변수를 조절하거나 인자로 받아도 됨
                yield return null;
                continue;
            }

            // 2. 도착했으면 행동 실행 (먹기 or 공격)
            // (Orbit 같은 연출이 필요하면 여기에 추가)

            onReachAction?.Invoke(); // 💡 여기서 구체적인 행동을 실행!

            // 3. 딜레이 (공격 속도 or 먹는 속도)
            yield return new WaitForSeconds(damagePerSecond); // 변수명은 attackInterval 등으로 일반화하는 게 좋음
        }

        // 행동 종료 후 정리
        isEating = false;
        isAttacking = false;
        PickWanderTarget();
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

        if (wanderTarget == null) return;

        Vector3 dir = Util.GetDirectionTo(this.transform, wanderTarget.transform);

        // 💡 수정 1: transform 대신 rb를 전달하여 물리 이동 사용 (벽 뚫기 방지)
        if (rb != null)
            Util.towards(this.rb, speed * 0.5f, dir);
        if (Vector3.Distance(transform.position, wanderTarget.transform.position) < 1.0f)
        {
            // 도착했으니 삭제하고, 타이머도 리셋해서 즉시 다음 행동 준비
            Destroy(wanderTarget);
            wanderTarget = null;
            wanderTimer = wanderInterval; // 다음 프레임에 즉시 PickWanderTarget 호출되게 함
        }
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
                potentialTarget.x >= minBounds.x + 10 && potentialTarget.x <= maxBounds.x - 10 &&
                potentialTarget.y >= minBounds.y + 10 && potentialTarget.y <= maxBounds.y + 10 &&
                potentialTarget.z >= minBounds.z + 10 && potentialTarget.z <= maxBounds.z + 10;

            if (isInsideBounds)
            {
                // 💡 2. 새 오브젝트 생성 및 위치 할당
                wanderTarget = new GameObject("WanderTarget_" + currentRoom.roomID);
                wanderTarget.transform.position = potentialTarget;
                return; // 생성 완료, 종료
            }
            attempts++;
        } while (attempts < maxAttempts);

        // 실패 시 (Fallback)
        wanderTarget = new GameObject("WanderTarget_Fallback_" + currentRoom.roomID);
        wanderTarget.transform.position = center;
    }

    protected void UpdateStatusString()
    {
        CreatureState currentState = GetCurrentState();
        statues = currentState.ToString();
    }

    public CreatureState GetCurrentState()
    {
        if (isEating) return CreatureState.Eating;
        if (isAttacking) return CreatureState.Attacking;

        // 도망 조건
        if (nearestEnemy != null && friends.Count < runAway) return CreatureState.Fleeing;

        return CreatureState.Wandering; // 기본값
    }

    // ---------------------- CreateHUD ------------------------

    void CreateHUD()
    {
        if (uiPrefab == null) return;
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();

        if (canvas != null)
        {
            GameObject hudObj = Instantiate(uiPrefab, canvas.transform);
            myHUD = hudObj.GetComponent<CreatureHUD>();
            myHUD.targetCreature = this;

            if (myHUD != null)
            {
                myHUD.targetCreature = this;
            }
        }
        else
        {
            Debug.LogError("씬에 Canvas가 없습니다! UI를 생성할 수 없습니다.");
        }
    }
    public bool IsInScreenCenter(Vector2 playerScreenPos)
    {
        if (myHUD == null) return false;
        float padding = 50f;

        if (playerScreenPos.x >= myHUD.minX - padding && playerScreenPos.x <= myHUD.maxX + padding &&
            playerScreenPos.y >= myHUD.minY - padding && playerScreenPos.y <= myHUD.maxY + padding)
        {
            return true;
        }
        return false;
    }

}

