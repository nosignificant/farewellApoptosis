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
    public CanvasGroup canvasGroup;
    public Collider targetCollider;

    // 3D 박스의 8개 모서리를 계산하기 위한 배열 미리 할당
    private Vector3[] corners = new Vector3[8];
    public float minX = float.MaxValue; public float maxX = float.MinValue;
    public float minY = float.MaxValue; public float maxY = float.MinValue;

    private Vector3 centerScreenPos;

    void Start()
    {
        mainCam = Camera.main;

        creatureBoxRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (statusBoxRect == null && transform.childCount > 0)
        {
            statusBoxRect = transform.GetChild(0).GetComponent<RectTransform>();
        }
        if (targetCreature != null)
        {
            targetCollider = targetCreature.GetComponent<Collider>();
        }
    }

    void LateUpdate()
    {
        if (targetCreature == null || targetCollider == null)
        {
            canvasGroup.alpha = 0;
            return;
        }

        // 1. 거리 및 화면 뒤 체크 pos - pos = dist 
        float dist = Vector3.Distance(mainCam.transform.position, targetCreature.transform.position);
        // 중심좌표 // world >> screen 좌표 , 스크린 좌표로 바꿔놨기때문에 z좌표는 카메라랑 거리 
        centerScreenPos = mainCam.WorldToScreenPoint(targetCollider.bounds.center);

        // 거리가 너무 멀거나, 카메라 뒤에 있으면 숨김
        if (dist > maxVisibleDistance || dist < minVisibleDistance || centerScreenPos.z < 0)
        {
            canvasGroup.alpha = 0;
            return;
        }

        canvasGroup.alpha = 1;
        if (statusText != null) statusText.text = targetCreature.creature_statues ?? "";

        Bounds b = targetCollider.bounds;

        minX = float.MaxValue;
        maxX = float.MinValue;
        minY = float.MaxValue;
        maxY = float.MinValue;

        // 8개 모서리 좌표
        corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
        corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
        corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
        corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
        corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
        corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
        corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
        corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);


        for (int i = 0; i < 8; i++)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(corners[i]);

            // [중요] 모서리 중 하나라도 카메라 뒤로 넘어가면 계산이 튀는 것을 방지
            if (screenPos.z < 0) continue;

            if (screenPos.x < minX) minX = screenPos.x;
            if (screenPos.x > maxX) maxX = screenPos.x;
            if (screenPos.y < minY) minY = screenPos.y;
            if (screenPos.y > maxY) maxY = screenPos.y;
        }

        // 유효한 좌표가 없으면 리턴
        if (minX == float.MaxValue || maxX == float.MinValue) return;

        // 1. 실제 계산된 크기
        float rawWidth = (maxX - minX) / 2;
        float rawHeight = (maxY - minY) / 2;

        float finalWidth = Mathf.Clamp(rawWidth, 0, maxBoxWidth);
        float finalHeight = Mathf.Clamp(rawHeight, 0, maxBoxHeight);

        // 3. UI 적용
        creatureBoxRect.sizeDelta = new Vector2(finalWidth, finalHeight);

        // 위치는 항상 물체의 중심을 따라가게 설정 (크기가 제한되어도 위치는 정확히 따라감)
        Vector3 screenCenter = mainCam.WorldToScreenPoint(b.center);
        creatureBoxRect.position = screenCenter;
    }
}