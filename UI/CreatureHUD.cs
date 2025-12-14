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
    public TMP_Text statusText;

    private Camera mainCam;
    private PlayerLockOn playerLockOn;
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
        if (Player.Instance != null)
            playerLockOn = Player.Instance.GetComponent<PlayerLockOn>();

        creatureBoxRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (targetCreature != null)
        {
            targetCollider = targetCreature.GetComponent<Collider>();
        }
    }

    void LateUpdate()
    {
        if (targetCreature == null || targetCollider == null)
        {
            Destroy(gameObject);
            return; // 함수 종료
        }

        CalculateBoxCoordinates();

        if (playerLockOn != null && playerLockOn.targetCreature == this.targetCreature)
        {
            canvasGroup.alpha = 1;
            ApplyRectSize();
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }
    void CalculateBoxCoordinates()
    {
        Bounds b = targetCollider.bounds;

        // 초기화 필수
        minX = float.MaxValue; maxX = float.MinValue;
        minY = float.MaxValue; maxY = float.MinValue;

        // 8개 모서리 좌표 갱신 (기존 코드 복붙)
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
            if (screenPos.z < 0) continue;

            if (screenPos.x < minX) minX = screenPos.x;
            if (screenPos.x > maxX) maxX = screenPos.x;
            if (screenPos.y < minY) minY = screenPos.y;
            if (screenPos.y > maxY) maxY = screenPos.y;
        }
    }

    void ApplyRectSize()
    {
        if (minX == float.MaxValue || maxX == float.MinValue) return;

        float rawWidth = (maxX - minX) / 2;
        float rawHeight = (maxY - minY) / 2;
        float finalWidth = Mathf.Clamp(rawWidth, 0, maxBoxWidth);
        float finalHeight = Mathf.Clamp(rawHeight, 0, maxBoxHeight);

        creatureBoxRect.sizeDelta = new Vector2(finalWidth, finalHeight);

        // 중심점 구하기
        Vector3 screenCenter = mainCam.WorldToScreenPoint(targetCollider.bounds.center);
        creatureBoxRect.position = screenCenter;

        if (statusText != null) statusText.text = targetCreature.statues ?? "";
    }
}