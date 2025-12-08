using UnityEngine;
using TMPro;

public class CreatureHUD : MonoBehaviour
{
    [Header("연결 대상")]
    public Creature targetCreature;

    [Header("설정")]
    public float padding = 10f; // 💡 박스와 생물 사이의 여백 (픽셀 단위)
    public float maxVisibleDistance = 50.0f; // 이 거리 밖이면 안 그림
    public float minVisibleDistance = 30f;

    [Header("박스 크기 제한")]
    public float maxBoxWidth = 600;  // 박스 가로 최대 크기 (픽셀)
    public float maxBoxHeight = 500;

    [Header("UI 컴포넌트")]
    public RectTransform creatureBoxRect; // 큰 박스
    public RectTransform statusBoxRect;   // 상태 박스
    public TMP_Text statusText;

    private Camera mainCam;
    private CanvasGroup canvasGroup;
    private Collider targetCollider; // 💡 생물의 크기를 잴 콜라이더

    // 3D 박스의 8개 모서리를 계산하기 위한 배열 미리 할당
    private Vector3[] corners = new Vector3[8];

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) mainCam = FindObjectOfType<Camera>();

        creatureBoxRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (statusBoxRect == null && transform.childCount > 0)
        {
            statusBoxRect = transform.GetChild(0).GetComponent<RectTransform>();
        }

        // 💡 타겟의 콜라이더 가져오기 (이게 있어야 크기를 잼)
        if (targetCreature != null)
        {
            targetCollider = targetCreature.GetComponent<Collider>();
        }
    }

    void LateUpdate()
    {
        // 타겟이 없거나 콜라이더가 없으면 숨김
        if (targetCreature == null || targetCollider == null)
        {
            canvasGroup.alpha = 0;
            return;
        }

        // 1. 거리 체크 (너무 멀면 안 그림)
        float dist = Vector3.Distance(mainCam.transform.position, targetCreature.transform.position);
        if (dist > maxVisibleDistance || dist < minVisibleDistance)
        {
            canvasGroup.alpha = 0;
            return;
        }

        // 2. 화면 뒤에 있는지 체크 (간단히 중심점으로)
        Vector3 centerScreenPos = mainCam.WorldToScreenPoint(targetCollider.bounds.center);
        if (centerScreenPos.z < 0)
        {
            canvasGroup.alpha = 0;
            return;
        }

        canvasGroup.alpha = 1;

        // 3. 상태 텍스트 갱신
        if (statusText != null)
        {
            statusText.text = targetCreature.creature_statues ?? "";
        }

        // =========================================================
        // 💡 핵심: 3D Bounds를 2D 화면 사각형으로 변환
        // =========================================================
        Bounds b = targetCollider.bounds;

        // 3D 박스의 8개 모서리 좌표 계산
        corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
        corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
        corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
        corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
        corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
        corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
        corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
        corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

        // 화면상에서 최소/최대 x, y 찾기
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < 8; i++)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(corners[i]);

            if (screenPos.x < minX) minX = screenPos.x;
            if (screenPos.x > maxX) maxX = screenPos.x;
            if (screenPos.y < minY) minY = screenPos.y;
            if (screenPos.y > maxY) maxY = screenPos.y;
        }

        if (minX == float.MaxValue)
        {
            canvasGroup.alpha = 0;
            return;
        }

        // 4. UI 박스 크기 및 위치 적용
        float width = maxX - minX + (padding * 2);
        float height = maxY - minY + (padding * 2);
        width = Mathf.Min(maxX - minX + (padding * 2), maxBoxWidth);
        height = Mathf.Min(maxY - minY + (padding * 2), maxBoxHeight);

        creatureBoxRect.sizeDelta = new Vector2(width, height);
        creatureBoxRect.position = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);

        // 5. 상태 박스는 몸통 박스 바로 위에 붙임
        if (statusBoxRect != null)
        {
            // 몸통 박스 위쪽(maxY) + 여백(padding) 위치로 이동
            statusBoxRect.position = new Vector2((minX + maxX) / 2, maxY + padding + 20f);
        }
    }
}