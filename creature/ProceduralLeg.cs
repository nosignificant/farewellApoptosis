using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ProceduralLeg : MonoBehaviour
{
    [Header("연결 대상")]
    public Transform bodyCenter; // 점1: 연결될 몸통 (부모)

    [Header("다리 설정")]
    public Vector3 defaultOffset = new Vector3(1, -1, 0); // 몸통 기준 다리 목표 위치 (오른쪽 아래 등)
    public float stepDistance = 1.0f; // 발을 떼는 한계 거리
    public float stepHeight = 0.5f;   // 발을 들 때 높이
    public float stepSpeed = 10f;     // 발 옮기는 속도

    // 내부 상태 변수
    private Vector3 currentFootPos; // 현재 발 끝 위치 (실제 보이는 곳)
    private Vector3 oldFootPos;     // 방금 전 발 위치
    private Vector3 newFootPos;     // 이동할 목표 발 위치
    private float lerp = 1;         // 발 이동 진행도 (0~1)

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // 점 2개 (몸통 -> 발끝)

        // 초기화
        currentFootPos = transform.position + defaultOffset;
        newFootPos = currentFootPos;
        oldFootPos = currentFootPos;
    }

    void Update()
    {
        // 1. 다리가 닿아야 할 이상적인 위치 계산 (Raycast)
        // 몸통 위치에서 설정한 오프셋 방향으로 레이를 쏩니다.
        Vector3 targetWorldPos = bodyCenter.TransformPoint(defaultOffset);
        Vector3 rayOrigin = bodyCenter.position;
        Vector3 rayDir = (targetWorldPos - bodyCenter.position).normalized;

        RaycastHit hit;
        // 몸통에서 다리 방향으로 레이를 쏴서 벽이나 땅을 찾음
        if (Physics.Raycast(rayOrigin, rayDir, out hit, 5f))
        {
            // 벽에 닿았으면 그 지점이 목표
            targetWorldPos = hit.point;
        }

        // 2. 발을 옮겨야 하는지 검사
        // 현재 목표지점과 실제 발 위치가 너무 멀어지면 스텝 시작
        if (Vector3.Distance(newFootPos, targetWorldPos) > stepDistance && lerp >= 1)
        {
            lerp = 0; // 이동 시작
            oldFootPos = currentFootPos; // 출발점 저장
            newFootPos = targetWorldPos; // 도착점 저장
        }

        // 3. 발 이동 애니메이션 (곡선 그리기)
        if (lerp < 1)
        {
            lerp += Time.deltaTime * stepSpeed;

            // 0~1 사이 값을 S자 곡선으로 부드럽게 만듦
            float smoothLerp = Mathf.SmoothStep(0, 1, lerp);

            // 직선 이동 위치
            Vector3 footPos = Vector3.Lerp(oldFootPos, newFootPos, smoothLerp);

            // 위로 살짝 들기 (Sin 그래프 이용: 0 -> 1 -> 0)
            footPos.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentFootPos = footPos;
        }
        else
        {
            // 이동 중이 아니면 목표 지점에 고정
            currentFootPos = newFootPos;
        }

        // 4. LineRenderer로 시각화 (점1 -> 점2 연결)
        DrawLeg();
    }

    void DrawLeg()
    {
        // 점 1: 몸통 중심 (또는 어깨 위치)
        lineRenderer.SetPosition(0, bodyCenter.position);

        // 점 2: 계산된 발 끝 위치
        lineRenderer.SetPosition(1, currentFootPos);
    }
}