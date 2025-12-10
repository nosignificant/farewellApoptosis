using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 10.0f;
    public float jumpForce = 5.0f;
    public LayerMask groundLayer;

    [Header("엔더 진주 설정")]
    public Throw pearlPrefab;
    public Transform throwPoint;

    [Header("던지기 설정")]
    public float minThrowForce = 10f;
    public float maxThrowForce = 40f;
    public float maxChargeTime = 2.0f; // 💡 1초 이상 눌러야 하니까 최대 시간도 좀 늘려두는 게 좋습니다.

    [Header("안전 장치 (새로 추가됨)")]
    [Tooltip("이 시간보다 짧게 누르면 던지지 않고 취소됩니다.")]
    public float minHoldTime = 1.0f; // 💡 1초 미만 클릭은 무시

    [Tooltip("이 거리 이상 날아가면 공중에서 터지고 이동함")]
    public float maxTeleportRange = 30.0f;

    [Header("공중 제한 설정")]
    public int maxAirThrows = 2;
    private int currentAirThrows = 0;

    [Header("시선 설정")]
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    public Transform cameraTransform;

    Rigidbody rb;
    Vector2 rotation = Vector2.zero;
    bool isGrounded = false;
    float currentChargeTime = 0f;
    bool isCharging = false;
    private static bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (throwPoint == null) throwPoint = cameraTransform;

        rotation.y = transform.eulerAngles.y;
        if (cameraTransform != null) rotation.x = cameraTransform.localEulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 0f;
    }

    void Update()
    {
        if (!canMove) return;

        RotationLogic();
        CheckGround();
        HandleThrow();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        MoveLogicSnappy();
    }

    void HandleThrow()
    {
        // 1. 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            if (isGrounded || currentAirThrows < maxAirThrows)
            {
                isCharging = true;
                currentChargeTime = 0f;
            }
        }

        // 2. 누르는 중
        if (isCharging && Input.GetMouseButton(0))
        {
            currentChargeTime += Time.deltaTime;
        }

        // 3. 손을 뗐을 때 (조건 검사)
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            // 💡 [핵심] 누른 시간이 1초(minHoldTime)보다 길어야만 발사!
            if (currentChargeTime >= minHoldTime)
            {
                ThrowPearl();
            }
            else
            {
                // 1초 미만이면 그냥 취소 (아무 일도 안 일어남)
                // Debug.Log("너무 짧게 눌러서 취소됨");
            }

            isCharging = false;
            currentChargeTime = 0f;
        }
    }

    void ThrowPearl()
    {
        if (pearlPrefab == null) return;

        if (!isGrounded)
        {
            currentAirThrows++;
        }

        float ratio = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        float finalForce = Mathf.Lerp(minThrowForce, maxThrowForce, ratio);

        Throw pearl = Instantiate(pearlPrefab, throwPoint.position + throwPoint.forward, Quaternion.identity);

        Vector3 throwDir = cameraTransform.forward;
        pearl.Initialize(this, throwDir * finalForce, maxTeleportRange);
    }

    public void TeleportTo(Vector3 position)
    {
        rb.MovePosition(position);

        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;
    }

    void MoveLogicSnappy()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camFwd = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camFwd.y = 0; camRight.y = 0;
        camFwd.Normalize(); camRight.Normalize();

        Vector3 moveDir = (camFwd * v + camRight * h).normalized;
        Vector3 targetVel = moveDir * moveSpeed;
        targetVel.y = rb.linearVelocity.y;

        rb.linearVelocity = targetVel;
    }

    void RotationLogic()
    {
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);

        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(rotation.x, 0f, 0f);
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.1f, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            currentAirThrows = 0;
        }
        else if (isGrounded)
        {
            currentAirThrows = 0;
        }
    }

    public static void setPlayerMove(bool onOff)
    {
        canMove = onOff;
        if (onOff == false) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}