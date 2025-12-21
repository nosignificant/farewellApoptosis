using System.Collections.Generic;
using UnityEngine;

public class SimpleFollowSnake : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform target;
    public float moveSpeed = 5f;
    public float stopDistance = 0.5f; // 목표 근처에 오면 멈춤 (필수)

    [Header("몸통 설정")]
    public List<Transform> bodyParts = new List<Transform>();
    public float bodySpacing = 0.8f;
    public float followSpeed = 10f;

    void Update()
    {
        MoveHead();
        MoveBody();
    }

    void MoveHead()
    {
        if (target == null) return;

        // 거리 체크: 너무 가까우면 움직이지 않음 (발작 방지)
        if (Vector3.Distance(transform.position, target.position) > stopDistance)
        {
            // 1. 그냥 목표 바라보기 (복잡한 계산 X, 즉시 회전)
            transform.LookAt(target);

            // 2. 앞으로 전진
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    void MoveBody()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            // 내 앞의 놈 (0번이면 머리, 아니면 앞 몸통)
            Transform leader = (i == 0) ? transform : bodyParts[i - 1];
            Transform current = bodyParts[i];

            // 1. 위치 이동: 앞 놈의 등 뒤(spacing 만큼) 위치로 그냥 이동
            Vector3 targetPos = leader.position - (leader.forward * bodySpacing);
            current.position = Vector3.Lerp(current.position, targetPos, Time.deltaTime * followSpeed);

            // 2. 회전: 계산 안 함. 그냥 앞 놈 회전값을 그대로 베낌. (이러면 절대 안 꼬임)
            current.rotation = Quaternion.Lerp(current.rotation, leader.rotation, Time.deltaTime * followSpeed);
        }
    }
}