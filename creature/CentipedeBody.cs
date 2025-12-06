using UnityEngine;
using System.Collections.Generic;

public class CentipedeBody : MonoBehaviour
{
    [Header("설정")]
    public List<Transform> bodyParts = new List<Transform>(); // 머리[0] ~ 꼬리[N]
    public float segmentDistance = 1.5f; // 마디 간격
    public float smoothSpeed = 10f;      // 따라가는 속도 (높을수록 뻣뻣, 낮을수록 흐물)

    // 머리 이동 테스트용 (나중에는 AI나 PlayerController로 대체)
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    void Update()
    {
        MoveHead(); // 머리 이동
        MoveBody(); // 몸통 따라가기
    }

    void MoveHead()
    {
        // 머리(0번)를 간단하게 키보드로 움직임
        Transform head = bodyParts[0];
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        head.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime);
        head.Rotate(Vector3.up * h * rotateSpeed * Time.deltaTime);
    }

    void MoveBody()
    {
        for (int i = 1; i < bodyParts.Count; i++)
        {
            Transform prevBody = bodyParts[i - 1]; // 앞 마디
            Transform currBody = bodyParts[i];     // 현재 마디

            // 1. 목표 위치 계산: 앞 마디의 '뒤쪽' 방향으로 간격만큼 떨어진 곳
            // (3D에서는 앞 마디가 바라보는 방향의 반대가 뒤쪽입니다)
            Vector3 targetPos = prevBody.position - (prevBody.forward * segmentDistance);

            // 2. 부드럽게 이동 (Lerp)
            currBody.position = Vector3.Lerp(currBody.position, targetPos, Time.deltaTime * smoothSpeed);

            // 3. 회전: 앞 마디를 바라보게 함
            // 현재 위치에서 앞 마디 위치를 바라보는 방향 벡터
            Vector3 lookDir = prevBody.position - currBody.position;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                // 회전도 부드럽게 (Slerp)
                currBody.rotation = Quaternion.Slerp(currBody.rotation, targetRot, Time.deltaTime * smoothSpeed);
            }
        }
    }
}